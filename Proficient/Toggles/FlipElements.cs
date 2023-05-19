using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI.Selection;

namespace Proficient.Toggles;

[Transaction(TransactionMode.Manual)]
internal class FlipElements : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var selIds = uiDoc.Selection.GetElementIds();

        if (selIds is not null && selIds.Count > 0)
        {
            foreach (var eid in selIds)
            {
                if (doc.GetElement(new ElementId(eid.IntegerValue + 1)) is ViewSection)
                {
                    FlipSection(doc, eid);
                    continue;
                }
                try
                {
                    using var tx = new Transaction(doc, "Flip Element");
                    if (tx.Start() != TransactionStatus.Started)
                        return Result.Failed;

                    if (doc.GetElement(eid) is not FamilyInstance famInst)
                        continue;

                    if (famInst.CanFlipFacing)
                        famInst.flipFacing();
                    else if ((famInst.MEPModel as MechanicalFitting)?.PartType == PartType.Tee)
                        FlipTee(uiDoc, doc, famInst);

                    tx.Commit();

                }
                catch (NullReferenceException) {  }
            }
            return Result.Succeeded;
        }

        while (true)
        {
            try
            {
                var eid = uiDoc.Selection.PickObject(ObjectType.Element).ElementId;

                if (doc.GetElement(new ElementId(eid.IntegerValue + 1)) is ViewSection)
                {
                    FlipSection(doc, new ElementId(eid.IntegerValue + 1));
                    continue;
                }

                using var tx = new Transaction(doc, "Flip Element");
                if (tx.Start() != TransactionStatus.Started)
                    return Result.Failed;

                if (doc.GetElement(eid) is FamilyInstance famInst)
                {
                    if (famInst.CanFlipFacing)
                        famInst.flipFacing();
                    else if ((famInst.MEPModel as MechanicalFitting)?.PartType == PartType.Tee)
                        FlipTee(uiDoc, doc, famInst);
                }
                tx.Commit();
                
            }
            catch (NullReferenceException) { }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Succeeded;
            }
        }
    }

    private static void FlipSection(Document doc, ElementId eid)
    {
        if (doc.GetElement(new ElementId(eid.IntegerValue + 1)) is not View sec) return;
            
        var p = Plane.CreateByNormalAndOrigin(sec.ViewDirection, sec.Origin);
            
        using var tx = new Transaction(doc, "Flip Section");
        if (tx.Start() == TransactionStatus.Started)
            ElementTransformUtils.MirrorElements(doc, new List<ElementId>{eid}, p, false);
        tx.Commit();
    }

    private static void FlipTee(UIDocument uiDoc, Document doc, FamilyInstance fi)
    {
        var teeCons = fi.MEPModel.ConnectorManager.Connectors;
        var cons = new List<Connector>();
        var locs = new List<LocationCurve>();
        foreach (Connector con in teeCons)
        {
            foreach (Connector refCon in con.AllRefs)
            {
                cons.Add(refCon);
                locs.Add(refCon.Owner.Location as LocationCurve);
            }
        }

        var dirSet = new List<XYZ>();

        foreach (var lc in locs)
        {
            dirSet.Add((lc.Curve as Line).Direction);
        }

        Connector con1 = null, con2 = null, con3 = null;
        if (Math.Round(dirSet[0].X, 3) == Math.Round(dirSet[1].X, 3) && Math.Round(dirSet[0].Y, 3) == Math.Round(dirSet[1].Y, 3))
        {
            con1 = cons[0];
            con2 = cons[1];
            con3 = cons[2];
        }

        else if (Math.Round(dirSet[1].X, 3) == Math.Round(dirSet[2].X, 3) && Math.Round(dirSet[1].Y, 3) == Math.Round(dirSet[2].Y, 3))
        {
            con1 = cons[1];
            con2 = cons[2];
            con3 = cons[0];
        }

        else if (Math.Round(dirSet[0].X, 3) == Math.Round(dirSet[2].X, 3) && Math.Round(dirSet[0].Y, 3) == Math.Round(dirSet[2].Y, 3))
        {
            con1 = cons[0];
            con2 = cons[2];
            con3 = cons[1];
        }

        XYZ originalhand = fi.HandOrientation;
        doc.Delete(fi.Id);
        FamilyInstance newtee = uiDoc.Document.Create.NewTeeFitting(con1, con2, con3);
        if (Math.Round(newtee.HandOrientation.X, 3) == Math.Round(originalhand.X, 3) && Math.Round(newtee.HandOrientation.Y, 3) == Math.Round(originalhand.Y, 3))
        {
            doc.Delete(newtee.Id);
            uiDoc.Document.Create.NewTeeFitting(con2, con1, con3);
        }
        return;
    }
}