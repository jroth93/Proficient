using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Proficient.Utilities
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ShowNotesPane : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            revit.Application.GetDockablePane(Forms.NotesPane.PaneId).Show();

            return Result.Succeeded;
        }
    }
}
