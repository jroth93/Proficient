namespace Proficient.Electrical;

[Transaction(TransactionMode.Manual)]
internal class LoadNameUpdate : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var doc = revit.Application.ActiveUIDocument.Document;

        var conMech = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .OfType<FamilyInstance>()
#if PRE21
            .Where(fi => fi.MEPModel.ElectricalSystems != null);
#else
            .Where(fi => fi.MEPModel.GetElectricalSystems() != null);
#endif

        using var tx = new Transaction(doc, "Update Load Names");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;
            
        foreach (var fi in conMech)
        {
            var planTag = fi.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString()
                          + fi.LookupParameter("MEI Display Separation").AsString()
                          + fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();

#if PRE21
            foreach (ElectricalSystem es in fi.MEPModel.ElectricalSystems)
                es.LoadName = planTag;
#else
            foreach (var es in fi.MEPModel.GetElectricalSystems())
                es.LoadName = planTag;
#endif
            
        }

        tx.Commit();

        return Result.Succeeded;
    }
}