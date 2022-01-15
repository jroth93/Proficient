using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using Proficient.Forms;
using System;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class AlignSchedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            Follower prompt = new Follower(revit);

            double aHt = 1.0;
            XYZ aPt = new XYZ();
            try
            {
                prompt.lbl1.Content = "Pick Anchor Schedule";
                Element anchor = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Anchor Schedule"));
                BoundingBoxXYZ aBb = anchor.get_BoundingBox(view);
                aHt = aBb.Max.Y - aBb.Min.Y;
                aPt = (anchor as ScheduleSheetInstance).Point;
                string schName = doc.GetElement((anchor as ScheduleSheetInstance).ScheduleId).Name.ToLower();
                if (schName.Contains("plumbing fixture"))
                {
                    aPt = new XYZ(aPt.X - 219.0 / 256 / 12, aPt.Y, aPt.Z);
                }
                else if(schName.Contains("plumbing specialties"))
                {
                    aPt = new XYZ(aPt.X - 193.0 / 256 / 12, aPt.Y, aPt.Z);
                }
                prompt.lbl1.Content = "Pick Schedule To Adjust";
                Element dependent = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Schedule To Adjust"));

                BoundingBoxXYZ dBb = dependent.get_BoundingBox(view);
                double dHt = dBb.Max.Y - dBb.Min.Y;
                XYZ dPt = (dependent as ScheduleSheetInstance).Point;
                XYZ endPt = dPt.Y > aPt.Y ?
                    new XYZ(aPt.X, aPt.Y + dHt + 43.0 / 256 / 12, aPt.Z) :
                    new XYZ(aPt.X, aPt.Y - aHt - 43.0 / 256 / 12, aPt.Z);

                using (Transaction tx = new Transaction(doc, "Align Schedules"))
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        (dependent as ScheduleSheetInstance).Point = endPt;
                    }

                    tx.Commit();
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                prompt.Close();
                return Result.Succeeded;
            }
            catch
            {
                prompt.Close();
                return Result.Failed;
            }
            prompt.Close();
            return Result.Succeeded;
    
        }
    }
}
