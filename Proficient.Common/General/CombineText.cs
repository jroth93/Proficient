namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class CombineText : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        var selIds = uiDoc.Selection.GetElementIds();
        if (!selIds.Any()) 
            return Result.Cancelled; 

        var sortedEls = selIds
            .Select(doc.GetElement)
            .Where(el => el is TextElement)
            .Cast<TextElement>()
            .OrderByDescending(el => el.Coord.Y)
            .ThenBy(el => el.Coord.X)
            .ToList();

        if (!sortedEls.Any()) 
            return Result.Cancelled; 

        var combText = string.Empty;
        TextElement previous = null;
            
        foreach (var el in sortedEls)
        {
            if (sortedEls.IndexOf(el) == 0)
            {
                combText += el.Text;
                previous = el;
                continue;
            }
                
            if (previous != null && Math.Abs(el.Coord.Y - previous.Coord.Y) > el.Height * view.Scale)
            {
                combText += "\n" + el.Text;
            }
            else
            {
                combText += " " + el.Text;
            }
            previous = el;
        }
        combText = combText.Replace("\r", "");
            
        var width = sortedEls.Select(el => el.Width).OrderByDescending(el => el).First()*1.05;

        using var tx = new Transaction(doc, "Combine");
        try
        {
            if (tx.Start() != TransactionStatus.Started)
                return Result.Failed;
            
            TextNote.Create(doc, view.Id, sortedEls.First().Coord, width, combText, sortedEls.First().Symbol.Id);
            foreach (var el in sortedEls) 
                doc.Delete(el.Id);
            tx.Commit();
        }
        catch (NullReferenceException)
        {
            tx.RollBack();
        }

        return Result.Succeeded;

    }
}