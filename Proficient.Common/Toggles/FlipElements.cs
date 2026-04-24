using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI.Selection;
using Proficient.Utilities;

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
                Flip(doc, uiDoc, eid);
            }
            return Result.Succeeded;
        }

        while (true)
        {
            try
            {
                var eid = uiDoc.Selection.PickObject(ObjectType.Element).ElementId;
                Flip(doc, uiDoc, eid);
            }
            catch (NullReferenceException) { }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Succeeded;
            }
        }
    }

    private static void Flip(Document doc, UIDocument uiDoc, ElementId eid)
    {

#if PRE24
        var vsId = new ElementId(eid.IntegerValue + 1);
#else
        var vsId = new ElementId(eid.Value + 1);
#endif
        if (doc.GetElement(vsId) is ViewSection sec)
        {
            FlipSection(doc, sec);
            return;
        }

        var el = doc.GetElement(eid);

#if PRE24
        var elCatId = el.Category.Id.IntegerValue;
#else
        var elCatId = el.Category.Id.Value;
#endif
        if ((BuiltInCategory)elCatId == BuiltInCategory.OST_DuctTerminalTags)
        {
            FlipAtTag(doc, el);
            return;
        }

        try
        {
            using var tx = new Transaction(doc, "Flip Element");
            if (tx.Start() != TransactionStatus.Started)
                return;

            if (el is not FamilyInstance famInst)
                return;

            if (famInst.CanFlipFacing)
                famInst.flipFacing();
            else if (famInst.MEPModel is MechanicalFitting { PartType: PartType.Tee })
                FlipTee(uiDoc, doc, famInst);

            tx.Commit();

        }
        catch (NullReferenceException) { }
    }

    private static void FlipSection(Document doc, ViewSection sec)
    {            
        var p = Plane.CreateByNormalAndOrigin(sec.ViewDirection, sec.Origin);
            
        using var tx = new Transaction(doc, "Flip Section");
        if (tx.Start() == TransactionStatus.Started)
            ElementTransformUtils.MirrorElements(doc, [sec.Id], p, false);
        tx.Commit();
    }

    private static void FlipAtTag(Document doc, Element el)
    {
        var elTypeId = el.GetTypeId();
        var elType = doc.GetElement(elTypeId);

        if (elType is not FamilySymbol elFs)
            return;
        if (!elFs.FamilyName.Contains("MEI Mech Tag AT"))
            return;

        var famTypeIds = elFs.Family.GetFamilySymbolIds();

        //find the type that has the same rattail and length but with opposite direction
        var newType = famTypeIds
            .Select(doc.GetElement)
            .Where(t =>
                t.GetParameters(Names.Parameter.AtReturnAirTag).First().AsInteger() ==
                elType.GetParameters(Names.Parameter.AtReturnAirTag).First().AsInteger())
            .Where(t =>
                t.GetParameters(Names.Parameter.AtRhTag).First().AsInteger() !=
                elType.GetParameters(Names.Parameter.AtRhTag).First().AsInteger())
            .FirstOrDefault(t =>
                Math.Abs(t.GetParameters(Names.Parameter.AtLdrLength).First().AsDouble() - elType.GetParameters(Names.Parameter.AtLdrLength).First().AsDouble()) < 0.001);

        if (newType is null) return;

        using var tx = new Transaction(doc, "Test");
        if (tx.Start() == TransactionStatus.Started)
            el.ChangeTypeId(newType.Id);
        tx.Commit();
    }

    private static void FlipTee(UIDocument uiDoc, Document doc, FamilyInstance fi)
    {
        var teeCons = fi.MEPModel.ConnectorManager.Connectors;
        var cons = new List<Connector>();
        var lcs = new List<LocationCurve>();
        foreach (Connector con in teeCons)
        {
            foreach (Connector refCon in con.AllRefs)
            {
                cons.Add(refCon);

                if(refCon.Owner.Location is LocationCurve lc)
                {
                    lcs.Add(lc);
                }
                else
                {
                    return;
                }
            }
        }

        var dirSet = new List<XYZ>();

        foreach (var lc in lcs)
        {
            if (lc.Curve is Line l)
            {
                dirSet.Add(l.Direction);
            }
            else
            {
                return;
            }
                
        }

        Connector? con1 = null, con2 = null, con3 = null;
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