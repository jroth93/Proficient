using Autodesk.Revit.UI.Selection;
using Proficient.Forms;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class PipeSpacer : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = revit.Application.ActiveUIDocument.Document;
        var view = uiDoc.ActiveView;

        bool prevErr = false;
        var prompt = new Follower(revit);

        try
        {
            while (true)
            {
                ElementId elId1, elId2;

                try
                {
                    prompt.Show();
                    prompt.lbl1.Content = prevErr ? "One or both items picked was not a pipe.\nPlease try again.\n\nPick Anchor Pipe." : "Pick Anchor Pipe";
                    elId1 = uiDoc.Selection.PickObject(ObjectType.Element, "Pick Anchor Pipe").ElementId;
                    prompt.lbl1.Content = "Pick Pipe To Be Moved";
                    elId2 = uiDoc.Selection.PickObject(ObjectType.Element, "Pick Pipe To Be Moved").ElementId;

                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    prompt.Close();
                    return Result.Succeeded;
                }

                if (doc.GetElement(elId1).Location is not LocationCurve lc1 || 
                    doc.GetElement(elId2).Location is not LocationCurve lc2)
                {
                    prevErr = true;
                    continue;
                }
                prevErr = false;

                if (lc1.Curve is not Line line1)
                    return Result.Failed;

                var lineDir = line1.Direction;
                XYZ vec;
                double pipeDist = Convert.ToDouble(view.Scale) * Main.Settings.PipeDist / 1152;

                if (Math.Abs(Math.Abs(lineDir.Z) - 1) < 0.001)
                {
                    var pt1 = line1.GetEndPoint(0);
                    var pt2 = lc2.Curve.GetEndPoint(0);
                    double xDiff = pt1.X - pt2.X;
                    double yDiff = pt1.Y - pt2.Y;
                    vec = Math.Abs(xDiff) > Math.Abs(yDiff) ? 
                        new XYZ(xDiff - Math.Sign(xDiff) * pipeDist, yDiff, 0) : 
                        new XYZ(xDiff, yDiff - Math.Sign(yDiff) * pipeDist, 0);
                }
                else
                {
                    XYZ dirVec = new(-lineDir.Y, lineDir.X, 0.0);
                    var intLine1 = Line.CreateUnbound(new XYZ(lc2.Curve.Evaluate(0.5, true).X, lc2.Curve.Evaluate(0.5, true).Y, 0), new XYZ(lineDir.X, lineDir.Y, 0.0));
                    var intLine2 = Line.CreateUnbound(new XYZ(line1.Evaluate(0.5, true).X, line1.Evaluate(0.5, true).Y, 0), dirVec);
                    intLine2.Intersect(intLine1, out var resArray);
                    var intPnt = resArray.get_Item(0).XYZPoint;

                    double curDist = intPnt.DistanceTo(new XYZ(line1.Evaluate(0.5, true).X, line1.Evaluate(0.5, true).Y, 0));
                    double moveDist = curDist - pipeDist;
                    var moveDir = new XYZ(line1.Evaluate(0.5, true).X - intPnt.X, line1.Evaluate(0.5, true).Y - intPnt.Y, 0).Normalize();
                    vec = moveDist * moveDir;
                }


                using var tx = new Transaction(doc, "Space Piping");
                if (tx.Start() != TransactionStatus.Started)
                    return Result.Failed;
                lc2.Move(vec);
                tx.Commit();
            }
        }
        catch
        {
            prompt.Close();
            throw;
        }
    }
}