using Proficient.Forms;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class SelectionFilterType : IExternalCommand
{
    internal static Document Doc;
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        Doc = uiDoc.Document;

        var selectedIds = uiDoc.Selection.GetElementIds();

        if (selectedIds.Count == 0)
            return Result.Succeeded;

        var tec = new TypeEqualityComparer();

        var types = selectedIds
            .Select(Doc.GetElement)
            .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
            .Distinct(tec)
            .OrderBy(el => (Doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName)
            .ThenBy(el => el.Name);

        var bvm = new BlankViewModel();

        var selectedFamily = string.Empty;
        var selectedType = string.Empty;

        foreach (var type in types)
        {
            string fam = (Doc.GetElement(type.GetTypeId()) as ElementType)?.FamilyName;
            string display = fam + " - " + type.Name;
            bvm.AddButton("smallBtn", display, () => { selectedFamily = fam; selectedType = type.Name; }, true, true);
        }

        if (!bvm.ShowWindow(true) ?? false)
            return Result.Cancelled;

        var filteredIds = selectedIds
            .Select(Doc.GetElement)
            .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
            .Where(el => el.Name == selectedType && (Doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName == selectedFamily)
            .Select(el => el.Id)
            .ToList();

        using var tx = new Transaction(Doc, "Quick Filter");
        if (tx.Start() == TransactionStatus.Started)
            uiDoc.Selection.SetElementIds(filteredIds);
        tx.Commit();

        return Result.Succeeded;
    }
}

internal class TypeEqualityComparer : IEqualityComparer<Element>
{
    public bool Equals(Element e1, Element e2)
    {
        string f1 = (SelectionFilterType.Doc.GetElement(e1?.GetTypeId()) as ElementType)?.FamilyName;
        string f2 = (SelectionFilterType.Doc.GetElement(e2?.GetTypeId()) as ElementType)?.FamilyName;

        if (e1 == null && e2 == null)
            return true;
        if (e1 == null || e2 == null)
            return false;
        return e1.Name == e2.Name && f1 == f2;
    }

    public int GetHashCode(Element e)
    {
        return SelectionFilterType.Doc.GetElement(e.Id).GetTypeId().IntegerValue.GetHashCode();
    }
}