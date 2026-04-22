using Autodesk.Revit.UI.Selection;
using Proficient.Forms;
using Proficient.Utilities;
using PFRF = Autodesk.Revit.DB.ParameterFilterRuleFactory;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class ViewFilterFamily : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var app = revit.Application;
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

        if (el.GetTypeId() == null || el.GetTypeId() == ElementId.InvalidElementId || el.Category.CategoryType != CategoryType.Model)
            return Result.Failed;

        if (view.ViewTemplateId != ElementId.InvalidElementId)
            view = (View)doc.GetElement(view.ViewTemplateId);
        string? name = (doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName;
        if (name == null)
            return Result.Failed;

        var pfes = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
        ParameterFilterElement filter;
        if (pfes.Any(pfe => pfe.Name == "*" + name))
        {
            filter = (ParameterFilterElement)pfes.First(pfe => pfe.Name == "*" + name);
        }
        else
        {
#if PRE23
            var rule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), name, true);
#else
            var rule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), name);
#endif
            using Transaction ftx = new(doc, "Create View Filter");
            if (ftx.Start() != TransactionStatus.Started) return Result.Failed;
            filter = ParameterFilterElement.Create(doc, "*" + name, new List<ElementId> { el.Category.Id }, new ElementParameterFilter(rule));
            ftx.Commit();
        }

        using Transaction tx = new(doc, "Add View Filter");
        if (tx.Start() != TransactionStatus.Started) return Result.Failed;
        view.AddFilter(filter.Id);
        view.SetFilterVisibility(filter.Id, false);
        tx.Commit();

        return Result.Succeeded;
    }
}