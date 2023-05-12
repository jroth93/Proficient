using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Events;
using Proficient.Utilities;

namespace Proficient.Electrical;

internal class BreakerDmu : IUpdater
{
    private static UpdaterId _updaterId;
    private static ICollection<ElementId> _addedIds;

    public BreakerDmu()
    {
        _updaterId = new UpdaterId(new AddInId(Main.AppId), new Guid("86168304-BB36-4B81-B498-D19FFF96377B"));
    }

    public void Execute(UpdaterData data)
    {
        try
        {
            var doc = data.GetDocument();

            if (data.GetAddedElementIds().Any())
            {
                _addedIds = data.GetAddedElementIds();
                Main.App.Idling += AddNewElementTriggers;

                foreach (var id in _addedIds)
                {
                    if (doc.GetElement(id) is not Wire w || !w.GetMEPSystems().Any()) continue;

                    var newVal = doc.GetElement(w.GetMEPSystems().First())
                        .LookupParameter(Names.Parameter.BreakerOptions).AsString();

                    w.LookupParameter(Names.Parameter.BreakerOptions)?.Set(newVal);
                }

                return;
            }

            foreach (var id in data.GetModifiedElementIds())
            {
                var el = doc.GetElement(id);
                var bic = (BuiltInCategory)el.Category.Id.IntegerValue;
                bool panelChange = data.IsChangeTriggered(id, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM)));
                bool circuitChange = data.IsChangeTriggered(id, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_WIRE_CIRCUITS)));

                if (panelChange || circuitChange)
                {
                    var par = el.LookupParameter(Names.Parameter.BreakerOptions);
                    if (par == null) continue;

                    if (el is Wire w && w.GetMEPSystems().Any())
                    {
                        var newVal = doc.GetElement(w.GetMEPSystems().First()).LookupParameter(Names.Parameter.BreakerOptions).AsString();
                        par.Set(newVal);
                    }
                    else
                    {
                        par.Set(string.Empty);
                    }
                }
                else if (bic == BuiltInCategory.OST_Wire && el is Wire wire && wire.GetMEPSystems().Any())
                {
                    var newVal = el.LookupParameter(Names.Parameter.BreakerOptions).AsString();
                    var ec = doc.GetElement(wire.GetMEPSystems().First());
                    ec.LookupParameter(Names.Parameter.BreakerOptions).Set(newVal);

                    var ps = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Wire)
                        .Where(w => w is Wire)
                        .Cast<Wire>()
                        .Where(w => w.GetMEPSystems().FirstOrDefault() == ec.Id)
                        .Where(w => w.Id != el.Id)
                        .Select(w => w.LookupParameter(Names.Parameter.BreakerOptions))
                        .Where(p => p.HasValue);

                    foreach (var p in ps)
                        p.Set(newVal);
                }
                else if ((BuiltInCategory)el.Category.Id.IntegerValue == BuiltInCategory.OST_ElectricalCircuit)
                {
                    var newVal = el.LookupParameter(Names.Parameter.BreakerOptions).AsString();

                    var ps = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_Wire)
                        .Where(e => e is Wire)
                        .Cast<Wire>()
                        .Where(w => w.GetMEPSystems().FirstOrDefault() == el.Id)
                        .Select(w => w.LookupParameter(Names.Parameter.BreakerOptions))
                        .Where(p => p.HasValue);

                    foreach (var p in ps)
                        p.Set(newVal);

                }
            }
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Breaker Updater Error", ex.Message + ex.StackTrace);
        }

    }

    private static void AddNewElementTriggers(object sender, IdlingEventArgs e)
    {
        if (sender is not UIApplication uiApp) return;
        var doc = uiApp.ActiveUIDocument.Document;
        foreach (var id in _addedIds)
        {
            var el = doc.GetElement(id);
            var par = el.LookupParameter(Names.Parameter.BreakerOptions);
            var f = new ElementCategoryFilter(BuiltInCategory.OST_Wire);

            if ((BuiltInCategory)el.Category.Id.IntegerValue == BuiltInCategory.OST_ElectricalCircuit)
                f = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);

            if (par != null)
                UpdaterRegistry.AddTrigger(_updaterId, f, Element.GetChangeTypeParameter(par));
        }

        Main.App.Idling -= AddNewElementTriggers;
    }

    public string GetAdditionalInformation() => "Josh Roth";

    public ChangePriority GetChangePriority() => ChangePriority.MEPSystems;

    public UpdaterId GetUpdaterId() => _updaterId;

    public string GetUpdaterName() => "BreakerDmu";
}