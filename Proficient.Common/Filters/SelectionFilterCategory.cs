using Proficient.Forms;
using Proficient.Utilities;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class SelectionFilterCategory : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var selectedIds = uiDoc.Selection.GetElementIds();

        if (selectedIds.Count == 0) 
            return Result.Succeeded;

        var cec = new CategoryEqualityComparer();

        var cats = selectedIds
            .Select(doc.GetElement)
            .Select(el => el.Category)
            .Distinct(cec)
            .OrderBy(cat => cat.Name);

        var bvm = new BlankViewModel();
        var mousePos = Mouse.GetCursorPosition();
        bvm.SetLocation(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y));
        var selectedCat = string.Empty;

        foreach (var cat in cats)
            bvm.AddButton("smallBtn", cat.Name, () => selectedCat = cat.Name, true, true);
            
        if (!bvm.ShowWindow(true) ?? false)
            return Result.Cancelled;

        var filteredIds = selectedIds
            .Where(id => doc.GetElement(id).Category.Name == selectedCat)
            .ToList();
            
        using var tx = new Transaction(doc, "Quick Filter");
        if (tx.Start() == TransactionStatus.Started)
            uiDoc.Selection.SetElementIds(filteredIds);
        tx.Commit();

        return Result.Succeeded;
    }
}

internal class CategoryEqualityComparer : IEqualityComparer<Category>
{
    public bool Equals(Category? c1, Category? c2)
    {
        if (c1 == null && c2 == null)
            return true;
        if (c1 == null || c2 == null)
            return false;
        return c1.Name == c2.Name;
    }

    public int GetHashCode(Category? c)
    {
#if PRE24
        return c?.Id.IntegerValue.GetHashCode() ?? 0;
#else
        return c?.Id.Value.GetHashCode() ?? 0;
#endif
    }
}