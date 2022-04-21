using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SetFittingUpDn : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;
            var dfec = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DuctFitting);
            var pfec = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_PipeFitting);
            List<Element> fList = dfec.ToElements() as List<Element>;
            fList.AddRange(pfec.ToElements());
            fList = fList.FindAll(x => (x as FamilyInstance) != null);


            using (Transaction tx = new Transaction(doc, "Set up/dn parameter"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (FamilyInstance f in fList)
                    {
                        List<PartType> noSet = new List<PartType>() { PartType.Transition, PartType.Cap, PartType.TapAdjustable, PartType.TapPerpendicular };

                        MechanicalFitting mf = f.MEPModel as MechanicalFitting;
                        if (mf.ConnectorManager != null && !noSet.Contains(mf.PartType))
                        {
                            foreach (Connector c in mf.ConnectorManager.Connectors)
                            {
                                Parameter fudPar = f.LookupParameter(Names.Parameter.FittingUpDn);
                                if (c.CoordinateSystem.BasisZ.Z == 1 && fudPar != null)
                                {
                                    fudPar.Set(1);
                                }
                                else if (c.CoordinateSystem.BasisZ.Z == -1 && fudPar != null)
                                {
                                    fudPar.Set(0);
                                }
                            }
                        }
                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
