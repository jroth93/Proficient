using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace Proficient
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
                        MechanicalFitting mf = f.MEPModel as MechanicalFitting;
                        if (mf.ConnectorManager != null)
                        {
                            foreach (Connector c in mf.ConnectorManager.Connectors)
                            {
                                if (c.CoordinateSystem.BasisZ.Z == 1)
                                {
                                    f.LookupParameter(Names.Parameter.FittingUpDn).Set(1);
                                }
                                else if (c.CoordinateSystem.BasisZ.Z == -1)
                                {
                                    f.LookupParameter(Names.Parameter.FittingUpDn).Set(0);
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
