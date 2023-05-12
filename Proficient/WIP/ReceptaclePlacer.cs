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
        Space sp = doc.GetElement(sel.GetElementIds().First()) as Space;
        var bsList = sp.GetBoundarySegments(new SpatialElementBoundaryOptions())[0].Select(x => x.GetCurve() as Curve);
        double per = bsList.Select(x => x.Length).Sum();

        View view = doc.GetElement(uiDoc.ActiveView.Id) as View;

        using (Transaction tx = new Transaction(doc, "commandname"))
        {
            if (tx.Start() == TransactionStatus.Started)
            {
                foreach (Line l in bsList)
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