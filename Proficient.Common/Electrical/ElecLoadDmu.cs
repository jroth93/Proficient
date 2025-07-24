using Autodesk.Revit.DB.Electrical; //don't delete, needed for PRE21 Electrical Systems
using Proficient.Utilities;

namespace Proficient.Electrical;

internal class ElecLoadDmu : IUpdater
{
    private static UpdaterId? _updaterId;
    private static ICollection<ElementId>? _addedIds;

    public ElecLoadDmu()
    {
        _updaterId = new UpdaterId(new AddInId(Main.AppId), Names.Guids.ElecLoadDmu);
    }

    public void Execute(UpdaterData data)
    {
        var doc = data.GetDocument();
        IEnumerable<FamilyInstance> fis;

        _addedIds = data.GetAddedElementIds();

        var circClasses = new []{"HVAC -", "ELEC HEAT -", "MOTOR -"};

        if (_addedIds.Count != 0)
        {
#if PRE24
            if (doc.GetElement(_addedIds.First()).Category.Id.IntegerValue != (int)BuiltInCategory.OST_ElectricalCircuit)
            {
                Main.App.Idling += AddNewElementTriggers;
                return;
            }
#else
            if (doc.GetElement(_addedIds.First()).Category.Id.Value != (long)BuiltInCategory.OST_ElectricalCircuit)
            {
                Main.App.Idling += AddNewElementTriggers;
                return;
            }
#endif

            fis = _addedIds
                .Where(id => doc.GetElement(id) is MEPSystem)
                .Select(id => doc.GetElement(id))
                .Cast<MEPSystem>()
                .Where(circ =>
                    circClasses.Contains(circ.get_Parameter(BuiltInParameter.CIRCUIT_LOAD_CLASSIFICATION_PARAM).AsString()))
                .SelectMany(circ => circ.Elements.Cast<Element>())
                .Where(el => el is FamilyInstance)
                .Cast<FamilyInstance>();
#if PRE24
            fis = fis.Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment);
#else
            fis = fis.Where(el => el.Category.Id.Value == (long)BuiltInCategory.OST_MechanicalEquipment);
#endif
        }
        else
        {
            var els = data
                .GetModifiedElementIds()
                .Select(id => doc.GetElement(id))
                .ToList();
                
            //handle type being modified
            if (els.Count == 1 && els.First() is FamilySymbol fs)
            {
                fis = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                    .Where(el => el is FamilyInstance)
                    .Cast<FamilyInstance>()
                    .Where(fi => fi.Symbol.Id == fs.Id)
#if (PRE21)
                    .Where(fi => fi.MEPModel.ElectricalSystems != null);
#else
                    .Where(fi => fi.MEPModel.GetElectricalSystems() != null);
#endif
            }
            //handle elements being modified
            else
            {
                fis = els
                    .Where(el => el is FamilyInstance)
                    .Cast<FamilyInstance>()
#if (PRE21)
                    .Where(fi => fi.MEPModel.ElectricalSystems != null);
#else
                    .Where(fi => fi.MEPModel.GetElectricalSystems() != null);
#endif
            }
        }
            
            
            
        foreach (var fi in fis)
        {
            var dsPar = fi.LookupParameter(Names.Parameter.DisplaySeparation);
            var typeMark = fi.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString();
            var mark = fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
            string planTag;

            if (dsPar != null)
            {
                planTag = typeMark + dsPar.AsString() + mark;
            }
            else
            {
                if(typeMark == string.Empty && mark != string.Empty)
                {
                    planTag = mark;
                }
                else if(mark == string.Empty && typeMark != string.Empty)
                {
                    planTag = typeMark;
                }
                else
                {
                    planTag = typeMark + "-" + mark;
                }
            }

#if (PRE21)
            foreach (ElectricalSystem es in fi.MEPModel.ElectricalSystems)
                es.LoadName = planTag;
#else
            foreach (var es in fi.MEPModel.GetElectricalSystems())
                es.LoadName = planTag;
#endif
        }

    }

    private static void AddNewElementTriggers(object? sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
    {
        if (sender is not UIApplication uiApp) return;
        var doc = uiApp.ActiveUIDocument.Document;

        if (_addedIds is null) return;

        foreach (var id in _addedIds)
        {
            var par = doc.GetElement(id).LookupParameter(Names.Parameter.DisplaySeparation);
            if (par == null) continue;

            try
            {
                UpdaterRegistry.AddTrigger(
                    _updaterId, 
                    new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment), 
                    Element.GetChangeTypeParameter(par));
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.Message + "\n" + ex.StackTrace);
            }
        }

        Main.App.Idling -= AddNewElementTriggers;
    }

    public string GetAdditionalInformation() => "Josh Roth";

    public ChangePriority GetChangePriority() => ChangePriority.MEPSystems;

    public UpdaterId? GetUpdaterId() => _updaterId;

    public string GetUpdaterName() => "ElecLoadDmu";
}