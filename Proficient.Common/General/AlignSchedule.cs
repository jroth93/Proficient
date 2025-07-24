using Autodesk.Revit.UI.Selection;
using Proficient.Forms;

namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class AlignScheduleL : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        return AlignSchedule.Align(revit, true);
    }
}

[Transaction(TransactionMode.Manual)]
internal class AlignScheduleR : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        return AlignSchedule.Align(revit, false);
    }
}

internal class AlignSchedule
{
    public static Result Align(ExternalCommandData revit, bool left)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = doc.GetElement(uiDoc.ActiveView.Id) as View;
            
        var prompt = new Follower(revit);

        try
        {
            prompt.lbl1.Content = "Pick Anchor Schedule";
            var anchor = doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element, "Pick Anchor Schedule"));
            var aBb = anchor.get_BoundingBox(view);
            var aHt = aBb.Max.Y - aBb.Min.Y;
            if (anchor is not ScheduleSheetInstance aSsi)
                return Result.Failed;
            var aPt = aSsi.Point;
            var aName = doc.GetElement(aSsi.ScheduleId).Name.ToLower();
                
            prompt.lbl1.Content = "Pick Schedule To Adjust";
            var dependent = doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element, "Pick Schedule To Adjust"));
            var dBb = dependent.get_BoundingBox(view);
            var dHt = dBb.Max.Y - dBb.Min.Y;
            if(dependent is not ScheduleSheetInstance dSsi)
                return Result.Failed;
            var dPt = dSsi.Point;
            var dName = doc.GetElement(dSsi.ScheduleId).Name.ToLower();

            double x, y;
            var adjDir = aName.Contains("plumbing") ? 1 : -1;

            if (aName.Contains("plumbing") && dName.Contains("plumbing"))
            {
                var adjDir2 = aName.Contains("plumbing fixture") ? 1 : -1;

                x = aPt.X - adjDir2 * 13.0 / 128 / 12;
                y = aPt.Y + adjDir2 * 23.0 / 64 / 12;
            }
            else if (aName.Contains("plumbing fixture") || dName.Contains("plumbing fixture"))
            {
                x = aPt.X - adjDir * 219.0 / 256 / 12;
                y = aPt.Y + adjDir * 23.0 / 64 / 12;
            }
            else if(aName.Contains("plumbing specialties") || dName.Contains("plumbing specialties"))
            {
                x = aPt.X - adjDir * 193.0 / 256 / 12;
                y = aPt.Y;
            }
            else
            {
                x = aPt.X;
                y = aPt.Y;
            }

            if (!left && doc.GetElement(aSsi.ScheduleId) is ViewSchedule aVs && doc.GetElement(dSsi.ScheduleId) is ViewSchedule dVs)
                x = x + aVs.GetTableData().Width - dVs.GetTableData().Width;

            y = dPt.Y > aPt.Y ? 
                y + dHt + 43.0 / 256 / 12 : 
                y - aHt - 43.0 / 256 / 12;

            var endPt = new XYZ(x, y, aPt.Z);

            using var tx = new Transaction(doc, "Align Schedules");
            if (tx.Start() == TransactionStatus.Started)
                dSsi.Point = endPt;

            tx.Commit();
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            prompt.Close();
            return Result.Succeeded;
        }
        catch
        {
            prompt.Close();
            return Result.Failed;
        }

        prompt.Close();
        return Result.Succeeded;
    
    }
}