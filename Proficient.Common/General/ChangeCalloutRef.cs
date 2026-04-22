using Proficient.Forms;

namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class ChangeCalloutRef : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

#if PRE24
        var selCalloutIds = uiDoc.Selection.GetElementIds()?
            .Select(doc.GetElement)
            .Where(el => el is ViewPlan)
            .Where(el => el.get_Parameter(BuiltInParameter.SECTION_PARENT_VIEW_NAME) is not null)
            .Where(el => el.OwnerViewId.IntegerValue != -1)
            .Select(el => el.Id)
            .ToList();
#else
        var selCalloutIds = uiDoc.Selection.GetElementIds()?
            .Select(doc.GetElement)
            .Where(el => el is ViewPlan)
            .Where(el => el.get_Parameter(BuiltInParameter.SECTION_PARENT_VIEW_NAME) is not null)
            .Where(el => el.OwnerViewId.Value != -1)
            .Select(el => el.Id)
            .ToList();
#endif
        if (selCalloutIds is null || selCalloutIds.Count == 0)
            return Result.Cancelled;

        var allCallouts = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Views)
            .Where(el => el is ViewPlan)
            .Where(el => el.get_Parameter(BuiltInParameter.SECTION_PARENT_VIEW_NAME) is not null)
            .Cast<View>()
            .ToList();

        string[] calloutNames = [.. allCallouts.Select(c => c.Name)];

        var vf =  new ViewForm(calloutNames);

        if (vf.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            return Result.Cancelled;

        var newView = allCallouts[vf.SelectedViewIndex];

        using var tx = new Transaction(doc, "Change Callout Reference");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Cancelled;
            
        foreach (var id in selCalloutIds)
            ReferenceableViewUtils.ChangeReferencedView(doc, id, newView.Id);

        tx.Commit();
        vf.Close();

        return Result.Succeeded;
    }
}