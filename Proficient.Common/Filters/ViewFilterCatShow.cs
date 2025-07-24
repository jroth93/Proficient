using Proficient.Forms;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class ViewFilterCatShow : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        if (view.ViewTemplateId != ElementId.InvalidElementId)
            view = (View)doc.GetElement(view.ViewTemplateId);

        var cats = doc.Settings.Categories
            .Cast<Category>()
            .Where(cat => view.GetCategoryHidden(cat.Id))
            .OrderBy(cat => cat.Name);

        Category selectedCat = null;

        var bvm = new BlankViewModel();
        foreach (var cat in cats)
        {
            bvm.AddButton("smallBtn", cat.Name, () => { selectedCat = cat; }, true, true);
        }

        if (!bvm.ShowWindow(true) ?? false)
            return Result.Cancelled;

        if (selectedCat is null)
            return Result.Failed;

        using Transaction tx = new(doc, "Show Category");
        if (tx.Start() != TransactionStatus.Started) return Result.Failed;
        view.SetCategoryHidden(selectedCat.Id, false);
        tx.Commit();

        return Result.Succeeded;
    }
}