using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI.Selection;
using Proficient.Utilities;

namespace Proficient.WIP;

[Transaction(TransactionMode.Manual)]
internal class ReceptaclePlacer : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        UIApplication app = revit.Application;
        UIDocument uiDoc = revit.Application.ActiveUIDocument;
        Document doc = uiDoc.Document;
        Selection sel = uiDoc.Selection;
        if (doc.GetElement(sel.GetElementIds().First()) is not Space sp) 
            return Result.Failed;

        var bsList = sp.GetBoundarySegments(new SpatialElementBoundaryOptions())[0].Select(x => x.GetCurve());
        var per = bsList.Select(x => x.Length).Sum();

        var view = doc.GetElement(uiDoc.ActiveView.Id) as View;

        using (var tx = new Transaction(doc, "commandname"))
        {
            if (tx.Start() == TransactionStatus.Started)
            {
                foreach (Line l in bsList.Cast<Line>())
                {
                    doc.Create.NewDetailCurve(view, l);
                }
            }

            tx.Commit();
        }

        XYZ pl = uiDoc.Selection.PickPoint();

        Util.BalloonTip("", pl.ToString(), "");

        return Result.Succeeded;
    }
}