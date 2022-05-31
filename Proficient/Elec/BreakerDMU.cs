using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Electrical;

namespace Proficient.Elec
{
    public class BreakerDMU : IUpdater
    {
        static UpdaterId _updaterId;
        static ICollection<ElementId> addedIds;
        static Document doc;

        public BreakerDMU()
        {
            _updaterId = new UpdaterId(new AddInId(Main.appId), new Guid("86168304-BB36-4B81-B498-D19FFF96377B"));
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                doc = data.GetDocument();

                if (data.GetAddedElementIds().Any())
                {
                    addedIds = data.GetAddedElementIds();
                    Main.App.Idling += AddNewElementTriggers;

                    foreach (ElementId id in addedIds)
                    {
                        Wire w = doc.GetElement(id) as Wire;
                        BuiltInCategory bic = w is Wire ? (BuiltInCategory)w.Category.Id.IntegerValue : BuiltInCategory.INVALID;
                        if (bic == BuiltInCategory.OST_Wire && w.GetMEPSystems().Any())
                        {
                            string newVal = doc.GetElement(w.GetMEPSystems().First())
                                .LookupParameter(Names.Parameter.BreakerOptions).AsString();

                            Parameter wPar = w.LookupParameter(Names.Parameter.BreakerOptions);

                            if (wPar != null)
                            {
                                wPar.Set(newVal);
                            }
                        }
                    }

                    return;
                }

                foreach (ElementId id in data.GetModifiedElementIds())
                {
                    bool panelChange = data.IsChangeTriggered(id, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM)));
                    bool circuitChange = data.IsChangeTriggered(id, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_WIRE_CIRCUITS)));
                    if (panelChange || circuitChange)
                    {
                        Wire w = doc.GetElement(id) as Wire;
                        Parameter par = w.LookupParameter(Names.Parameter.BreakerOptions);
                        if (par != null)
                        {
                            if (w.GetMEPSystems().Any())
                            {
                                string newVal = doc.GetElement(w.GetMEPSystems().First()).LookupParameter(Names.Parameter.BreakerOptions).AsString();
                                par.Set(newVal);
                            }
                            else
                            {
                                par.Set(string.Empty);
                            }
                        }
                        continue;
                    }

                    Element el = doc.GetElement(id);
                    BuiltInCategory bic = (BuiltInCategory)el.Category.Id.IntegerValue;

                    if (bic == BuiltInCategory.OST_Wire && (el as Wire).GetMEPSystems().Any())
                    {
                        string newVal = el.LookupParameter(Names.Parameter.BreakerOptions).AsString();
                        Element ec = doc.GetElement((el as Wire).GetMEPSystems().First());
                        ec.LookupParameter(Names.Parameter.BreakerOptions).Set(newVal);

                        var ws = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_Wire)
                            .Where(w => w is Wire)
                            .Cast<Wire>()
                            .Where(w => w.GetMEPSystems().Any())
                            .Where(w => w.GetMEPSystems().First() == ec.Id)
                            .Where(w => w.Id != el.Id);

                        foreach (Wire w in ws)
                        {
                            Parameter wPar = w.LookupParameter(Names.Parameter.BreakerOptions);

                            if (wPar != null)
                            {
                                wPar.Set(newVal);
                            }
                        }

                    }
                    else if ((BuiltInCategory)el.Category.Id.IntegerValue == BuiltInCategory.OST_ElectricalCircuit)
                    {
                        string newVal = el.LookupParameter(Names.Parameter.BreakerOptions).AsString();

                        var ws = new FilteredElementCollector(doc)
                            .OfCategory(BuiltInCategory.OST_Wire)
                            .Where(el => el is Wire)
                            .Cast<Wire>()
                            .Where(w => w.GetMEPSystems().Any())
                            .Where(w => w.GetMEPSystems().First() == el.Id);

                        foreach (Wire w in ws)
                        {
                            Parameter wPar = w.LookupParameter(Names.Parameter.BreakerOptions);

                            if (wPar != null)
                            {
                                wPar.Set(newVal);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                TaskDialog.Show("Breaker Updater Error", ex.Message + ex.StackTrace);
            }

        }

        private void AddNewElementTriggers(object sender, IdlingEventArgs e)
        {
            foreach (ElementId id in addedIds)
            {
                Element el = doc.GetElement(id);
                Parameter par = el.LookupParameter(Names.Parameter.BreakerOptions);
                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_Wire);

                if ((BuiltInCategory) el.Category.Id.IntegerValue == BuiltInCategory.OST_ElectricalCircuit)
                {
                    f = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);
                }

                if (par != null)
                {
                    UpdaterRegistry.AddTrigger(_updaterId, f, Element.GetChangeTypeParameter(par));
                }
            }

            Main.App.Idling -= AddNewElementTriggers;
        }

        public string GetAdditionalInformation()
        {
            return "Josh Roth";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.MEPSystems;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "BreakerDMU";
        }
    }
}
