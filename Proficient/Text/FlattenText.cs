using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class FlattenText : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ElementId viewid = uidoc.ActiveView.Id;
            View curview = doc.GetElement(viewid) as View;

            var selectedids = uidoc.Selection.GetElementIds() as IEnumerable<ElementId>;
            if (selectedids.Count() == 0) { return Result.Cancelled; }

            var els = selectedids.Select(id => doc.GetElement(id) as TextElement).Where(el => el != null);


            using (Transaction tx = new Transaction(doc, "Remove Line Breaks"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (TextElement el in els)
                    {
                        string txt = el.Text;
                        txt = txt.Replace("\r", " ").Replace("  ", " ");
                        el.Text = txt;
                    }

                    tx.Commit();
                }

            }

            return Autodesk.Revit.UI.Result.Succeeded;

        }
    }
}