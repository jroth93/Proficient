using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Proficient.Archive
{
    [Transaction(TransactionMode.Manual)]
    class GraphKNLauncher : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            string pn = Util.GetProjectNumber(revit);

            MSGraph.OpenKNFile(pn);

            return Result.Succeeded;
        }
    }
}
