using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using System.Linq;


namespace Proficient.Elec
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PanelUtil : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            var eleqpcol = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .Where(x => x.GetType() == typeof(FamilyInstance));
            var picol = new FilteredElementCollector(doc).OfClass(typeof(PanelScheduleSheetInstance)).OrderBy(x => x.Name);
            var pscol = new FilteredElementCollector(doc).OfClass(typeof(PanelScheduleView)).OrderBy(x => x.Name);

            ElecPanelDataFrm epdf = new ElecPanelDataFrm();

            foreach (Element panel in eleqpcol)
            {
                string placedsheet = "None";
                string hassched = "No";
                foreach (PanelScheduleView ps in pscol)
                {
                    if (panel.Name == ps.Name)
                    {
                        hassched = "Yes";
                        foreach (PanelScheduleSheetInstance pi in picol)
                        {
                            if (pi.ScheduleId == ps.Id)
                            {
                                placedsheet = placedsheet == "None" ? doc.GetElement(pi.OwnerViewId).get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString()
                                    : placedsheet + ", " + doc.GetElement(pi.OwnerViewId).get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
                            }
                        }
                    }

                }
                string[] row = { panel.Name, hassched, placedsheet };
                epdf.dgv.Rows.Add(row);
            }
            epdf.Text = "Panel Checker";

            epdf.Show();

            return Result.Succeeded;
        }
    }
}
