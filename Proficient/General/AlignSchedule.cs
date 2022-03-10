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
    class AlignSchedule
    {
        public static Result Align(ExternalCommandData revit, bool left)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;
            
            Follower prompt = new Follower(revit);

            try
            {
                prompt.lbl1.Content = "Pick Anchor Schedule";
                Element anchor = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Anchor Schedule"));
                BoundingBoxXYZ aBb = anchor.get_BoundingBox(view);
                double aHt = aBb.Max.Y - aBb.Min.Y;
                XYZ aPt = (anchor as ScheduleSheetInstance).Point;
                string aName = doc.GetElement((anchor as ScheduleSheetInstance).ScheduleId).Name.ToLower();
                
                prompt.lbl1.Content = "Pick Schedule To Adjust";
                Element dependent = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Schedule To Adjust"));
                string dName = doc.GetElement((dependent as ScheduleSheetInstance).ScheduleId).Name.ToLower();
                BoundingBoxXYZ dBb = dependent.get_BoundingBox(view);
                double dHt = dBb.Max.Y - dBb.Min.Y;
                XYZ dPt = (dependent as ScheduleSheetInstance).Point;

                double x, y;
                int adjDir = aName.Contains("plumbing") ? 1 : -1;

                if (aName.Contains("plumbing") && dName.Contains("plumbing"))
                {
                    int adjDir2 = aName.Contains("plumbing fixture") ? 1 : -1;

                    x = aPt.X - adjDir2 * 13.0 / 128 / 12;
                    y = aPt.Y + adjDir2 * 23.0 / 64 / 12;
                }
                else if (aName.Contains("plumbing fixture") || dName.Contains("plumbing fixture"))
                {
                    x = aPt.X - adjDir * 219.0 / 256 / 12;
                    y = aPt.Y + adjDir * 23.0 / 64 / 12;
                }
                else if(aName.Contains("plumbing specialties") || dName.Contains("plumbing specialties"))
                {
                    x = aPt.X - adjDir * 193.0 / 256 / 12;
                    y = aPt.Y;
                }
                else
                {
                    x = aPt.X;
                    y = aPt.Y;
                }

                if (!left)
                {
                    double aWidth = (doc.GetElement((anchor as ScheduleSheetInstance).ScheduleId) as ViewSchedule).GetTableData().Width;
                    double dWidth = (doc.GetElement((dependent as ScheduleSheetInstance).ScheduleId) as ViewSchedule).GetTableData().Width;
                    x = x + aWidth - dWidth;
                }
                y = dPt.Y > aPt.Y ? y + dHt + 43.0 / 256 / 12 : y - aHt - 43.0 / 256 / 12;

                XYZ endPt = new XYZ(x, y, aPt.Z);
                
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
