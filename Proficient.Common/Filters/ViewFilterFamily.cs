using Autodesk.Revit.UI.Selection;
using Proficient.Forms;
using Proficient.Utilities;
using PFRF = Autodesk.Revit.DB.ParameterFilterRuleFactory;

namespace Proficient.Filters;

[Transaction(TransactionMode.Manual)]
internal class ViewFilterFamily : IExternalCommand
{
    private const string MechFilterName = "Proficient Mechanical Hidden Families";
    private const string ElecFilterName = "Proficient Electrical Hidden Families";
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;
        var isViewTemplate = false;
        var linkEl = false;

        var filterName = Main.Settings?.DefWorkset.StartsWith("M") ?? true ? MechFilterName : ElecFilterName;

        var selIds = uiDoc.Selection.GetElementIds();
#if !PRE23
        var selRefs = uiDoc.Selection.GetReferences();
#endif
        RevitLinkInstance? selectedLink = null;
        Element el;

        if (selIds.Count > 0)
        {
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
            linkEl = true;

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
                selectedLink = doc.GetElement(selRef) as RevitLinkInstance;
                if (selectedLink is null)
                    return Result.Cancelled;
                el = selectedLink.GetLinkDocument().GetElement(selRef.LinkedElementId);
            }
            else
            {
                el = doc.GetElement(selRef.ElementId);
            }
        }

        if (el.GetTypeId() == null || el.GetTypeId() == ElementId.InvalidElementId || el.Category.CategoryType != CategoryType.Model)
            return Result.Failed;

        if (view.ViewTemplateId != ElementId.InvalidElementId)
        {
            view = (View)doc.GetElement(view.ViewTemplateId);
            isViewTemplate = true;
        }
        var famName = linkEl && selectedLink is not null ?
            (selectedLink.GetLinkDocument().GetElement(el.GetTypeId()) as ElementType)?.FamilyName:
            (doc.GetElement(el.GetTypeId()) as ElementType)?.FamilyName;
        if (famName == null)
            return Result.Failed;

        var catId = el.Category.Id;

        var pfes = new FilteredElementCollector(doc)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .ToList();

        var existing = pfes.FirstOrDefault(pfe => pfe.Name == filterName);

        using Transaction tx = new(doc, "Add View Filter");
        if (tx.Start() != TransactionStatus.Started) return Result.Failed;

        ParameterFilterElement filter;

        if (existing == null)
        {
            // Create brand new filter with this one rule and category
            filter = ParameterFilterElement.Create(
                doc,
                filterName,
                [catId],
                BuildOrFilter([famName]));
        }
        else
        {
            // Collect existing family names from the current rules
            var existingFamilyNames = GetExistingFamilyNames(existing);

            if (existingFamilyNames.Contains(famName) && existing.GetCategories().Contains(catId))
            {
                // Nothing new to add - just ensure filter is applied to view below
                filter = existing;
            }
            else
            {
                if (!existingFamilyNames.Contains(famName))
                    existingFamilyNames.Add(famName);

                // Add category if not already present
                var cats = existing.GetCategories().ToList();
                if (!cats.Contains(catId))
                    cats.Add(catId);

                existing.SetCategories(cats);
                existing.SetElementFilter(BuildOrFilter(existingFamilyNames));
                filter = existing;
            }
        }

        var filterAdded = false;

        if (!view.GetFilters().Contains(filter.Id))
        {
            filterAdded = true;
            view.AddFilter(filter.Id);
        }

        view.SetFilterVisibility(filter.Id, false);

        var viewType = isViewTemplate ? "View Template" : "View";
        var filterAddedText = filterAdded ? $"\n{filterName} filter added to {viewType} {view.Name}" : string.Empty;
        Util.BalloonTip("Family Hidden", $"\"{famName}\" added to {filterName} filter.{filterAddedText}", string.Empty);

        tx.Commit();
        return Result.Succeeded;
    }

    private static ElementFilter BuildOrFilter(List<string> familyNames)
    {
        var rules = familyNames.Select(name =>
        {
#if PRE23
            var rule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), name, true);
#else
            var rule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME), name);
#endif
            return (ElementFilter)new ElementParameterFilter(rule);
        }).ToList();

        return rules.Count == 1
            ? rules[0]
            : new LogicalOrFilter(rules);
    }

    private static List<string> GetExistingFamilyNames(ParameterFilterElement pfe)
    {
        List<string> names = [];
        try
        {
            // Walk the filter tree to collect family name rule values
            CollectFamilyNames(pfe.GetElementFilter(), names);
        }
        catch { /* filter structure unreadable, start fresh */ }

        return names;
    }

    private static void CollectFamilyNames(ElementFilter? filter, List<string> names)
    {
        if (filter is null) return;

        if (filter is LogicalOrFilter orFilter)
        {
            foreach (var f in orFilter.GetFilters())
                CollectFamilyNames(f, names);
        }
        else if (filter is ElementParameterFilter epf)
        {
            foreach (var rule in epf.GetRules())
            {
#if PRE23
                if (rule is FilterStringRule fsr &&
                    fsr.GetRuleParameter() == new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME))
                    names.Add(fsr.RuleString);
#else
                if (rule is FilterStringRule fsr &&
                    fsr.GetRuleParameter() == new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME))
                    names.Add(fsr.RuleString);
#endif
            }
        }
    }
}