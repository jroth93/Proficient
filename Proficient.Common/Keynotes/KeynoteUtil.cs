using Proficient.Forms;

namespace Proficient.Keynotes;

[Transaction(TransactionMode.Manual)]
internal class KeynoteUtil : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var doc = revit.Application.ActiveUIDocument.Document;
        var kuf = new KeynoteUtilFrm();
        //get association between views and sheets
        var viewSheetDict = GetViewsOnSheets(doc);
        var fecPlacedKeynotes = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_KeynoteTags)
            .OfClass(typeof(IndependentTag))
            .Cast<IndependentTag>();
        var kte = KeynoteTable
            .GetKeynoteTable(doc)
            .GetKeyBasedTreeEntries()
            .Cast<KeynoteEntry>();

        //get association between placed keynotes and their owner views
        var knViewDict = new Dictionary<string, List<ElementId>>();
        foreach (var it in fecPlacedKeynotes)
        {
            var knNum = it.get_Parameter(BuiltInParameter.KEYNOTE_NUMBER).AsString();

            if (!knViewDict.ContainsKey(knNum))
                knViewDict.Add(knNum, []);

            var ownerId = it.OwnerViewId;
            if (doc.GetElement(ownerId) is not View v)
                continue;
            var dvIds = v.GetDependentViewIds();
            if (dvIds.Count > 0)
            {
                foreach (var viewId in dvIds)
                {
                    if(doc.GetElement(viewId) is View curView && IsInView(curView, it.TagHeadPosition))
                        knViewDict[knNum].Add(viewId);
                }
            }
            else
            {
                knViewDict[knNum].Add(ownerId);
            }
        }

        foreach (var ke in kte)
        {
            if (ke.KeynoteText == string.Empty) 
                continue;

            if (knViewDict.TryGetValue(ke.Key, out List<ElementId>? value))
            {
                var sheets = value.Where(viewSheetDict.ContainsKey)
                    .Select(id => viewSheetDict[id])
                    .ToList();

                if (sheets.Count > 0)
                {
                    var sheetsOutput = sheets.Count > 1 ? 
                        string.Join(", ", sheets.Distinct().OrderBy(x => x).ToArray()) : 
                        sheets[0];
                    object[] row = [ ke.Key, ke.KeynoteText, sheetsOutput ];
                    kuf.dgv.Rows.Add(row);
                }
                else
                {
                    object[] row = [ ke.Key, ke.KeynoteText, "None" ];
                    kuf.dgv.Rows.Add(row);
                }
            }
            else
            {
                object[] row = [ ke.Key, ke.KeynoteText, "None" ];
                kuf.dgv.Rows.Add(row);
            }
        }
        kuf.Show();
        return Result.Succeeded;
    }

    private static Dictionary<ElementId, string> GetViewsOnSheets(Document doc)
    {
        var viewSheetPairs = new Dictionary<ElementId, string>();
        var fecViewSheet = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet))
            .Cast<ViewSheet>();


        foreach (var vs in fecViewSheet)
        foreach (var vId in vs.GetAllPlacedViews())
            if (!viewSheetPairs.ContainsKey(vId)) 
                viewSheetPairs.Add(vId, vs.SheetNumber);
            
        return viewSheetPairs;
    }

    private static bool IsInView(View view, XYZ elementPosition)
    {
        var bb = view.CropBox;
        return elementPosition.X <= bb.Max.X
               && elementPosition.X >= bb.Min.X
               && elementPosition.Y <= bb.Max.Y
               && elementPosition.Y >= bb.Min.Y;
    }
}