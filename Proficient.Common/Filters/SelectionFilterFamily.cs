using Proficient.Forms;
using Proficient.Utilities;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class SelectionFilterFamily : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var selectedIds = uiDoc.Selection.GetElementIds();

        if (selectedIds.Count == 0)
            return Result.Succeeded;

        var fams = selectedIds
            .Select(id => doc.GetElement(id))
            .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
            .Select(el => (doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName)
            .Distinct()
            .OrderBy(fam => fam);

        var bvm = new BlankViewModel();

        var mousePos = Mouse.GetCursorPosition();
        bvm.SetLocation(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y));
        var selectedFam = string.Empty;

        foreach (var fam in fams)
        {
            if (fam is null)
                continue;
            bvm.AddButton("smallBtn", fam, () => selectedFam = fam, true, true);
        }
            

        if (!bvm.ShowWindow(true) ?? false)
            return Result.Cancelled;

        var filteredIds = selectedIds
            .Select(id => doc.GetElement(id))
            .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
            .Where(el => (doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName == selectedFam)
            .Select(el => el.Id)
            .ToList();

        using var tx = new Transaction(doc, "Quick Filter");
        if (tx.Start() == TransactionStatus.Started)
            uiDoc.Selection.SetElementIds(filteredIds);
        tx.Commit();

        return Result.Succeeded;
    }
}