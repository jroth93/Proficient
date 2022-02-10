using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class AddLeader : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            IList<ElementId> ids = uidoc.Selection.GetElementIds() as IList<ElementId>;
            Element el = ids.Count() == 0 ? doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Element")) : doc.GetElement(ids[0]);

            List<ViewType> DrawingVTs = new List<ViewType>
            {
                ViewType.DraftingView,
                ViewType.DrawingSheet,
                ViewType.Legend
            };

            if(view.SketchPlane == null && !DrawingVTs.Contains(view.ViewType))
            {
                using (Transaction tx = new Transaction(doc, "Add Sketch Plane"))
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        view.SketchPlane = SketchPlane.Create(doc, view.GenLevel.Id);
                        tx.Commit();
                    }
                }
            }

            XYZ endPt = uidoc.Selection.PickPoint();

            FamilySymbol ldrFs = new FilteredElementCollector(doc)
                                        .OfClass(typeof(Family))
                                        .OfType<Family>()
                                        .First(f => f.Name.Equals(Names.Family.BlankLeader))?
                                        .GetFamilySymbolIds()
                                        .Select(id => doc.GetElement(id)).First() as FamilySymbol;

            string elType = (doc.GetElement(el.GetTypeId()) as FamilySymbol)?.Family.Name ??
                (doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName;

            XYZ loc = new XYZ();
            bool knt = false;
            bool txtEl = false;
            IndependentTag it = null;
            TextNote txt = null;
            int dir = 1;

            if (elType == Names.Family.KeynoteTag)
            {
                knt = true;
                it = el as IndependentTag;
                dir = Math.Sign(endPt.X - it.TagHeadPosition.X);
                loc = new XYZ(it.TagHeadPosition.X + dir * (Convert.ToDouble(view.Scale) * 157 / 1024 / 12), it.TagHeadPosition.Y, it.TagHeadPosition.Z);
            }
            else if (elType == "View Reference")
            {
                BoundingBoxXYZ bb = el.get_BoundingBox(view);
                dir = Math.Sign(endPt.X - (bb.Max.X + bb.Min.X) / 2);
                loc = new XYZ(
                    dir == 1 ? bb.Max.X : bb.Min.X,
                    (bb.Max.Y + bb.Min.Y) / 2 + Convert.ToDouble(view.Scale) * 1 / 16 / 12,
                    (bb.Max.Z + bb.Min.Z) / 2);
            }
            else if (elType == "Text")
            {
                txtEl = true;
                txt = (el as TextNote);
                dir = Math.Sign(endPt.X - txt.Coord.X);
            }
            else
            {
                BoundingBoxXYZ bb = el.get_BoundingBox(view);
                dir = Math.Sign(endPt.X - (bb.Max.X + bb.Min.X) / 2);
                loc = new XYZ(
                    dir == 1 ? bb.Max.X : bb.Min.X,
                    (bb.Max.Y + bb.Min.Y) / 2,
                    (bb.Max.Z + bb.Min.Z) / 2);
            }

            using (Transaction tx = new Transaction(doc, "Add Leader"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    if (txtEl)
                    {
                        Leader ldr = txt.AddLeader(dir == 1 ?
                            TextNoteLeaderTypes.TNLT_STRAIGHT_R :
                            TextNoteLeaderTypes.TNLT_STRAIGHT_L);
                        txt.HorizontalAlignment = dir == 1 ?
                            HorizontalTextAlignment.Right :
                            HorizontalTextAlignment.Left;
                        ldr.End = endPt;
                        ldr.Elbow = new XYZ(ldr.Anchor.X + dir * view.Scale / 96.0, ldr.Anchor.Y, ldr.Anchor.Z);
                    }
                    else
                    {
                        FamilyInstance fi = doc.Create.NewFamilyInstance(loc, ldrFs, view);
                        AnnotationSymbol fias = fi as AnnotationSymbol;
                        fias.addLeader();
                        Leader ldr = fias.GetLeaders()[0];
                        ldr.End = endPt;

                        if (knt && it.HasLeader && it.HasElbow)
                        {
                            ldr.Elbow = it.LeaderElbow;
                        }
                        
                    }
                    tx.Commit();
                }
            }

            return Result.Succeeded;

        }
    }
}