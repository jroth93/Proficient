namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class _Template : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var app = revit.Application;
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        using Transaction tx = new(doc, "commandname");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;

        //do stuff

        tx.Commit();

        return Result.Succeeded;
    }
}