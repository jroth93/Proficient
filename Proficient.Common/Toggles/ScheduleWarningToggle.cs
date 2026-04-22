using Proficient.Utilities;

namespace Proficient.Toggles;

[Transaction(TransactionMode.Manual)]
internal class ScheduleWarningToggle : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        if (Main.Settings is null) return Result.Failed;
        Main.Settings.SuppressSchWarning = !Main.Settings.SuppressSchWarning;

        var warningState = Main.Settings.SuppressSchWarning ? "off" : "on";

        Util.BalloonTip("Schedule Warning", $"Type schedule warning is {warningState}", "");

        return Result.Succeeded;
    }
}