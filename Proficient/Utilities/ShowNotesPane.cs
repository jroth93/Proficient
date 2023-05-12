namespace Proficient.Utilities;

[Transaction(TransactionMode.Manual)]
internal class ShowNotesPane : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        revit.Application.GetDockablePane(Forms.NotesPane.PaneId).Show();

        return Result.Succeeded;
    }
}