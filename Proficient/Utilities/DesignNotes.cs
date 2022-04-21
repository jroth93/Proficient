using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Proficient.Utilities
{
    internal class DesignNotes
    {
        private static Dictionary<View, ICollection<ElementId>> DesignNoteViews = new Dictionary<View, ICollection<ElementId>>();

        public static void Hide(Document doc, List<ElementId> printViews)
        {
            var textEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TextNotes)
                .Where(x => x.Name.ToLower().Contains("design"));

            var lineEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Lines)
                .Where(x => (x as CurveElement).LineStyle.Name.ToLower().Contains("design"));

            var dimEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Dimensions)
                .Where(x => x.Name.ToLower().Contains("design"));

            List<Element> designEl = new List<Element>();

            designEl.AddRange(textEl);
            designEl.AddRange(lineEl);
            designEl.AddRange(dimEl);

            List<ElementId> printViewsSub = new List<ElementId>();
            printViewsSub.AddRange(printViews);

            foreach (ElementId id in printViews)
            {
                if (doc.GetElement(id) is ViewSheet vs)
                {
                    foreach (ElementId vid in vs.GetAllPlacedViews())
                    {
                        printViewsSub.Add(vid);
                        ElementId pvid = (doc.GetElement(vid) as View).GetPrimaryViewId();
                        if (pvid == ElementId.InvalidElementId) printViewsSub.Add(pvid);
                    }
                }
                else
                {
                    ElementId pvid = (doc.GetElement(id) as View).GetPrimaryViewId();
                    if (pvid != ElementId.InvalidElementId) printViewsSub.Add(pvid);
                }
            }

            var noteViews = designEl
                .Select(x => x.OwnerViewId)
                .Distinct()
                .ToList();
            var noteDependentViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Where(x => noteViews.Contains((x as View).GetPrimaryViewId()))
                .Select(x => x.Id);
            noteViews.AddRange(noteDependentViews);
            var views = printViewsSub.Intersect(noteViews);

            DesignNoteViews.Clear();

            using (Transaction tx = new Transaction(doc, "Hide Design Notes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (ElementId viewId in views)
                    {
                        View curView = doc.GetElement(viewId) as View;
                        ICollection<ElementId> viewDesignEl = designEl
                            .Where(x => x.OwnerViewId == viewId || x.OwnerViewId == curView.GetPrimaryViewId())
                            .Select(x => x.Id)
                            .ToList();
                        DesignNoteViews.Add(curView, viewDesignEl);
                        curView.HideElements(viewDesignEl);
                    }
                }
                tx.Commit();
            }
        }
        public static void Unhide(Document doc)
        {
            using (Transaction tx = new Transaction(doc, "Unhide Design Notes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (View curView in DesignNoteViews.Keys)
                    {
                        curView.UnhideElements(DesignNoteViews[curView]);
                    }
                }
                tx.Commit();
            }
        }


    }

}
