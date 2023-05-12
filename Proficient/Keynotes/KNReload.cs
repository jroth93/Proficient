using Autodesk.Revit.DB.ExternalService;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Proficient.Utilities;

namespace Proficient.Keynotes;

[Transaction(TransactionMode.Manual)]
internal class KnReload : IExternalCommand
{
    public static Guid DbId { get; set; }

    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var pn = Util.GetProjectNumber(revit);
        var doc = revit.Application.ActiveUIDocument.Document;

        var filePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath());
        var fileDir = 
            filePath.Substring(0, 7) == "BIM 360" || filePath.Substring(0, 8) == "Autodesk" ? 
                Util.GetProjectFolder(revit) : 
                Path.GetDirectoryName(filePath);

        var xlPath = $"{fileDir}\\{pn} Keynotes.xlsx";

        if (!File.Exists(xlPath))
            File.Copy(Names.File.KnTempFile, xlPath);

        var knList = new List<KeynoteEntry>();

        try
        {
            using var xlDoc = SpreadsheetDocument.Open(xlPath, false);
            var wbp = xlDoc.WorkbookPart;
            if (wbp?.SharedStringTablePart == null || wbp.Workbook.Sheets == null)
                return Result.Failed;
            var sst = wbp.SharedStringTablePart.SharedStringTable;

            foreach (var wsp in wbp.WorksheetParts)
            {
                var sheet = wbp.Workbook.Sheets
                    .Cast<Sheet>()
                    .First(sh => sh.Id != null && sh.Id.Value == wbp.GetIdOfPart(wsp));
                    
                knList.Add(new KeynoteEntry(sheet.Name?.Value, string.Empty));
                var data = wsp.Worksheet.Elements<SheetData>().First();
                foreach (var r in data.Elements<Row>())
                {
                    try
                    {
                        var key = sst.ElementAt(int.Parse(r.ElementAt(0).InnerText)).InnerText;
                        var note = sst.ElementAt(int.Parse(r.ElementAt(1).InnerText)).InnerText;
                        knList.Add(new KeynoteEntry(key, sheet.Name, note));
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
        catch (IOException e)
        {
            if (!e.Message.Contains("cannot access the file")) 
                return Result.Failed;

            var td = new TaskDialog("File Access Denied")
            {
                MainContent = @"Enable Excel Sharing",
                MainInstruction = @"File could not be opened due to incorrect Excel file sharing settings.",
                CommonButtons = TaskDialogCommonButtons.Close,
                DefaultButton = TaskDialogResult.Close,
            };
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "How to turn on Excel Legacy Sharing");
            const string url = @"https://support.microsoft.com/en-us/office/what-happened-to-shared-workbooks-150fc205-990a-4763-82f1-6c259303fe05";
            if (td.Show() == TaskDialogResult.CommandLink1)
                System.Diagnostics.Process.Start(url);
                
            return Result.Failed;
        }

        var externalResourceService = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService);

        if (externalResourceService.GetServer(DbId) is not ExternalResourceDBServer knSrv)
            return Result.Failed;

        knSrv.knList = knList;

        using var tx = new Transaction(doc, "Reload Keynotes");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;

        var p = ModelPathUtils.ConvertUserVisiblePathToModelPath("KNServer://Keynotes.txt");
        var s = ExternalResourceReference.CreateLocalResource(doc,
            ExternalResourceTypes.BuiltInExternalResourceTypes.KeynoteTable, p, PathType.Absolute);
        KeynoteTable.GetKeynoteTable(doc).LoadFrom(s, null);

        if (tx.Commit() == TransactionStatus.Committed) 
            Util.BalloonTip("Keynotes", "Keynotes Reloaded!", string.Empty);
            

        return Result.Succeeded;
    }
}