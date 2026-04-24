using Proficient.Forms;
using Proficient.Utilities;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class ViewFilterCatShow : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;
        var isViewTemplate = false;

        if (view.ViewTemplateId != ElementId.InvalidElementId)
        {
            view = (View)doc.GetElement(view.ViewTemplateId);
            isViewTemplate = true;
        }

        var cats = doc.Settings.Categories
            .Cast<Category>()
            .Where(cat => view.GetCategoryHidden(cat.Id))
            .OrderBy(cat => cat.Name);

        Category? selectedCat = null;

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
        var viewType = isViewTemplate ? "View Template" : "View";
        Util.BalloonTip("Category Visible", $"{selectedCat.Name} category visible in {viewType} {view.Name}", string.Empty);
        tx.Commit();

        return Result.Succeeded;
    }
}