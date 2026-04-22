using Proficient.Utilities;

namespace Proficient.Mechanical;

internal class DuctFittingDmu : IUpdater
{
    private static UpdaterId _updaterId = new(new AddInId(Main.AppId), Names.Guids.DuctFittingDmu);

    public void Execute(UpdaterData data)
    {
        if (data is null) return;
        var doc = data.GetDocument();

        if (data.GetAddedElementIds().Count != 0)
        {
            foreach (var id in data.GetAddedElementIds())
            {
                var el = doc.GetElement(id);
                var famName = (el as FamilyInstance)?.Symbol.FamilyName;
                if (famName != Names.Family.MiteredElbow) 
                    continue;

                var sysClass = el.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
                if (sysClass is "Return Air" or "Exhaust Air")
                    el.LookupParameter(Names.Parameter.DisplayVanes).Set(0);
            }
        }

        if (data.GetModifiedElementIds().Count == 0) 
            return;
            
        foreach (var id in data.GetModifiedElementIds())
        {
            var el = doc.GetElement(id);
            var famName = (el as FamilyInstance)?.Symbol.FamilyName;
            if (famName != Names.Family.MiteredElbow) 
                continue;

            var sysClass = el.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString();
            if (sysClass is "Return Air" or "Exhaust Air")
                el.LookupParameter(Names.Parameter.DisplayVanes).Set(0);
        }
    }

    public string GetAdditionalInformation() => "Josh Roth";
    public ChangePriority GetChangePriority() => ChangePriority.MEPSystems;
    public UpdaterId GetUpdaterId() => _updaterId;
    public string GetUpdaterName() => "DuctFittingDMU";
}