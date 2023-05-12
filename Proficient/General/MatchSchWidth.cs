using Autodesk.Revit.UI.Selection;
using Proficient.Forms;

namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class MatchSchWidth : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var prompt = new Follower(revit);

        try
        {
            prompt.lbl1.Content = "Pick Anchor Schedule";
            var anchor = doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element, "Pick Anchor Schedule"));

            prompt.lbl1.Content = "Pick Schedule To Adjust";
            var dependent = doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element, "Pick Schedule To Adjust"));

            if (anchor is not ScheduleSheetInstance ai || dependent is not ScheduleSheetInstance di)
                return Result.Failed;

            if(doc.GetElement(ai.ScheduleId) is not ViewSchedule avs || doc.GetElement(di.ScheduleId) is not ViewSchedule dvs)
                return Result.Failed;
                
            var width = avs.GetTableData().Width;

            using var tx = new Transaction(doc, "Adjust Schedule Width");
            if (tx.Start() != TransactionStatus.Started)
                return Result.Failed;
            dvs.GetTableData().Width = width;
            tx.Commit();

            prompt.Close();
            return Result.Succeeded;
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            prompt.Close();
            return Result.Cancelled;
        }
        catch
        {
            prompt.Close();
            return Result.Failed;
        }
    }
}