using Autodesk.Revit.DB.Mechanical;
using Proficient.Utilities;
using RPRGT = Autodesk.Revit.DB.RoutingPreferenceRuleGroupType;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class DamperToggle : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var dtFec = new FilteredElementCollector(doc).OfClass(typeof(DuctType)).Cast<DuctType>();
        var sel = uiDoc.Selection.GetElementIds();

        if (sel.Any())
        {
            var taps =
                sel.Select(id => doc.GetElement(id))
                    .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting)
                    .Cast<FamilyInstance>()
                    .Select(fi => fi.MEPModel)
                    .Cast<MechanicalFitting>()
                    .Where(mf => mf.PartType == PartType.TapPerpendicular)
                    .Select(mf => mf.ConnectorManager.Owner);

            var tapTuples = 
                dtFec.Where(dt => dt.RoutingPreferenceManager.GetNumberOfRules(RPRGT.Junctions) > 1)
                    .Select(dt => 
                        (dt.RoutingPreferenceManager.GetRule(RPRGT.Junctions, 0).MEPPartId, 
                            dt.RoutingPreferenceManager.GetRule(RPRGT.Junctions, 1).MEPPartId))
                    .ToList();

            foreach (var tap in taps)
            {
                var newType = tapTuples.Any(tt => tt.Item1 == tap.GetTypeId()) ?
                    tapTuples.First(tt => tt.Item1 == tap.GetTypeId()).Item2 :
                    tapTuples.First(tt => tt.Item2 == tap.GetTypeId()).Item1;

                if (newType == null) 
                    continue;

                using var tx = new Transaction(doc, "Damper Toggle");
                if (tx.Start() != TransactionStatus.Started)
                    return Result.Failed;
                tap.ChangeTypeId(newType);
                tx.Commit();

            }
        }
        else
        {
            var alert = string.Empty;

            using var tx = new Transaction(doc, "Damper Toggle");
            if (tx.Start() != TransactionStatus.Started)
                return Result.Failed;
                
            foreach (var dt in dtFec)
            {
                var rpm = dt.RoutingPreferenceManager;
                if (rpm.GetNumberOfRules(RPRGT.Junctions) <= 1) 
                    continue;
                var juncRule = rpm.GetRule(RPRGT.Junctions, 0);
                rpm.RemoveRule(RPRGT.Junctions, 0);
                rpm.AddRule(RPRGT.Junctions, juncRule, 1);

                var fs = doc.GetElement(rpm.GetRule(RPRGT.Junctions, 0).MEPPartId) as FamilySymbol;

                alert += $"{dt.Name}: {fs?.FamilyName} - {fs?.Name}\n";
            }

            tx.Commit();

            Util.BalloonTip("Routing Preferences Updated", alert, string.Empty);
        }

        return Result.Succeeded;
    }
}