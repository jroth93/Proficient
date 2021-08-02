using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class FlipElements : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<ElementId> selectedIds = uidoc.Selection.GetElementIds() as IList<ElementId>;

            if (selectedIds.Count > 0)
            {

                foreach (ElementId elemid in selectedIds)
                {
                    using (Transaction tx = new Transaction(doc, "Flip"))
                    {
                        try
                        {
                            if (tx.Start() == TransactionStatus.Started)
                            {
                                FamilyInstance faminst = doc.GetElement(elemid) as FamilyInstance;
                                if (faminst.CanFlipFacing) { faminst.flipFacing(); }
                                else if ((faminst.MEPModel as MechanicalFitting).PartType.ToString() == "Tee") { FlipTee(uidoc, doc, faminst); }
                            }
                        }
                        catch (NullReferenceException)
                        {
                            continue;
                        }
                        tx.Commit();
                    }
                }
                return Autodesk.Revit.UI.Result.Succeeded;
            }
            while (true)
            {
                Reference reference = null;
                FamilyInstance faminst = null;
                try
                {
                    reference = uidoc.Selection.PickObject(ObjectType.Element);
                    faminst = doc.GetElement(reference) as FamilyInstance;
                    using (Transaction tx = new Transaction(doc, "Flip"))
                    {
                        if (tx.Start() == TransactionStatus.Started)
                        {
                            if (faminst.CanFlipFacing) { faminst.flipFacing(); }
                            if ((faminst.MEPModel as MechanicalFitting).PartType.ToString() == "Tee") { FlipTee(uidoc, doc, faminst); }
                        }
                        tx.Commit();
                    }
                }
                catch (NullReferenceException)
                {
                    continue;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
            }
        }

        static void FlipTee(UIDocument uidoc, Document doc, FamilyInstance faminst)
        {
            ConnectorSet teeconset = faminst.MEPModel.ConnectorManager.Connectors;
            IList<Connector> conset = new List<Connector>();
            IList<LocationCurve> locset = new List<LocationCurve>();
            foreach (Connector con in teeconset)
            {
                foreach (Connector refcon in con.AllRefs)
                {
                    conset.Add(refcon);
                    locset.Add(refcon.Owner.Location as LocationCurve);
                }
            }

            IList<XYZ> dirset = new List<XYZ>();

            foreach (LocationCurve loccurve in locset)
            {
                dirset.Add((loccurve.Curve as Line).Direction);
            }

            Connector con1 = null, con2 = null, con3 = null;
            if (Math.Round(dirset[0].X, 3) == Math.Round(dirset[1].X, 3) && Math.Round(dirset[0].Y, 3) == Math.Round(dirset[1].Y, 3))
            {
                con1 = conset[0];
                con2 = conset[1];
                con3 = conset[2];
            }

            else if (Math.Round(dirset[1].X, 3) == Math.Round(dirset[2].X, 3) && Math.Round(dirset[1].Y, 3) == Math.Round(dirset[2].Y, 3))
            {
                con1 = conset[1];
                con2 = conset[2];
                con3 = conset[0];
            }

            else if (Math.Round(dirset[0].X, 3) == Math.Round(dirset[2].X, 3) && Math.Round(dirset[0].Y, 3) == Math.Round(dirset[2].Y, 3))
            {
                con1 = conset[0];
                con2 = conset[2];
                con3 = conset[1];
            }

            XYZ originalhand = faminst.HandOrientation;
            doc.Delete(faminst.Id);
            FamilyInstance newtee = uidoc.Document.Create.NewTeeFitting(con1, con2, con3);
            if (Math.Round(newtee.HandOrientation.X, 3) == Math.Round(originalhand.X, 3) && Math.Round(newtee.HandOrientation.Y, 3) == Math.Round(originalhand.Y, 3))
            {
                doc.Delete(newtee.Id);
                uidoc.Document.Create.NewTeeFitting(con2, con1, con3);
            }
            return;
        }
    }
}