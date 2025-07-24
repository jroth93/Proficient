using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Proficient.Forms;
using Proficient.Utilities;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class PipeTag : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;
        var viewId = view.Id;

        if (view is View3D)
            return Result.Cancelled;

        var selEls =
            uiDoc.Selection
                .GetElementIds()?
                .Select(doc.GetElement)
                .ToList();

        IEnumerable<Element> pipes;
        IEnumerable<FamilyInstance> fittings;

        if (selEls is not null && selEls.Any())
        {
            pipes = selEls
                .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeCurves)
                .Where(p => p.Location is LocationCurve lc && lc.Curve.Length > 3 && !Util.IsTagged(doc, view.Id, p));
            fittings = selEls
                .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                .Where(pf => pf is FamilyInstance && !Util.IsTagged(doc, view.Id, pf))
                .Where(f => f.LookupParameter(Names.Parameter.FittingUpDn) is not null)
                .Cast<FamilyInstance>();
        }
        else
        {
            pipes = new FilteredElementCollector(doc, viewId)
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .Where(p => p.Location is LocationCurve lc && lc.Curve.Length > 3 && !Util.IsTagged(doc, view.Id, p));

            fittings =
                new FilteredElementCollector(doc, viewId)
                    .OfCategory(BuiltInCategory.OST_PipeFitting)
                    .Where(pf => pf is FamilyInstance && !Util.IsTagged(doc, view.Id, pf))
                    .Where(f => f.LookupParameter(Names.Parameter.FittingUpDn) is not null)
                    .Cast<FamilyInstance>();
                
        }

        var ldr = true;

        BlankViewModel bvm = new();
        var mousePos = Mouse.GetCursorPosition();
        bvm.SetLocation(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y));
        bvm.AddButton("smallBtn", "Leader", () => ldr = true, true, true);
        bvm.AddButton("smallBtn", "No Leader", () => ldr = false, true, true);

        if (!bvm.ShowWindow(true) ?? false)
            return Result.Cancelled;

        #region tag pipes

        var ptId = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .First(f => f.Name == Names.Family.PipeTag)
            .GetFamilySymbolIds()
            .First();
        var ptrId = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .First(f => f.Name == Names.Family.PipeTagRotating)
            .GetFamilySymbolIds()
            .First();

        using Transaction pTx = new(doc, "Add pipe tags");
        if (pTx.Start() != TransactionStatus.Started)
            return Result.Failed;
        
        foreach (var pipe in pipes)
        {
            var pc = ((LocationCurve) pipe.Location).Curve;
            var ep1 = pc.GetEndPoint(0);
            var ep2 = pc.GetEndPoint(1);

            bool isVert = Math.Abs(Math.Round(ep1.X, 4) - Math.Round(ep2.X, 4)) < 0.01;
            bool isHor = Math.Abs(Math.Round(ep1.Y, 4) - Math.Round(ep2.Y, 4)) < 0.01;
            bool isInPlane = Math.Abs(Math.Round(ep1.Z, 4) - Math.Round(ep2.Z, 4)) < 0.01;

            if (!isInPlane) continue;

            var point = pc.Evaluate(0.5, true);
            
            var tagId = isVert || isHor || ldr ? ptId : ptrId;
            var tagOr = !ldr && isVert ? TagOrientation.Vertical : TagOrientation.Horizontal;

            IndependentTag.Create(doc, tagId, viewId, new Reference(pipe), ldr, tagOr, point);
        }
        
        pTx.Commit();

        #endregion

        #region tag pipe drops/rises

        using Transaction pfTx = new (doc, "Add fitting tags");
        List<PartType> noTag = new() { PartType.Transition, PartType.Cap, PartType.TapAdjustable, PartType.TapPerpendicular };
        if (pfTx.Start() != TransactionStatus.Started) 
            return Result.Failed;
        
        var pfTagId = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .First(fam => fam.Name == Names.Family.PipeFittingTag)
            .GetFamilySymbolIds()
            .First();

        var cons = fittings
            .Where(f => f.MEPModel is MechanicalFitting {ConnectorManager: {}} mf && !noTag.Contains(mf.PartType))
            .Select(f => f.MEPModel)
            .Cast<MechanicalFitting>()
            .SelectMany(mf => mf.ConnectorManager.Connectors.Cast<Connector>())
            .Select(c => c.AllRefs.Cast<Connector>().FirstOrDefault(cn => cn.ConnectorType == ConnectorType.End))
            .Where(c => c?.Owner.Location is LocationCurve);

        foreach (var c in cons)
        {
            var cf = c.AllRefs
                .Cast<Connector>()
                .FirstOrDefault(cn => cn.ConnectorType == ConnectorType.End && cn.Owner.Location is LocationPoint);

            if (cf is null) continue;
            var f = cf.Owner;

            var zDir = Convert.ToInt32(c.CoordinateSystem.BasisZ.Z);
            var crv = ((LocationCurve)c.Owner.Location).Curve;
            var point = ((LocationPoint)f.Location).Point;
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
            IndependentTag.Create(doc, pfTagId, viewId, new Reference(f), true, TagOrientation.Horizontal, point);
        }
        pfTx.Commit();
        

        #endregion

        return Result.Succeeded;
    }
}