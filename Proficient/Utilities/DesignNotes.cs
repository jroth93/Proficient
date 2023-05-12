namespace Proficient.Utilities;

internal class DesignNotes
{
    private static readonly Dictionary<View, ICollection<ElementId>> _designNoteViews = new();

    public static void Hide(Document doc, List<ElementId> printViews)
    {
        ElementMulticategoryFilter mcf = new(new List<BuiltInCategory> { BuiltInCategory.OST_TextNotes, BuiltInCategory.OST_Lines, BuiltInCategory.OST_Dimensions, BuiltInCategory.OST_GenericAnnotation });

        foreach (var id in printViews)
            if (doc.GetElement(id) is ViewSheet vs)
                printViews.AddRange(vs.GetAllPlacedViews());

        _designNoteViews.Clear();

        using Transaction tx = new (doc, "Hide Design Notes");
        if (tx.Start() != TransactionStatus.Started) return;

        foreach (var viewId in printViews)
        {
            if(doc.GetElement(viewId) is not View view) continue;

            var designEl = new FilteredElementCollector(doc, viewId)
                .WherePasses(mcf)
                .Concat(new FilteredElementCollector(doc).OfClass(typeof(IndependentTag)))
                .Where(el =>
                    el.Name.ToLower().Contains("design") ||
                    el is CurveElement ce && ce.LineStyle.Name.ToLower().Contains("design"))
                .Select(el => el.Id)
                .ToList();

            _designNoteViews.Add(view, designEl);
            view.HideElements(designEl);
        }
        tx.Commit();
    }
    public static void Unhide(Document doc)
    {
        using Transaction tx = new (doc, "Unhide Design Notes");
        if (tx.Start() != TransactionStatus.Started) return;

        foreach (var view in _designNoteViews.Keys)
            view.UnhideElements(_designNoteViews[view]);
        
        tx.Commit();
    }
}