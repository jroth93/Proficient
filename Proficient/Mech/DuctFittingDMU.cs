using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Electrical;

namespace Proficient.Elec
{
    public class DuctFittingDMU : IUpdater
    {
        static UpdaterId _updaterId;
        static Document doc;

        public DuctFittingDMU()
        {
            _updaterId = new UpdaterId(new AddInId(Main.appId), new Guid("76364FDC-B97B-4D3A-BDA7-EC6DD273B60F"));
        }

        public void Execute(UpdaterData data)
        {
            doc = data.GetDocument();

            if (data.GetAddedElementIds().Any())
            {
                foreach (ElementId id in data.GetAddedElementIds())
                {
                    Element el = doc.GetElement(id);
                    string sysClass = el.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                    string famName = (el as FamilyInstance)?.Symbol.FamilyName;
                    if (famName == Names.Family.MiteredElbow)
                    {
                        if (sysClass == "Return Air" || sysClass == "Exhaust Air")
                        {
                            el.LookupParameter(Names.Parameter.DisplayVanes).Set(0);
                        }
                    }
                }
            }

            if (data.GetModifiedElementIds().Any())
            {
                foreach (ElementId id in data.GetModifiedElementIds())
                {
                    Element el = doc.GetElement(id);
                    string sysClass = el.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                    string famName = (el as FamilyInstance)?.Symbol.FamilyName;
                    if (famName == Names.Family.MiteredElbow)
                    {
                        if (sysClass == "Return Air" || sysClass == "Exhaust Air")
                        {
                            el.LookupParameter(Names.Parameter.DisplayVanes).Set(0);
                        }
                    }
                }
            }

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
            return "DuctFittingDMU";
        }
    }
}
