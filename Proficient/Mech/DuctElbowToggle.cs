using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class DuctElbowToggle : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<RoutingPreferenceRule> rprList = new List<RoutingPreferenceRule>();
            FilteredElementCollector ductTypeFec = new FilteredElementCollector(doc).OfClass(typeof(DuctType));

            using (Transaction tx = new Transaction(doc, "Duct Elbow Toggle"))
            {
                string alert = String.Empty;
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (DuctType dt in ductTypeFec)
                    {
                        if (dt.Name == "Rectangular Duct")
                        {
                            rprList.Clear();
                            RoutingPreferenceManager rpm = dt.RoutingPreferenceManager;
                            int count = rpm.GetNumberOfRules(RoutingPreferenceRuleGroupType.Elbows);
                            if (count > 1)
                            {

                                for (int i = 1; i <= count; i++)
                                {
                                    rprList.Add(rpm.GetRule(RoutingPreferenceRuleGroupType.Elbows, 0));
                                    rpm.RemoveRule(RoutingPreferenceRuleGroupType.Elbows, 0);
                                }

                                RoutingPreferenceRule temp = rprList[0];
                                rprList[0] = rprList[1];
                                rprList[1] = temp;

                                for (int i = 1; i <= rprList.Count; i++)
                                {
                                    rpm.AddRule(RoutingPreferenceRuleGroupType.Elbows, rprList[i - 1]);
                                }
                                FamilySymbol fs = doc.GetElement(rprList[0].MEPPartId) as FamilySymbol;

                                alert += $"{dt.Name}: {fs.FamilyName} - {fs.Name}\n";
                            }
                        }


                    }
                }

                tx.Commit();

                Util.BalloonTip("Routing Preferences Updated", alert, String.Empty);
            }

            return Result.Succeeded;
        }
    }
}
