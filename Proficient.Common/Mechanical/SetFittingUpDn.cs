using Autodesk.Revit.DB.Mechanical;
using Proficient.Utilities;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class SetFittingUpDn : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        if (view is View3D)
            return Result.Cancelled;

        var selEls = 
            uiDoc.Selection
                .GetElementIds()?
                .Select(doc.GetElement)
                .ToList();

#if PRE24
        static bool IsFitting(Element el) => 
            el.Category.Id.IntegerValue is (int)BuiltInCategory.OST_DuctFitting or (int)BuiltInCategory.OST_PipeFitting;
#else
        static bool IsFitting(Element el) =>
            el.Category.Id.Value is (long)BuiltInCategory.OST_DuctFitting or (long)BuiltInCategory.OST_PipeFitting;
#endif

        IEnumerable<FamilyInstance> fittings;
        if(selEls is not null && selEls.Any())
        {
            fittings = selEls
                .Where(IsFitting)
                .Where(f => f is FamilyInstance)
                .Cast<FamilyInstance>();
        }
        else
        {
            fittings = new FilteredElementCollector(doc, view.Id)
                .OfClass(typeof(FamilyInstance))
                .Where(IsFitting)
                .Cast<FamilyInstance>();
        }

        var noSet = new List<PartType> { PartType.Transition, PartType.Cap, PartType.TapAdjustable, PartType.TapPerpendicular };

        var cons = fittings
            .Where(f => f.MEPModel is MechanicalFitting {ConnectorManager: { }} mf && !noSet.Contains(mf.PartType))
            .Select(f => f.MEPModel)
            .Cast<MechanicalFitting>()
            .SelectMany(mf => mf.ConnectorManager.Connectors.Cast<Connector>());

        using Transaction tx = new(doc, "Set up/dn parameter");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;

        foreach (var c in cons)
        {
            var par = c.Owner.LookupParameter(Names.Parameter.FittingUpDn);
            if (par == null) continue;

            var zDir = Convert.ToInt32(c.CoordinateSystem.BasisZ.Z);

            switch (zDir)
            {
                case 1:
                    par.Set(1);
                    break;
                case -1:
                    par.Set(0);
                    break;
            }
            
        }

        tx.Commit();

        return Result.Succeeded;
    }
}