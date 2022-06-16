using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Proficient.Forms;
using System;


namespace Proficient.Mech
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

            try
            {
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

                    XYZ lineDir = (loc1.Curve as Line).Direction;
                    XYZ vec = new XYZ();
                    double pipeDist = Convert.ToDouble(view.Scale) * Main.Settings.pipeDist / 1152;

                    if (Math.Abs(lineDir.Z) == 1)
                    {
                        XYZ pt1 = loc1.Curve.GetEndPoint(0);
                        XYZ pt2 = loc2.Curve.GetEndPoint(0);
                        double xDiff = pt1.X - pt2.X;
                        double yDiff = pt1.Y - pt2.Y;
                        if(Math.Abs(xDiff) > Math.Abs(yDiff))
                        {
                            vec = new XYZ(xDiff - Math.Sign(xDiff) * pipeDist, yDiff, 0);
                        }
                        else
                        {
                            vec = new XYZ(xDiff, yDiff - Math.Sign(yDiff) * pipeDist, 0);
                        }
                    }
                    else
                    {
                        XYZ dirVec = new XYZ(-lineDir.Y, lineDir.X, 0.0);
                        Line intLine1 = Line.CreateUnbound(new XYZ(loc2.Curve.Evaluate(0.5, true).X, loc2.Curve.Evaluate(0.5, true).Y, 0), new XYZ(lineDir.X, lineDir.Y, 0.0));
                        Line intLine2 = Line.CreateUnbound(new XYZ(loc1.Curve.Evaluate(0.5, true).X, loc1.Curve.Evaluate(0.5, true).Y, 0), dirVec);
                        intLine2.Intersect(intLine1, out IntersectionResultArray resarray);
                        XYZ intPnt = resarray.get_Item(0).XYZPoint;

                        double curDist = intPnt.DistanceTo(new XYZ(loc1.Curve.Evaluate(0.5, true).X, loc1.Curve.Evaluate(0.5, true).Y, 0));
                        double moveDist = curDist - pipeDist;
                        XYZ moveDir = new XYZ(loc1.Curve.Evaluate(0.5, true).X - intPnt.X, loc1.Curve.Evaluate(0.5, true).Y - intPnt.Y, 0).Normalize();
                        vec = moveDist * moveDir;
                    }
                    

                    using (Transaction tx = new Transaction(doc, "Space Piping"))
                    {
                        if (tx.Start() == TransactionStatus.Started)
                        {
                            loc2.Move(vec);
                        }

                        tx.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                prompt.Close();
                throw e;
            }
        }
    }


}
