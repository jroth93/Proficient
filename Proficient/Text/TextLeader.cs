using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class TextLeader : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ElementId viewid = uidoc.ActiveView.Id;
            View curview = doc.GetElement(viewid) as View;

            XYZ pl = uidoc.Selection.PickPoint();
            XYZ pt = uidoc.Selection.PickPoint();

            FilteredElementCollector coll = new FilteredElementCollector(doc);
            TextNoteType txttype = coll.WherePasses(new ElementClassFilter(typeof(TextNoteType))).Where(type => type.Name == Main.Settings.defFont).ElementAt(0) as TextNoteType;
            TextNoteOptions txtoptions = new TextNoteOptions(txttype.Id);

            using (Transaction tx = new Transaction(doc, "Remove Line Breaks"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    TextNote newtxt = TextNote.Create(doc, viewid, pt, 0.05, "TEXT", txtoptions);
                    if (pl.X <= pt.X)
                    {
                        Leader ldr = newtxt.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_L);
                        newtxt.HorizontalAlignment = HorizontalTextAlignment.Left;
                        ldr.End = pl;
                        ldr.Elbow = new XYZ(ldr.Anchor.X - curview.Scale / 96.0, ldr.Anchor.Y, ldr.Anchor.Z);
                    }
                    else if (pl.X > pt.X)
                    {
                        Leader ldr = newtxt.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_R);
                        newtxt.HorizontalAlignment = HorizontalTextAlignment.Right;
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