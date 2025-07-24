namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class ReloadFamily : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var ids = uiDoc.Selection.GetElementIds();

        if (ids.Any() && doc.GetElement(ids.First()) is FamilyInstance fi)
            BrowserUtility.ReloadFamily(doc, fi.Symbol.Family.Id);

        return Result.Succeeded;
    }
}