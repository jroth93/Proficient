using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Proficient.Forms;
using System;


namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class PipeSpacer : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = revit.Application.ActiveUIDocument.Document;
            ElementId viewid = uidoc.ActiveView.Id;
            View view = doc.GetElement(viewid) as View;

            bool prevErr = false;
            Follower prompt = new Follower(revit);

            while (true)
            {
                LocationCurve loc1, loc2;
                Reference ref1, ref2;

                try
                {
                    prompt.Show();
                    prompt.lbl1.Content = prevErr ? "One or both items picked was not a pipe.\nPlease try again.\n\nPick Anchor Pipe." : "Pick Anchor Pipe";
                    ref1 = uidoc.Selection.PickObject(ObjectType.Element, "Pick Anchor Pipe");
                    prompt.lbl1.Content = "Pick Pipe To Be Moved";
                    ref2 = uidoc.Selection.PickObject(ObjectType.Element, "Pick Pipe To Be Moved");

                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    prompt.Close();
                    return Result.Succeeded;
                }

                loc1 = doc.GetElement(ref1).Location as LocationCurve;
                loc2 = doc.GetElement(ref2).Location as LocationCurve;
                if (loc1 == null || loc2 == null)
                {
                    prevErr = true;
                    continue;
                }
                prevErr = false;

                XYZ linedir = (loc1.Curve as Line).Direction;
                XYZ dirvect = new XYZ(-linedir.Y, linedir.X, 0.0);
                Line intersectline1 = Line.CreateUnbound(new XYZ(loc2.Curve.Evaluate(0.5, true).X, loc2.Curve.Evaluate(0.5, true).Y, 0), linedir);
                Line intersectline2 = Line.CreateUnbound(new XYZ(loc1.Curve.Evaluate(0.5, true).X, loc1.Curve.Evaluate(0.5, true).Y, 0), dirvect);
                intersectline2.Intersect(intersectline1, out IntersectionResultArray resarray);

                XYZ intersectpnt = resarray.get_Item(0).XYZPoint;

                double curdist = intersectpnt.DistanceTo(new XYZ(loc1.Curve.Evaluate(0.5, true).X, loc1.Curve.Evaluate(0.5, true).Y, 0));
                double pipedist = Convert.ToDouble(view.Scale) * Main.Settings.pipeDist / 1152;
                double movedist = curdist - pipedist;
                XYZ movedir = new XYZ(loc1.Curve.Evaluate(0.5, true).X - intersectpnt.X, loc1.Curve.Evaluate(0.5, true).Y - intersectpnt.Y, 0).Normalize();
                XYZ vector = movedist * movedir;

                using (Transaction tx = new Transaction(doc, "Space Piping"))
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        loc2.Move(vector);
                    }

                    tx.Commit();
                }
            }
        }
    }


}
