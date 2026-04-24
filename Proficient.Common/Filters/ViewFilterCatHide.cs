using Autodesk.Revit.UI.Selection;
using Proficient.Forms;
using Proficient.Utilities;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class ViewFilterCatHide : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;
        var linkEl = true;
        var isViewTemplate = false;

        var selIds = uiDoc.Selection.GetElementIds();
#if !PRE23
        var selRefs = uiDoc.Selection.GetReferences();
#endif
        Element el;
        RevitLinkInstance? selectedLink = null;
#if !PRE24
        RevitLinkGraphicsSettings? settings = null;
#endif

        if (selIds.Count > 0)
        {
            linkEl = false;
            el = doc.GetElement(selIds.First());
        }
#if !PRE23
        else if (selRefs.Count > 0)
        {
            var selRef = selRefs.First();
            el = doc.GetElement(selRef.ElementId);
            selectedLink = doc.GetElement(selRef.ElementId) as RevitLinkInstance;
            if (selectedLink is null)
                return Result.Cancelled;
            el = selectedLink.GetLinkDocument().GetElement(selRef.LinkedElementId);
            
        }
#endif
        else
        {
            BlankViewModel bvm = new();
            var mousePos = Mouse.GetCursorPosition();
            bvm.SetLocation(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y));
            bvm.AddButton("smallBtn", "Linked Element", () => linkEl = true, true, true);
            bvm.AddButton("smallBtn", "Model Element", () => linkEl = false, true, true);

            if (!bvm.ShowWindow(true) ?? false)
                return Result.Cancelled;

            var ot = linkEl ? ObjectType.LinkedElement : ObjectType.Element;
            var selRef = uiDoc.Selection.PickObject(ot, "Pick an element");

            if (linkEl)
            {
                selectedLink = doc.GetElement(selRef.ElementId) as RevitLinkInstance;
                if (selectedLink is null)
                    return Result.Cancelled;
                el = selectedLink.GetLinkDocument().GetElement(selRef.LinkedElementId);
            }
            else
            {
                el = doc.GetElement(selRef.ElementId);
            }
        }

#if !PRE24
        if (view.ViewTemplateId != ElementId.InvalidElementId)
        {
            view = (View)doc.GetElement(view.ViewTemplateId);
            isViewTemplate = true;
        }
        if (linkEl)
            settings = view.GetLinkOverrides(selectedLink?.GetTypeId());

        using Transaction tx = new(doc, "Hide Category");
        if (tx.Start() != TransactionStatus.Started) return Result.Failed;

        if (selectedLink is not null && settings is not null && settings.LinkVisibilityType == LinkVisibility.Custom)
        {
            Util.BalloonTip("Operation Unavailable", "Unable to hide category of link with custom visibility settings.", string.Empty);
        }
        else
        {
            view.SetCategoryHidden(el.Category.Id, true);
            var viewType = isViewTemplate ? "View Template" : "View";
            Util.BalloonTip("Category Hidden", $"{el.Category.Name} category hidden in {viewType} {view.Name}", string.Empty);
        }
        tx.Commit();
#endif

        return Result.Succeeded;
    }
}