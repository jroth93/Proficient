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
        
        var selIds = uiDoc.Selection.GetElementIds();
        Element el;
        if (selIds.Any())
        {
            el = doc.GetElement(selIds.First());
        }
        else
        {
            var linkEl = true;
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
                if (doc.GetElement(selRef) is not RevitLinkInstance linkInst)
                    return Result.Cancelled;
                el = linkInst.GetLinkDocument().GetElement(selRef.LinkedElementId);
            }
            else
            {
                el = doc.GetElement(selRef.ElementId);
            }
        }

        if (view.ViewTemplateId != ElementId.InvalidElementId)
            view = (View)doc.GetElement(view.ViewTemplateId);

        using Transaction tx = new(doc, "Hide Category");
        if (tx.Start() != TransactionStatus.Started) return Result.Failed;
        view.SetCategoryHidden(el.Category.Id, true);
        tx.Commit();


        return Result.Succeeded;
    }
}