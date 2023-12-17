using Autodesk.Revit.UI.Selection;
using Proficient.Utilities;

namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class AddLeader : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        if (view is View3D v3d && (!v3d.IsLocked || v3d.IsPerspective)) 
            return Result.Cancelled;
            

        var ids = uiDoc.Selection.GetElementIds();
        var el = ids.Count == 0 ? 
            doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element, "Pick Element")) : 
            doc.GetElement(ids.FirstOrDefault());

        var drawingVTs = new List<ViewType>
        {
            ViewType.DraftingView,
            ViewType.DrawingSheet,
            ViewType.Legend,
        };

        if(view.SketchPlane == null && !drawingVTs.Contains(view.ViewType))
        {
            using var sptx = new Transaction(doc, "Add Sketch Plane");
            if (sptx.Start() == TransactionStatus.Started)
            {
                view.SketchPlane = view is ViewSection ? 
                    SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin)) : 
                    SketchPlane.Create(doc, view.GenLevel.Id);
            }
            sptx.Commit();
        }


        var endPt = uiDoc.Selection.PickPoint();

        var ldrFs = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .OfType<Family>()
            .First(f => f.Name.Equals(Names.Family.BlankLeader))
            .GetFamilySymbolIds()
            .Select(id => doc.GetElement(id)).First() as FamilySymbol;

        var elType = (doc.GetElement(el.GetTypeId()) as FamilySymbol)?.Family.Name ??
                     (doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName;

        var loc = new XYZ();
        var knt = false;
        var txtEl = false;
        IndependentTag it = null;
        TextNote txt = null;
        int dir;

        switch (elType)
        {
            case Names.Family.KeynoteTag:
                knt = true;
                it = (IndependentTag)el;
                dir = Math.Sign(endPt.X - it.TagHeadPosition.X);
                loc = new XYZ(
                    it.TagHeadPosition.X + dir * (Convert.ToDouble(view.Scale) * 190 / 1024 / 12), 
                    it.TagHeadPosition.Y, 
                    it.TagHeadPosition.Z);
                break;
            case "View Reference":
            {
                var bb = el.get_BoundingBox(view);
                dir = Math.Sign(endPt.X - (bb.Max.X + bb.Min.X) / 2);
                loc = new XYZ(
                    dir == 1 ? bb.Max.X : bb.Min.X,
                    (bb.Max.Y + bb.Min.Y) / 2 + Convert.ToDouble(view.Scale) * 1 / 16 / 12,
                    (bb.Max.Z + bb.Min.Z) / 2 + Convert.ToDouble(view.Scale) * 1 / 16 / 12);
                break;
            }
            case "Text":
                txtEl = true;
                txt = (TextNote)el;
                dir = Math.Sign(endPt.X - txt.Coord.X);
                break;
            default:
            {
                var bb = el.get_BoundingBox(view);
                dir = Math.Sign(endPt.X - (bb.Max.X + bb.Min.X) / 2);
                loc = new XYZ(
                    dir == 1 ? bb.Max.X : bb.Min.X,
                    (bb.Max.Y + bb.Min.Y) / 2,
                    (bb.Max.Z + bb.Min.Z) / 2);
                break;
            }
        }

        using var tx = new Transaction(doc, "Add Leader");
        if (tx.Start() != TransactionStatus.Started) return Result.Failed;

        if (txtEl)
        {
            var ldr = txt.AddLeader(dir == 1 ?
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
            if (view.ViewType == ViewType.ThreeD) return Result.Cancelled;

            if (doc.Create.NewFamilyInstance(loc, ldrFs, view) is AnnotationSymbol fias)
            {
                fias.addLeader();
                var ldr = fias.GetLeaders()[0];
                ldr.End = endPt;

                if (knt && it.HasLeader)
                {
#if (FORGE && !R21)
                        Reference itRef = it.GetTaggedReferences()?.First();
                        if (it.HasLeaderElbow(itRef))
                            ldr.Elbow = it.GetLeaderElbow(itRef);
#else
                    if (it.HasElbow)
                        ldr.Elbow = it.LeaderElbow;
#endif
                }
            }
        }
        tx.Commit();

        return Result.Succeeded;

    }
}