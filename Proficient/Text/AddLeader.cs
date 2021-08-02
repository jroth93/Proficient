using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class AddLeader : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ElementId viewid = uidoc.ActiveView.Id;
            View curview = doc.GetElement(viewid) as View;
            IList<ElementId> ids = uidoc.Selection.GetElementIds() as IList<ElementId>;
            if (ids.Count() == 0)
                ids.Add(doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick Element")).Id);
            ElementId txtid = ids.Where(id => (doc.GetElement(id) as TextNote) != null).FirstOrDefault();

            //"MEI Callout"


            TextNote txt = (doc.GetElement(txtid) as TextNote);

            if (txt == null)
            {
                TaskDialog td = new TaskDialog("Invalid Selection");
                td.MainContent = "Element Picked Was Not Text.";
                td.Show();
                return Result.Failed;
            }
            XYZ pl = uidoc.Selection.PickPoint();

            using (Transaction tx = new Transaction(doc, "Remove Line Breaks"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    if (pl.X <= txt.Coord.X)
                    {
                        Leader ldr = txt.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_L);
                        txt.HorizontalAlignment = HorizontalTextAlignment.Left;
                        ldr.End = pl;
                        ldr.Elbow = new XYZ(ldr.Anchor.X - curview.Scale / 96.0, ldr.Anchor.Y, ldr.Anchor.Z);
                    }
                    else if (pl.X > txt.Coord.X)
                    {
                        Leader ldr = txt.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_R);
                        txt.HorizontalAlignment = HorizontalTextAlignment.Right;
                        ldr.End = pl;
                        ldr.Elbow = new XYZ(ldr.Anchor.X + curview.Scale / 96.0, ldr.Anchor.Y, ldr.Anchor.Z);
                    }
                    tx.Commit();
                }

            }

            return Autodesk.Revit.UI.Result.Succeeded;

        }
    }
}