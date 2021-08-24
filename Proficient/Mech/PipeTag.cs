using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Proficient.Forms;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PipeTag : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ElementId viewId = uidoc.ActiveView.Id;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            IList<Element> pipes = new FilteredElementCollector(doc, viewId)
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .ToElements();
            IEnumerable<Element> fittings =
                new FilteredElementCollector(doc, viewId)
                .OfCategory(BuiltInCategory.OST_PipeFitting)
                .OfClass(typeof(FamilyInstance))
                .Where(f => f.LookupParameter(Names.Parameter.FittingUpDn) != null);

            if (pipes.Count == 0 && fittings.Count() == 0)
            {
                return Result.Succeeded;
            }
            
            #region get leader preference
            Blank frm = new Blank();
            frm.sp.Orientation = Orientation.Horizontal;

            Button btnLdr = new Button();
            btnLdr.Content = "Leader";
            frm.sp.Children.Add(btnLdr);

            Label spacer = new Label();
            spacer.Content = " ";
            frm.sp.Children.Add(spacer);

            Button btnNoLdr = new Button();
            btnNoLdr.Content = "No Leader";
            frm.sp.Children.Add(btnNoLdr);
            
            bool ldr = true;
            
            frm.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Rectangle mwe = revit.Application.MainWindowExtents;
                frm.Left = (mwe.Left + mwe.Right) / 2 - frm.Width / 2;
                frm.Top = (mwe.Top + mwe.Bottom) / 2 - frm.Height / 2;
            };

            btnLdr.Click += (object sender, RoutedEventArgs e) =>
            {
                frm.DialogResult = true;
                ldr = true;
                frm.Close();
            };

            btnNoLdr.Click += (object sender, RoutedEventArgs e) =>
            {
                frm.DialogResult = true;
                ldr = false;
                frm.Close();
            };

            if (!frm.ShowDialog() ?? false)
            {
                return Result.Cancelled;
            }

            #endregion
            
            #region tag pipes
            using (Transaction tx = new Transaction(doc, "Add pipe tags"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (Element pipe in pipes)
                    {
                        Reference pRef = new Reference(pipe);
                        LocationCurve locCrv = pipe.Location as LocationCurve;
                        bool longEnough = locCrv.Curve.Length > 3;

                        XYZ ep1 = locCrv.Curve.GetEndPoint(0);
                        XYZ ep2 = locCrv.Curve.GetEndPoint(1);

                        bool isVert = Math.Round(ep1.X, 4) == Math.Round(ep2.X, 4);
                        bool isHor = Math.Round(ep1.Y, 4) == Math.Round(ep2.Y, 4);
                        bool isInPlane = Math.Round(ep1.Z, 4) == Math.Round(ep2.Z, 4);

                        if (longEnough && isInPlane && !Util.IsTagged(doc, viewId, pipe))
                        {
                            XYZ point = locCrv.Curve.Evaluate(0.5, true);
                            TagOrientation tagOr = !ldr && isVert ? TagOrientation.Vertical : TagOrientation.Horizontal;

                            ElementId symId = (new FilteredElementCollector(doc)
                                .OfClass(typeof(Family))
                                .First(f => f.Name == Names.Family.PipeTag) as Family)
                                .GetFamilySymbolIds()
                                .First();

                            if (!isVert && !isHor && !ldr)
                            {
                                symId = (new FilteredElementCollector(doc)
                                .OfClass(typeof(Family))
                                .First(f => f.Name == Names.Family.PipeTagRotating) as Family)
                                .GetFamilySymbolIds()
                                .First();
                            }

                            IndependentTag tag = IndependentTag.Create(doc, symId, viewId, pRef, ldr, tagOr, point);
                        }
                    }
                }
                tx.Commit();
            }
            #endregion

            #region tag pipe drops/rises
            using (Transaction tx = new Transaction(doc, "Add fitting tags"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    ElementId symId = (new FilteredElementCollector(doc)
                        .OfClass(typeof(Family))
                        .First(fam => fam.Name == Names.Family.PipeFittingTag) as Family)
                        .GetFamilySymbolIds()
                        .First();

                    foreach (FamilyInstance f in fittings)
                    {
                        MechanicalFitting mf = f.MEPModel as MechanicalFitting;
                        if (!Util.IsTagged(doc, view.Id, f))
                        {
                            foreach (Connector c in mf.ConnectorManager.Connectors)
                            {
                                if (c.CoordinateSystem.BasisZ.Z == 1)
                                {
                                    foreach (Connector cr in c.AllRefs)
                                    {
                                        if (cr.Owner.Location is LocationCurve lc)
                                        {
                                            Curve crv = lc.Curve;
                                            double top = Math.Max(crv.GetEndPoint(0).Z, crv.GetEndPoint(1).Z);
                                            double viewTop = Util.GetViewBound(doc, view, Util.ViewPlane.Top);
                                            if (viewTop - top > 70)
                                            {
                                                viewTop -= 100;
                                            }
                                            if (top > viewTop)
                                            {
                                                Reference fRef = new Reference(f);
                                                XYZ point = (f.Location as LocationPoint).Point;
                                                IndependentTag tag = IndependentTag.Create(doc, symId, viewId, fRef, true, TagOrientation.Horizontal, point);
                                            }
                                        }
                                    }
                                }
                                else if (c.CoordinateSystem.BasisZ.Z == -1)
                                {
                                    foreach (Connector cr in c.AllRefs)
                                    {
                                        if (cr.Owner.Location is LocationCurve lc)
                                        {
                                            Curve crv = lc.Curve;
                                            double bottom = Math.Min(crv.GetEndPoint(0).Z, crv.GetEndPoint(1).Z);
                                            double viewBottom = Util.GetViewBound(doc, view, Util.ViewPlane.Bottom);
                                            if (viewBottom - bottom > 70)
                                            {
                                                viewBottom -= 100;
                                            }
                                            if (bottom < viewBottom)
                                            {
                                                Reference fRef = new Reference(f);
                                                XYZ point = (f.Location as LocationPoint).Point;
                                                IndependentTag tag = IndependentTag.Create(doc, symId, viewId, fRef, true, TagOrientation.Horizontal, point);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    tx.Commit();
                }
            }
            #endregion

            return Result.Succeeded;
        }
    }
}
