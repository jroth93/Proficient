using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using OperationCanceledException = Autodesk.Revit.Exceptions.OperationCanceledException;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class HardBreak : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var sel = uiDoc.Selection.GetElementIds();
        ElementId eId;
        XYZ pt;

        try
        {
            eId = sel.Count > 0 ? sel.First() : uiDoc.Selection.PickObject(ObjectType.Element, "Pick Element").ElementId;
            pt = uiDoc.Selection.PickPoint();
        }
        catch (OperationCanceledException)
        {
            return Result.Cancelled;
        }

        var el = doc.GetElement(eId);

        using var tx = new Transaction(doc, "Hard Break");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;

        switch (el)
        {
            case Pipe:
                PlumbingUtils.BreakCurve(doc, eId, pt);
                break;
            case Duct:
                MechanicalUtils.BreakCurve(doc, eId, pt);
                break;
        }
        
        tx.Commit();

        return Result.Succeeded;
    }
}