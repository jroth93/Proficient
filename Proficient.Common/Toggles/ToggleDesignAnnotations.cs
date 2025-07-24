using Autodesk.Revit.DB.ExtensibleStorage;
using Proficient.Utilities;

namespace Proficient.Toggles;

[Transaction(TransactionMode.Manual)]
internal class ToggleDesignAnnotations : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {

        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        bool visible = GetVisibilityField(view);

        ElementMulticategoryFilter mcf = new(new List<BuiltInCategory>{
            BuiltInCategory.OST_TextNotes,
            BuiltInCategory.OST_Lines, 
            BuiltInCategory.OST_Dimensions, 
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_DetailComponents});
        List<ElementId> designElIds;

        if (visible)
        {
            designElIds = new FilteredElementCollector(doc, view.Id)
                .WherePasses(mcf)
                .Concat(new FilteredElementCollector(doc, view.Id).OfClass(typeof(IndependentTag)))
                .Where(el =>
                    el.Name.ToLower().Contains("design") ||
                    el is CurveElement ce && ce.LineStyle.Name.ToLower().Contains("design"))
                .Select(el => el.Id)
                .ToList();
        }
        else
        {
            designElIds = new FilteredElementCollector(doc)
                .WherePasses(mcf)
                .Concat(new FilteredElementCollector(doc).OfClass(typeof(IndependentTag)))
                .Where(el => el.IsHidden(view))
                .Where(el =>
                    el.Name.ToLower().Contains("design") ||
                    el is CurveElement ce && ce.LineStyle.Name.ToLower().Contains("design"))
                .Select(el => el.Id)
                .ToList();
        }

        if (!designElIds.Any())
            return Result.Succeeded;

        using Transaction tx = new (doc, "Toggle Design Note Visibility");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;
        if (visible)
            view.HideElements(designElIds);
        else
            view.UnhideElements(designElIds);
        
        SetVisibilityField(view, !visible);
        string alert = visible ? "hidden" : "visible";
        Util.BalloonTip("Design Notes", $"Design notes are {alert}.","");

        tx.Commit();

        return Result.Succeeded;
    }

    private static bool GetVisibilityField(Element view)
    {
        var pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
        var ent = view.GetEntity(pSchema);
        var field = pSchema.GetField(SchemaKeys.BoolDict);

        if (ent.Schema == null)
            return true;
        
        ent.Get<IDictionary<string, bool>>(field)
            .TryGetValue(SchemaKeys.DesignNoteVisibility, out bool visible);
        
        return visible;
    }

    private static void SetVisibilityField(Element view, bool visible)
    {

        var pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
        var ent = view.GetEntity(pSchema);
        var field = pSchema.GetField(SchemaKeys.BoolDict);
        IDictionary<string, bool> boolDict = new Dictionary<string, bool>();

        if (ent.Schema is null)
            ent = new Entity(pSchema);

        boolDict[SchemaKeys.DesignNoteVisibility] = visible;

        ent.Set(field, boolDict);
        view.SetEntity(ent);
    }

}