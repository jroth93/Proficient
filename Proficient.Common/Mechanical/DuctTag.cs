using Autodesk.Revit.DB.Mechanical;
using Proficient.Utilities;

namespace Proficient.Mechanical;


[Transaction(TransactionMode.Manual)]
internal class DuctTag : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {

        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = revit.Application.ActiveUIDocument.Document;
        var view = uiDoc.ActiveView;

        if (view is View3D)
            return Result.Cancelled;

        var selEls =
            uiDoc.Selection
                .GetElementIds()?
                .Select(doc.GetElement)
                .ToList();

        IEnumerable<Duct> ducts;
        IEnumerable<FamilyInstance> fittings;

        if(selEls is not null && selEls.Count != 0)
        {
#if PRE24
            ducts = selEls
                .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctCurves)
                .Where(d => d.Location is LocationCurve lc && lc.Curve.Length > 3 && !Util.IsTagged(doc, view.Id, d))
                .Cast<Duct>();
            fittings = selEls
                .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting)
                .Where(df => df is FamilyInstance && !Util.IsTagged(doc, view.Id, df))
                .Where(f => f.LookupParameter(Names.Parameter.FittingUpDn) is not null)
                .Cast<FamilyInstance>();
#else
            ducts = selEls
                .Where(el => el.Category.Id.Value == (int)BuiltInCategory.OST_DuctCurves)
                .Where(d => d.Location is LocationCurve lc && lc.Curve.Length > 3 && !Util.IsTagged(doc, view.Id, d))
                .Cast<Duct>();
            fittings = selEls
                .Where(el => el.Category.Id.Value == (int)BuiltInCategory.OST_DuctFitting)
                .Where(df => df is FamilyInstance && !Util.IsTagged(doc, view.Id, df))
                .Where(f => f.LookupParameter(Names.Parameter.FittingUpDn) is not null)
                .Cast<FamilyInstance>();
#endif
        }
        else
        {
            ducts = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_DuctCurves)
                .Where(d => d.Location is LocationCurve lc && lc.Curve.Length > 3 && !Util.IsTagged(doc, view.Id, d))
                .Cast<Duct>();

            fittings = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_DuctFitting)
                .Where(df => df is FamilyInstance && !Util.IsTagged(doc, view.Id, df))
                .Where(f => f.LookupParameter(Names.Parameter.FittingUpDn) is not null)
                .Cast<FamilyInstance>();
        }

        #region tag ducts

        var dtId = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .First(f => f.Name == Names.Family.DuctTag)
            .GetFamilySymbolIds()
            .First();
        var dtrId = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .First(f => f.Name == Names.Family.DuctTagRotating)
            .GetFamilySymbolIds()
            .First();
        double minNoLdr = Convert.ToDouble(view.Scale) / 64.0;

        using var dTx = new Transaction(doc, "Add duct tags");
        if (dTx.Start() != TransactionStatus.Started)
            return Result.Failed;

        foreach (var duct in ducts)
        {
            var dc = ((LocationCurve) duct.Location).Curve;
            var ep1 = dc.GetEndPoint(0);
            var ep2 = dc.GetEndPoint(1);

            bool isVert = Math.Abs(Math.Round(ep1.X, 4) - Math.Round(ep2.X, 4)) < 0.01;
            bool isHor = Math.Abs(Math.Round(ep1.Y, 4) - Math.Round(ep2.Y, 4)) < 0.01;
            bool isInPlane = Math.Abs(Math.Round(ep1.Z, 4) - Math.Round(ep2.Z, 4)) < 0.01;

            if (!isInPlane) continue;

            double dWidth = duct.DuctType.Shape == ConnectorProfileType.Round ?
                duct.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble() :
                duct.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble();

            var point = dc.Evaluate(0.5, true);

            var tagId = isVert || isHor ? dtId : dtrId;
            bool ldr = dWidth < minNoLdr;
            var tagOr = isVert && !ldr ? TagOrientation.Vertical : TagOrientation.Horizontal;

            IndependentTag.Create(doc, tagId, view.Id, new Reference(duct), ldr, tagOr, point);
        }
        dTx.Commit();

        #endregion

        #region tag duct drops/rises

        using var dfTx = new Transaction(doc, "Add fitting tags");
        List<PartType> noTag = new() { PartType.Transition, PartType.Cap, PartType.TapAdjustable, PartType.TapPerpendicular };
        if (dfTx.Start() != TransactionStatus.Started) 
            return Result.Failed;

        var dfSymId = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .First(fam => fam.Name == Names.Family.DuctFittingTag)
            .GetFamilySymbolIds()
            .First();

        var cons = fittings
            .Where(f => f.MEPModel is MechanicalFitting {ConnectorManager: { }} mf && !noTag.Contains(mf.PartType))
            .Select(f => f.MEPModel)
            .Cast<MechanicalFitting>()
            .SelectMany(mf => mf.ConnectorManager.Connectors.Cast<Connector>())
            .SelectMany(c => c.AllRefs.Cast<Connector>().Where(cn => cn.ConnectorType == ConnectorType.End))
            .Where(c => c?.Owner.Location is LocationCurve);

        foreach (var c in cons)
        {
            var cf = c.AllRefs
                .Cast<Connector>()
                .FirstOrDefault(cn => cn.ConnectorType == ConnectorType.End && cn.Owner.Location is LocationPoint);

            if(cf is null) continue;
            var f = cf.Owner;

            var zDir = Convert.ToInt32(c.CoordinateSystem.BasisZ.Z);
            var crv = ((LocationCurve)c.Owner.Location).Curve;
            var point = (f.Location as LocationPoint)?.Point;
            if(point is null) continue;

            switch (zDir)
            {
                case -1:
                    double top = Math.Max(crv.GetEndPoint(0).Z, crv.GetEndPoint(1).Z);
                    double viewTop = Util.GetViewBound(doc, view, Util.ViewPlane.Top);
                    if (viewTop - top > 70)
                        viewTop -= 100;
                    if (top <= viewTop)
                        continue;
                    break;
                case 1:
                    double bottom = Math.Min(crv.GetEndPoint(0).Z, crv.GetEndPoint(1).Z);
                    double viewBottom = Util.GetViewBound(doc, view, Util.ViewPlane.Bottom);
                    if (viewBottom - bottom > 70)
                        viewBottom -= 100;
                    if (bottom >= viewBottom)
                        continue;
                    break;
                default:
                    continue;
            }
            IndependentTag.Create(doc, dfSymId, view.Id, new Reference(f), true, TagOrientation.Horizontal, point); 
        }
            
        dfTx.Commit();

        #endregion

        return Result.Succeeded;
    }
}