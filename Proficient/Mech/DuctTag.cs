using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Linq;


namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class DuctTag : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {

            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = revit.Application.ActiveUIDocument.Document;
            ElementId viewId = uidoc.ActiveView.Id;
            View view = doc.GetElement(viewId) as View;

            var ducts = new FilteredElementCollector(doc, viewId)
                .OfCategory(BuiltInCategory.OST_DuctCurves)
                .ToElements();
            var fittings =
                new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DuctFitting)
                .OfClass(typeof(FamilyInstance))
                .Where(f => f.LookupParameter(Names.Parameter.FittingUpDn) != null);

            if (ducts.Count == 0 && fittings.Count() == 0)
            {
                return Result.Succeeded;
            }

            #region tag ducts
            using (Transaction tx = new Transaction(doc, "Add duct tags"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (Element duct in ducts)
                    {
                        Reference dRef = new Reference(duct);
                        Location loc = duct.Location;
                        LocationCurve locCrv = loc as LocationCurve;
                        bool longEnough = locCrv.Curve.Length > 3;
                        double dWidth =
                            (duct as Duct).DuctType.Shape == ConnectorProfileType.Round ?
                            duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble() :
                            duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();
                        double minNoLdr = Convert.ToDouble(view.Scale) / 64.0;

                        XYZ ep1 = locCrv.Curve.GetEndPoint(0);
                        XYZ ep2 = locCrv.Curve.GetEndPoint(1);

                        bool isVert = Math.Round(ep1.X, 4) == Math.Round(ep2.X, 4);
                        bool isHor = Math.Round(ep1.Y, 4) == Math.Round(ep2.Y, 4);
                        bool isInPlane = Math.Round(ep1.Z, 4) == Math.Round(ep2.Z, 4);

                        if (longEnough && isInPlane && !Util.IsTagged(doc, viewId, duct))
                        {
                            XYZ point = locCrv.Curve.Evaluate(0.5, true) as XYZ;
                            bool ldr = false;
                            TagOrientation tagOr = TagOrientation.Horizontal;
                            ElementId symId = (new FilteredElementCollector(doc)
                                .OfClass(typeof(Family))
                                .First(f => f.Name == Names.Family.DuctTag) as Family)
                                .GetFamilySymbolIds()
                                .First();

                            if (isVert)
                            {
                                if (dWidth >= minNoLdr)
                                {
                                    tagOr = TagOrientation.Vertical;
                                }
                                else
                                {
                                    ldr = true;
                                }
                            }
                            else if (isHor && dWidth < minNoLdr)
                            {
                                ldr = true;
                            }
                            else
                            {
                                symId = (new FilteredElementCollector(doc)
                                    .OfClass(typeof(Family))
                                    .First(f => f.Name == Names.Family.DuctTagRotating) as Family)
                                    .GetFamilySymbolIds()
                                    .First();
                                if (dWidth < minNoLdr)
                                {
                                    ldr = true;
                                }
                            }

                            IndependentTag tag = IndependentTag.Create(doc, symId, viewId, dRef, ldr, tagOr, point);
                        }
                    }
                }
                tx.Commit();
            }
            #endregion

            #region tag duct drops/rises
            using (Transaction tx = new Transaction(doc, "Add fitting tags"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    Family tagFam = new FilteredElementCollector(doc)
                        .OfClass(typeof(Family))
                        .Where(fam => fam.Name == Names.Family.DuctFittingTag)
                        .FirstOrDefault() as Family;
                    ElementId symId = tagFam.GetFamilySymbolIds().FirstOrDefault();

                    foreach (FamilyInstance f in fittings)
                    {
                        MechanicalFitting mf = f.MEPModel as MechanicalFitting;
                        if (!Util.IsTagged(doc, view.Id, f) && mf.ConnectorManager != null)
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
