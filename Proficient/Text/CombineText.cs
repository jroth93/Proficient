using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CombineText : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ElementId viewid = uidoc.ActiveView.Id;
            View curview = doc.GetElement(viewid) as View;

            var selectedids = uidoc.Selection.GetElementIds() as IEnumerable<ElementId>;
            if (selectedids.Count() == 0) { return Result.Cancelled; }

            var sortedelements = selectedids
                .Select(id => doc.GetElement(id) as TextElement)
                .Where(el => el != null)
                .OrderByDescending(el => el.Coord.Y)
                .ThenBy(el => el.Coord.X);

            if (sortedelements.Count() == 0) { return Result.Cancelled; }

            string combinedtext = "";
            TextElement previous = null;

            foreach (TextElement el in sortedelements)
            {

                if (previous != null)
                {
                    if (Math.Abs(el.Coord.Y - previous.Coord.Y) > el.Height * curview.Scale)
                    {
                        combinedtext += "\n";
                    }
                }
                combinedtext += el.Text + " ";
                previous = el;
            }

            combinedtext = combinedtext.Replace("\r", "");
            double width = (sortedelements.ElementAt(sortedelements.Count() - 1).Coord.X - sortedelements.ElementAt(0).Coord.X) / curview.Scale + sortedelements.Select(el => el.Width).OrderByDescending(el => el).ElementAt(0);
            ElementId txttype = (sortedelements.ElementAt(0) as TextNote).TextNoteType.Id;

            using (Transaction tx = new Transaction(doc, "Combine"))
            {
                try
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        TextNote.Create(doc, viewid, sortedelements.ElementAt(0).Coord, width, combinedtext, txttype);
                        foreach (Element el in sortedelements) { doc.Delete(el.Id); };
                    }
                }
                catch (NullReferenceException)
                {
                }
                tx.Commit();
            }

            return Autodesk.Revit.UI.Result.Succeeded;

        }
    }
}