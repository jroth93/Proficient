using Autodesk.Revit.DB.Mechanical;
using Proficient.Utilities;
using RPRGT = Autodesk.Revit.DB.RoutingPreferenceRuleGroupType;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class DuctElbowToggle : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var dtEl = new FilteredElementCollector(doc)
            .OfClass(typeof(DuctType))
            .FirstOrDefault(d => d.Name == "Rectangular Duct");
        if (dtEl is not DuctType dt) return Result.Failed;

        var sel = uiDoc.Selection.GetElementIds();
        var rpm = dt.RoutingPreferenceManager;
        var elbowRule = rpm.GetRule(RPRGT.Elbows, 0);


        if (sel.Any())
        {
            var elbows =
                sel.Select(doc.GetElement)
                    .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting)
                    .Cast<FamilyInstance>()
                    .Select(fi => fi.MEPModel)
                    .OfType<MechanicalFitting>()
                    .Where(mf => mf.PartType == PartType.Elbow)
                    .Select(mf  => mf.ConnectorManager.Owner);

            var el1Id = elbowRule.MEPPartId;
            var el2Id = rpm.GetRule(RPRGT.Elbows, 1).MEPPartId;

                
            foreach (var elbow in elbows)
            {
                using var tx = new Transaction(doc, "Duct Elbow Toggle");
                if (tx.Start() != TransactionStatus.Started) return Result.Failed;
                    
                var elbId = elbow.GetTypeId();
                if (elbId == el1Id)
                {
                    elbow.ChangeTypeId(el2Id);
                }
                else if (elbId == el2Id)
                {
                    elbow.ChangeTypeId(el1Id);
                }

                tx.Commit();
            }
        }
        else
        {
            var alert = string.Empty;

            using var tx = new Transaction(doc, "Duct Elbow Toggle");
            if (tx.Start() != TransactionStatus.Started) return Result.Failed;
                
            if (rpm.GetNumberOfRules(RPRGT.Elbows) > 1)
            {
                rpm.RemoveRule(RPRGT.Elbows, 0);
                rpm.AddRule(RPRGT.Elbows, elbowRule, 1);

                var fs = doc.GetElement(rpm.GetRule(RPRGT.Elbows, 0).MEPPartId) as FamilySymbol;

                alert += $"{dt.Name}: {fs?.FamilyName} - {fs?.Name}\n";
            }

            tx.Commit();

            Util.BalloonTip("Routing Preferences Updated", alert, string.Empty);
        }

        return Result.Succeeded;
    }
}