using Proficient.Utilities;

namespace Proficient.Toggles;

[Transaction(TransactionMode.Manual)]
internal class ScheduleWarningToggle : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        Main.Settings.SuppressSchWarning = !Main.Settings.SuppressSchWarning;

        var warningState = Main.Settings.SuppressSchWarning ? "off" : "on";

        Util.BalloonTip("Schedule Warning", $"Type schedule warning is {warningState}", "");

        return Result.Succeeded;
    }
}