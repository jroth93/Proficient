using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class DuctLauncher : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            DuctMain ductulator = new DuctMain();
            ductulator.Show();

            return Result.Succeeded;
        }
    }
}
