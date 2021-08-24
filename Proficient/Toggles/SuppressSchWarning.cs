using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SuppressSchWarning : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Main.Settings.suppressSchWarning = !Main.Settings.suppressSchWarning;

            string warningState = Main.Settings.suppressSchWarning ? "off" : "on";

            Util.BalloonTip("Schedule Warning", $"Type mark schedule warning is {warningState}", "");

            return Result.Succeeded;
        }
    }
}
