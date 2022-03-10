using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using Proficient.Forms;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class MatchSchWidth : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            Follower prompt = new Follower(revit);

            IList<ElementId> ids = uidoc.Selection.GetElementIds() as IList<ElementId>;
            double width = 1.0;
            try
            {
                prompt.lbl1.Content = "Pick Anchor Schedule";
                Element anchor = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Anchor Schedule"));
                width = (doc.GetElement((anchor as ScheduleSheetInstance).ScheduleId) as ViewSchedule).GetTableData().Width;

                prompt.lbl1.Content = "Pick Schedule To Adjust";
                Element dependent = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Schedule To Adjust"));

                using (Transaction tx = new Transaction(doc, "Adjust Schedule Width"))
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        (doc.GetElement((dependent as ScheduleSheetInstance).ScheduleId) as ViewSchedule).GetTableData().Width = width;
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
