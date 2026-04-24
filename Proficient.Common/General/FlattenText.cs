namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class FlattenText : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var selIds = uiDoc.Selection.GetElementIds();
        if (selIds.Count <= 0) 
            return Result.Cancelled;

        var textEls = selIds
            .Select(doc.GetElement)
            .Where(el => el is TextElement)
            .Cast<TextElement>();

        using var tx = new Transaction(doc, "Remove Line Breaks");
        if (tx.Start() != TransactionStatus.Started) 
            return Result.Failed;

        foreach (var te in textEls)
            te.Text = te.Text.Replace("\r", " ").Replace("  ", " ");

        tx.Commit();
            
        return Result.Succeeded;
    }
}