using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Electrical;

namespace Proficient.Elec
{
    public class ElecLoadDMU : IUpdater
    {
        static UpdaterId _updaterId;
        static ICollection<ElementId> addedIds;
        static Document doc;

        public ElecLoadDMU()
        {
            _updaterId = new UpdaterId(new AddInId(Main.appId), new Guid("088EE9E1-8EFA-438D-9287-F180436519BD"));
        }

        public void Execute(UpdaterData data)
        {
            doc = data.GetDocument();
            List<FamilyInstance> fis = new List<FamilyInstance>();

            if (data.GetAddedElementIds().Any())
            {
                addedIds = data.GetAddedElementIds();
                if ((BuiltInCategory)doc.GetElement(addedIds.First()).Category.Id.IntegerValue == BuiltInCategory.OST_ElectricalCircuit)
                {
                    foreach (ElementId id in addedIds)
                    {
                        MEPSystem circ = doc.GetElement(id) as MEPSystem;
                        string circClass = circ.get_Parameter(BuiltInParameter.CIRCUIT_LOAD_CLASSIFICATION_PARAM).AsString();
                        if (circClass == "HVAC -" || circClass == "ELEC HEAT -" || circClass == "MOTOR -")
                        {
                            foreach (Element el in circ.Elements)
                            {
                                if ((BuiltInCategory)el.Category.Id.IntegerValue == BuiltInCategory.OST_MechanicalEquipment)
                                {
                                    fis.Add(el as FamilyInstance);
                                }
                            }

                        }
                    }
                }
                else
                {
                    Main.App.Idling += AddNewElementTriggers;
                    return;
                }            
            }
            else
            {
                List<Element> els = data.GetModifiedElementIds().Select(id => doc.GetElement(id)).ToList();
                
                if (els.Count == 1 && (els.First() as FamilySymbol) != null)
                {
                    fis = new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                        .Where(el => el is FamilyInstance)
                        .Cast<FamilyInstance>()
                        .Where(fi => fi.Symbol.Id == (els.First() as FamilySymbol).Id)
#if (R21 || R22)
                        .Where(fi => fi.MEPModel.GetElectricalSystems() != null)
#else
                        .Where(fi => fi.MEPModel.ElectricalSystems != null)
#endif
                        .ToList();
                }
                else
                {
                    fis = els
                        .Cast<FamilyInstance>()
#if (R21 || R22)
                        .Where(fi => fi.MEPModel.GetElectricalSystems() != null)
#else
                        .Where(fi => fi.MEPModel.ElectricalSystems != null)
#endif
                        .ToList();
                }
            }
            
            
            
            foreach (FamilyInstance fi in fis)
            {
                Parameter ds = fi.LookupParameter(Names.Parameter.DisplaySeparation);
                string typeMark = fi.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString();
                string mark = fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
                string planTag;

                if (ds != null)
                {
                    planTag = typeMark + ds.AsString() + mark;
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

#if (R21 || R22)
                foreach (ElectricalSystem es in fi.MEPModel.GetElectricalSystems())
                {
                    es.LoadName = planTag;
                }
#else
                foreach (ElectricalSystem es in fi.MEPModel.ElectricalSystems)
                {
                    es.LoadName = planTag;
                }
#endif
            }
                
        }

        private void AddNewElementTriggers(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            foreach (ElementId id in addedIds)
            {
                Element el = doc.GetElement(id);
                Parameter par = el.LookupParameter(Names.Parameter.DisplaySeparation);
                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);
                if (par != null)
                {
                    try
                    {
                        UpdaterRegistry.AddTrigger(_updaterId, f, Element.GetChangeTypeParameter(par));
                    }
                    catch (Exception ex)
                    {
                        TaskDialog.Show("Exception", ex.Message + "\n" + ex.StackTrace);
                    }
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
            return "ElecLoadDMU";
        }
    }
}
