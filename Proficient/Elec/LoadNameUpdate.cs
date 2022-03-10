using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using System.Linq;


namespace Proficient.Elec
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class LoadNameUpdate : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var conMech = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                .OfType<FamilyInstance>()
#if (R21 || R22)
                .Where(fi => fi.MEPModel.GetElectricalSystems() != null);
#else
                .Where(fi => fi.MEPModel.ElectricalSystems != null);
#endif

            using (Transaction tx = new Transaction(doc, "updateloadnames"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (FamilyInstance fi in conMech)
                    {
                        string planTag = fi.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString()
                            + fi.LookupParameter("MEI Display Separation").AsString()
                            + fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();

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

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
