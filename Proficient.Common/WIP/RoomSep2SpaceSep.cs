using Autodesk.Revit.ApplicationServices;

namespace Proficient;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]

class RoomSep2SpaceSep : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        UIDocument uidoc = revit.Application.ActiveUIDocument;
        Document doc = revit.Application.ActiveUIDocument.Document;

        Application app = doc.Application;
        Autodesk.Revit.Creation.Application appcreation = app.Create;
        Autodesk.Revit.Creation.Document doccreation = doc.Create;

        ElementId viewid = uidoc.ActiveView.Id;
        var view = doc.GetElement(viewid) as View;
        FilteredElementCollector coll = new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(CurveElement)));
        var rslines = coll.Where(el => el.Category.Name == "<Room Separation>");

        using var tx = new Transaction(doc, "Change Division Lines");
        if (tx.Start() == TransactionStatus.Started)
        {
            foreach (Element rsline in rslines)
            {
                var skp = (rsline as CurveElement)?.SketchPlane;
                var curview = doc.GetElement((doc.GetElement(rsline.LevelId) as Level)?.FindAssociatedPlanViewId());
                var cclone = (rsline.Location as LocationCurve)?.Curve.Clone();
                var curvearr = new CurveArray();
                if (cclone != null)
                {
                    curvearr.Append(cclone);
                }
                if (skp != null && curview is View cv)
                {
                    doccreation.NewSpaceBoundaryLines(skp, curvearr, cv);
                }

            }
        }
        tx.Commit();
        return Result.Succeeded;
    }

}