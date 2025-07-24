using Autodesk.Revit.DB.Electrical;
using Proficient.Forms;


namespace Proficient.Electrical;

[Transaction(TransactionMode.Manual)]
internal class PanelUtil : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var eeFec = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
            .Where(x => x.GetType() == typeof(FamilyInstance));
        var piFec = new FilteredElementCollector(doc)
            .OfClass(typeof(PanelScheduleSheetInstance))
            .OrderBy(x => x.Name)
            .Cast<PanelScheduleSheetInstance>()
            .ToList();
        var psFec = new FilteredElementCollector(doc)
            .OfClass(typeof(PanelScheduleView))
            .OrderBy(x => x.Name)
            .Cast<PanelScheduleView>()
            .ToList();

        var epdf = new ElecPanelDataFrm();

        foreach (var panel in eeFec)
        {
            var placedSheet = "None";
            var hasSched = "No";
            foreach (var ps in psFec.Where(ps => panel.Name == ps.Name))
            {
                hasSched = "Yes";
                placedSheet = piFec
                    .Where(pi => pi.ScheduleId == ps.Id)
                    .Aggregate(placedSheet, (current, pi) => current == "None"
                        ? doc.GetElement(pi.OwnerViewId).get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString()
                        : current + ", " + doc.GetElement(pi.OwnerViewId).get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString());
            }
            object[] row = { panel.Name, hasSched, placedSheet };
            epdf.dgv.Rows.Add(row);
        }
        epdf.Text = @"Panel Checker";

        epdf.Show();

        return Result.Succeeded;
    }
}