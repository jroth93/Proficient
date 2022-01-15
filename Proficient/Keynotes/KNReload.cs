using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace Proficient.Keynotes
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class KNReload : IExternalCommand
    {
        public static Guid dbID;

        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            string pn = Util.GetProjectNumber(revit);
            Document doc = revit.Application.ActiveUIDocument.Document;

            string filePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath());
            string fileDir = 
                filePath.Substring(0, 7) == "BIM 360" || filePath.Substring(0, 8) == "Autodesk" ? 
                Util.GetProjectFolder(revit) : 
                Path.GetDirectoryName(filePath);

            string xlPath = $"{fileDir}\\{pn} Keynotes.xlsx";

            if (!File.Exists(xlPath))
            {
                File.Copy(Names.File.KnTempFile, xlPath);
            }

            List<KeynoteEntry> knList = new List<KeynoteEntry>();

            using (SpreadsheetDocument xlDoc = SpreadsheetDocument.Open(xlPath, false))
            {
                WorkbookPart wbp = xlDoc.WorkbookPart;
                WorksheetPart worksheetPart = wbp.WorksheetParts.First();
                SharedStringTable sst = wbp.SharedStringTablePart.SharedStringTable;

                wbp.Workbook.Sheets.ToList().ForEach(ws => Console.WriteLine((ws as Sheet).Name));

                foreach (WorksheetPart wsp in wbp.WorksheetParts)
                {
                    string sheetName = (wbp.Workbook.Sheets.Where(sh => (sh as Sheet).Id.Value == wbp.GetIdOfPart(wsp)).First() as Sheet).Name;
                    knList.Add(new KeynoteEntry(sheetName, string.Empty));
                    SheetData data = wsp.Worksheet.Elements<SheetData>().First();
                    foreach (Row r in data.Elements<Row>())
                    {

                        try
                        {
                            string key = sst.ElementAt(int.Parse(r.ElementAt(0).InnerText)).InnerText;
                            string note = sst.ElementAt(int.Parse(r.ElementAt(1).InnerText)).InnerText;
                            knList.Add(new KeynoteEntry(key, sheetName, note));
                        }
                        catch { }
                    }
                }
            }

            ExternalService externalResourceService = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService);
            ExternalResourceDBServer knSrv = externalResourceService.GetServer(dbID) as ExternalResourceDBServer;

            knSrv.knList = knList;

            using (Transaction tx = new Transaction(doc, "Reload Keynotes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    ModelPath p = ModelPathUtils.ConvertUserVisiblePathToModelPath("KNServer://Keynotes.txt");
                    ExternalResourceReference s = ExternalResourceReference.CreateLocalResource(doc, ExternalResourceTypes.BuiltInExternalResourceTypes.KeynoteTable, p, PathType.Absolute);
                    KeynoteTable.GetKeynoteTable(doc).LoadFrom(s, null);
                }
                if (tx.Commit() == TransactionStatus.Committed) {
                    Util.BalloonTip("Keynotes", "Keynotes Reloaded!", string.Empty);
                };
            }

            return Result.Succeeded;
        }
    }
}
