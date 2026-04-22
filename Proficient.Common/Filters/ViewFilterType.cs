using Autodesk.Revit.UI.Selection;
using Proficient.Forms;
using Proficient.Utilities;
using PFRF = Autodesk.Revit.DB.ParameterFilterRuleFactory;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class ViewFilterType : IExternalCommand
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

        if (el.GetTypeId() == null || el.GetTypeId() == ElementId.InvalidElementId || el.Category.CategoryType != CategoryType.Model)
            return Result.Failed;

        if (view.ViewTemplateId != ElementId.InvalidElementId)
            view = (View)doc.GetElement(view.ViewTemplateId);
        string famName = (doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName ?? string.Empty;
        string typeName = el.Name;
        string filtName = "*" + famName + "-" + typeName;
        var pfes = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement));
        ParameterFilterElement filter;
        if (pfes.Any(pfe => pfe.Name == filtName))
        {
            filter = (ParameterFilterElement)pfes.First(pfe => pfe.Name == filtName);
        }
        else
        {
#if PRE23
            var famRule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), famName, true);
            var typeRule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), typeName, true);
#else
            var famRule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), famName);
            var typeRule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME), typeName);
#endif
            var epf = new ElementParameterFilter(new List<FilterRule> {famRule, typeRule});
            using Transaction ftx = new(doc, "Create View Filter");
            if (ftx.Start() != TransactionStatus.Started) return Result.Failed;
            filter = ParameterFilterElement.Create(doc, filtName, new List<ElementId> { el.Category.Id }, epf);
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