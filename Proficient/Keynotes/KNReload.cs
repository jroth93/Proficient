using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;


namespace Proficient
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
            string fileDir = filePath.Substring(0, 7) == "BIM 360" || filePath.Substring(0, 8) == "Autodesk" ? Util.GetProjectFolder(revit) : Path.GetDirectoryName(filePath);
            string xlPath = File.Exists($"{fileDir}\\{pn} Keynotes.xlsm") ? $"{fileDir}\\{pn} Keynotes.xlsm" : $"{fileDir}\\{pn} Keynotes.xlsx";
            bool oldFile = Path.GetExtension(xlPath) == ".xlsm";

            if (!File.Exists(xlPath))
            {
                File.Copy(Names.File.KnTempFile, xlPath);
            }

            ExternalService externalResourceService = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService);
            ExternalResourceDBServer knSrv = externalResourceService.GetServer(dbID) as ExternalResourceDBServer;

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
                        string key = sst.ElementAt(int.Parse(r.ElementAt(0).InnerText)).InnerText;
                        string note = sst.ElementAt(int.Parse(r.ElementAt(1).InnerText)).InnerText;

                        if (key != string.Empty && note != string.Empty)
                        {
                            knList.Add(oldFile ? MacroFilePatch(sheetName, key, note) : new KeynoteEntry(key, sheetName, note));
                        }
                    }
                }
            }





            //XL.Application app;
            //try
            //{
            //    app = Marshal.GetActiveObject("Excel.Application") as XL.Application;
                
            //}
            //catch (COMException)
            //{
            //    app = new XL.Application();
            //}

            
            //XL.Workbook book = app.Workbooks.Open(Filename: xlPath, ReadOnly: true);

            //foreach (XL.Worksheet sheet in book.Worksheets)
            //{
            //    knList.Add(new KeynoteEntry(sheet.Name, string.Empty, string.Empty));

            //    XL.Range range = sheet.UsedRange;
            //    var vals = range.Value;

            //    for (int row = 1; row <= vals.GetLength(0); row++)
            //    {
            //        var key = Convert.ToString(vals[row, 1]);
            //        var note = Convert.ToString(vals[row, 2]);
            //        if (key != string.Empty && note != string.Empty)
            //        {
            //            knList.Add(oldFile ? MacroFilePatch(sheet.Name, key, note) : new KeynoteEntry(key, sheet.Name, note));
            //        }
            //    }
            //}
            
            //book.Close(false);
            //app.Quit();

            knSrv.knList = knList;

            using (Transaction tx = new Transaction(doc, "Reload Keynotes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    ModelPath p = ModelPathUtils.ConvertUserVisiblePathToModelPath("KNServer://Keynotes.txt");
                    ExternalResourceReference s = ExternalResourceReference.CreateLocalResource(doc, ExternalResourceTypes.BuiltInExternalResourceTypes.KeynoteTable, p, PathType.Absolute);
                    KeynoteTable.GetKeynoteTable(doc).LoadFrom(s, null);
                }
                tx.Commit();
            }

            Util.BalloonTip("Keynotes", "Keynotes Reloaded!", string.Empty);

            return Result.Succeeded;
        }

        private static KeynoteEntry MacroFilePatch(string wsName, string key, string note)
        {
            if (wsName.Contains("DEMO") && Convert.ToInt32(key) <= 100)
            {
                if (Convert.ToInt32(key) <= 10)
                {
                    return new KeynoteEntry($"{wsName[0]}00{key}", wsName, note);
                }
                else
                {
                    return new KeynoteEntry($"{wsName[0]}0{key}", wsName, note);
                }
            }
            else
            {
                return new KeynoteEntry($"{wsName[0]}{key}", wsName, note);
            }
        }
    }
}
