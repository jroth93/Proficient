using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Proficient.Keynotes
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class KeynoteUtil : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            Document doc = revit.Application.ActiveUIDocument.Document;
            KeynoteUtilFrm kuf = new KeynoteUtilFrm();
            //get association between views and sheets
            Dictionary<ElementId, string> viewSheetDict = GetViewsOnSheets(doc);
            var fecPlacedKeynotes = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_KeynoteTags).OfClass(typeof(IndependentTag));
            KeyBasedTreeEntries kte = (KeynoteTable.GetKeynoteTable(doc)).GetKeyBasedTreeEntries();

            //get association between placed keynotes and their owner views
            Dictionary<string, List<ElementId>> knViewDict = new Dictionary<string, List<ElementId>>();
            foreach (IndependentTag it in fecPlacedKeynotes)
            {
                string knNum = it.get_Parameter(BuiltInParameter.KEYNOTE_NUMBER).AsString();

                if (!knViewDict.ContainsKey(knNum))
                {
                    knViewDict.Add(knNum, new List<ElementId>());
                }

                ElementId ownerId = it.OwnerViewId;
                var dvIds = (doc.GetElement(it.OwnerViewId) as View).GetDependentViewIds();
                if (dvIds.Count > 0)
                {
                    foreach (ElementId viewId in dvIds)
                    {
                        View curView = doc.GetElement(viewId) as View;
                        if (IsInView(curView.CropBox, it.TagHeadPosition)) knViewDict[knNum].Add(viewId);
                    }
                }
                else
                {
                    knViewDict[knNum].Add(ownerId);
                }
            }

            foreach (KeynoteEntry ke in kte)
            {
                if (ke.KeynoteText != string.Empty)
                {
                    if (knViewDict.ContainsKey(ke.Key))
                    {
                        List<string> sheets = new List<string>();
                        foreach (ElementId viewid in knViewDict[ke.Key])
                        {
                            if (viewSheetDict.ContainsKey(viewid)) sheets.Add(viewSheetDict[viewid]);
                        }

                        if (sheets.Count > 0)
                        {
                            string sheetsOutput = sheets.Count > 1 ? string.Join(", ", sheets.Distinct().OrderBy(x => x).ToArray()) : sheets[0];
                            string[] row = { ke.Key, ke.KeynoteText, sheetsOutput };
                            kuf.dgv.Rows.Add(row);
                        }
                        else
                        {
                            string[] row = { ke.Key, ke.KeynoteText, "None" };
                            kuf.dgv.Rows.Add(row);
                        }
                    }
                    else
                    {
                        string[] row = { ke.Key, ke.KeynoteText, "None" };
                        kuf.dgv.Rows.Add(row);
                    }
                }
            }
            kuf.Show();
            return Result.Succeeded;
        }

        private Dictionary<ElementId, string> GetViewsOnSheets(Document doc)
        {
            Dictionary<ElementId, string> viewSheetPairs = new Dictionary<ElementId, string>();
            FilteredElementCollector fecViewSheet = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet));
            foreach (ViewSheet vs in fecViewSheet)
            {
                foreach (ElementId vID in vs.GetAllPlacedViews())
                {
                    if (!viewSheetPairs.ContainsKey(vID)) viewSheetPairs.Add(vID, vs.SheetNumber);
                }
            }
            return viewSheetPairs;
        }

        private bool IsInView(BoundingBoxXYZ bb, XYZ elementPosition)
        {
            return elementPosition.X <= bb.Max.X
                && elementPosition.X >= bb.Min.X
                && elementPosition.Y <= bb.Max.Y
                && elementPosition.Y >= bb.Min.Y;
        }
    }
}
