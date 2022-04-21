using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ShowPane : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            revit.Application.GetDockablePane(Forms.NotesPane.PaneId).Show();

            return Result.Succeeded;
        }
    }
}
