using Proficient.Ductulator;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class DuctLauncher : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        new DuctMain().Show();

        return Result.Succeeded;
    }
}