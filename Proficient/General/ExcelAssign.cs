using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XL = Microsoft.Office.Interop.Excel;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ExcelAssign : IExternalCommand
    {
        private static Document doc;
        private static XL.Application xl;
        private static XL.Workbook wb;
        private static XL.Worksheet ws;
        private static bool xlReadOnly = false;
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            doc = revit.Application.ActiveUIDocument.Document;
            ExcelAssignFrm eafrm = new ExcelAssignFrm();
            eafrm.ShowDialog();

            if (xlReadOnly) File.Delete(wb.FullName);
            else wb.Close(false);

            return Result.Succeeded;
        }

        public static String[] GetExcelColumns(int wsIndex, int hdrRow)
        {
            List<string> cols = new List<string>();
            ws = wb.Worksheets.Item[wsIndex];

            int totCols = ws.UsedRange.Columns.Count;
            string cellVal;

            if (totCols > 0)
            {
                for (int i = 1; i <= totCols; i++)
                {
                    cellVal = ws.Cells[hdrRow, i].Value == null ? "" : Convert.ToString(ws.Cells[hdrRow, i].Value);
                    cols.Add(cellVal);
                }
            }

            return cols.ToArray();
        }

        public static String[] OpenExcel(string xlPath)
        {
            xl = new XL.Application();

            try
            {
                FileStream stream = File.Open(xlPath, FileMode.Open, FileAccess.Read);
                stream.Close();
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("being used by another process"))
                {
                    string newPath =
                        Path.GetExtension(xlPath) == ".xlsx" ?
                        Path.GetDirectoryName(xlPath) + @"\" + Path.GetFileNameWithoutExtension(xlPath) + "-temp.xlsx" :
                        Path.GetDirectoryName(xlPath) + @"\" + Path.GetFileNameWithoutExtension(xlPath) + "-temp.xlsm";
                    File.Copy(xlPath, newPath);
                    xlPath = newPath;
                    xlReadOnly = true;
                }
            }

            wb = xl.Workbooks.Open(Filename: xlPath, ReadOnly: true);
            var ws = wb.Worksheets;
            List<string> wslist = new List<string>();

            for (int i = 1; i <= ws.Count; i++)
                wslist.Add((ws.Item[i] as XL.Worksheet).Name);

            return wslist.ToArray();
        }
#if (R21 || R22)
        private static dynamic GetCellVal(int row, int col, string parType, ForgeTypeId dispUnit)
        {
            switch (parType)
            {
                case "String":
                    return Convert.ToString(ws.Cells[row, col].Value);
                case "Integer":
                    try
                    {
                        int val = Convert.ToInt32(ws.Cells[row, col].Value);
                        val = (int)UnitUtils.ConvertToInternalUnits(val, dispUnit);
                        return val;
                    }
                    catch
                    {
                        return null;
                    }
                case "Double":
                    try
                    {
                        double val = Convert.ToDouble(ws.Cells[row, col].Value);
                        val = UnitUtils.ConvertToInternalUnits(val, dispUnit);
                        return val;
                    }
                    catch
                    {
                        return null;
                    }

                case "ElementId":
                    try
                    {
                        return new ElementId(Convert.ToInt32(ws.Cells[row, col].Value));
                    }
                    catch
                    {
                        return null;
                    }
            }
            return null;
        }
#else
        private static dynamic GetCellVal(int row, int col, string parType, DisplayUnitType dispUnit)
        {
            switch (parType)
            {
                case "String":
                    return Convert.ToString(ws.Cells[row, col].Value);
                case "Integer":
                    try
                    {
                        int val = Convert.ToInt32(ws.Cells[row, col].Value);
                        val = (int)UnitUtils.ConvertToInternalUnits(val, dispUnit);
                        return val;
                    }
                    catch
                    {
                        return null;
                    }
                case "Double":
                    try
                    {
                        double val = Convert.ToDouble(ws.Cells[row, col].Value);
                        val = UnitUtils.ConvertToInternalUnits(val, dispUnit);
                        return val;
                    }
                    catch
                    {
                        return null;
                    }

                case "ElementId":
                    try
                    {
                        return new ElementId(Convert.ToInt32(ws.Cells[row, col].Value));
                    }
                    catch
                    {
                        return null;
                    }
            }
            return null;
        }
#endif
        private static dynamic GetCellVal(int row, int col, string parType)
        {
            switch (parType)
            {
                case "String":
                    return Convert.ToString(ws.Cells[row, col].Value);
                case "Integer":
                    try
                    {
                        return Convert.ToInt32(ws.Cells[row, col].Value);
                    }
                    catch
                    {
                        return null;
                    }
                case "Double":
                    try
                    {
                        return Convert.ToDouble(ws.Cells[row, col].Value);
                    }
                    catch
                    {
                        return null;
                    }

                case "ElementId":
                    try
                    {
                        return new ElementId(Convert.ToInt32(ws.Cells[row, col].Value));
                    }
                    catch
                    {
                        return null;
                    }
            }
            return null;
        }

        public static void WriteErrorFile(string errorLog)
        {
            string logFilePath = wb.Path + "\\ExcelToRevitErrorLog.txt";

            using (StreamWriter sw = File.CreateText(logFilePath))
            {
                sw.Write(errorLog);
            }
        }

        public static string[] GetCategories()
        {
            List<string> cList = new List<string>();

            foreach (Category c in doc.Settings.Categories)
            {
                int catCount = new FilteredElementCollector(doc).OfCategoryId(c.Id).Count();

                if (c.AllowsBoundParameters && catCount > 0)
                {
                    cList.Add(c.Name);
                }
            }
            cList.Sort();

            return cList.ToArray();
        }

        public static String[] GetFamiliesOfCategory(string catName)
        {
            string[] fn = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .Where(q => (q as Family).FamilyCategory.Name == catName)
            .Select(q => q.Name)
            .ToArray();

            return fn;
        }

        public static string[] GetFamilyParameters(string familyName)
        {
            List<string> pars = new List<string>();

            FamilySymbol fs = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Where(q => (q as FamilySymbol).Family.Name == familyName)
            .FirstOrDefault() as FamilySymbol;
            foreach (Parameter par in fs.Parameters)
            {
                if (!par.IsReadOnly)
                {
                    pars.Add(par.Definition.Name + " (type)");
                }
            }

            FamilyInstance fi = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Where(q => (q as FamilyInstance).Symbol.Family.Name == Convert.ToString(familyName))
                .FirstOrDefault() as FamilyInstance;
            if (fi != null)
            {
                foreach (Parameter par in fi.Parameters)
                {
                    if (!par.IsReadOnly)
                    {
                        pars.Add(par.Definition.Name + " (inst)");
                    }
                }
            }

            return pars.ToArray();
        }

        public static string AssignParameterValuesType(string familyName, string parName, int keyCol, int startRow, int parCol)
        {
            string errorLog = String.Empty;

            int totRows = ws.UsedRange.Rows.Count;
            string keyCellVal = null;

            var fslist = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Where(q => (q as FamilySymbol).Family.Name == familyName)
                .OrderBy(q => Convert.ToString(q.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK)));

            string typeMark = null;
            string parType = Convert.ToString(fslist.First().LookupParameter(parName).StorageType);

            foreach (FamilySymbol fs in fslist)
            {
                typeMark = fs.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString();
                for (int r = startRow; r <= totRows; r++)
                {
                    keyCellVal = Convert.ToString(ws.Cells[r, keyCol].Value);
                    if (typeMark == keyCellVal)
                    {
                        bool hasUnits;

                        

                        try
                        {
#if (R21 || R22)
                            var dut = fs.LookupParameter(parName).GetUnitTypeId();
#else
                            DisplayUnitType dut = fs.LookupParameter(parName).DisplayUnitType;
#endif
                            hasUnits = true;
                        }
                        catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                        {
                            hasUnits = false;
                        }


                                using (Transaction tx = new Transaction(doc, "Assign Type Parameter"))
                        {
                            if (tx.Start() == TransactionStatus.Started)
                            {
#if (R21 || R22)
                                var newVal = hasUnits ? GetCellVal(r, parCol, parType, fs.LookupParameter(parName).GetUnitTypeId()) : GetCellVal(r, parCol, parType);
#else
                                var newVal = hasUnits ? GetCellVal(r, parCol, parType, fs.LookupParameter(parName).DisplayUnitType) : GetCellVal(r, parCol, parType);
#endif
                                if (newVal != null)
                                {
                                    fs.LookupParameter(parName).Set(newVal);
                                    tx.Commit();
                                }
                                else
                                {
                                    errorLog += $"Incorrect data type in Excel File for element '{typeMark}' parameter '{parName}.' Parameter will not be assigned.\n";
                                }
                            }
                        }
                    }
                }
            }


            return errorLog;
        }

        public static string AssignParameterValuesInst(string familyName, string parName, int keyCol, int startRow, int parCol)
        {
            string errorLog = String.Empty;

            int totRows = ws.UsedRange.Rows.Count;

            List<Element> filist = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Where(q => (q as FamilyInstance).Symbol.Family.Name == familyName).ToList();

            string parType = Convert.ToString(filist.First().LookupParameter(parName).StorageType);

            string[] rvtMarkList = filist.Select(x => x.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString()).ToArray();
            Dictionary<string, int> markRowIndex = new Dictionary<string, int>();

            for (int r = startRow; r <= totRows; r++)
            {
                string curXLMark = Convert.ToString(ws.Cells[r, keyCol].Value);
                if (Array.IndexOf(rvtMarkList, curXLMark) > -1)
                    markRowIndex.Add(curXLMark, r);
            }


            foreach (FamilyInstance fi in filist)
            {
                string mark = fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
                if (markRowIndex.ContainsKey(mark))
                {
                    Parameter par = fi.LookupParameter(parName);
                    bool hasUnits;
                    try
                    {
#if (R21 || R22)
                        var dut = fi.LookupParameter(parName).GetUnitTypeId();
#else
                        DisplayUnitType dut = fi.LookupParameter(parName).DisplayUnitType;
#endif
                        hasUnits = true;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        hasUnits = false;
                    }

#if (R21 || R22)
                    var newVal = hasUnits ? GetCellVal(markRowIndex[mark], parCol, parType, par.GetUnitTypeId()) : GetCellVal(markRowIndex[mark], parCol, parType);
#else
                    var newVal = hasUnits ? GetCellVal(markRowIndex[mark], parCol, parType, par.DisplayUnitType) : GetCellVal(markRowIndex[mark], parCol, parType);
#endif
                    if (newVal == null)
                    {
                        errorLog += $"Incorrect data type in Excel File for element '{mark}' parameter '{parName}.' Expecting type of {par.StorageType}. Parameter will not be assigned.\n";
                    }
                    else if (ParToString(par, parType) != Convert.ToString(newVal))
                    {
                        using (Transaction tx = new Transaction(doc, "Assign Instance Parameter"))
                        {
                            if (tx.Start() == TransactionStatus.Started)
                            {
                                par.Set(newVal);
                                tx.Commit();
                            }
                        }
                    }
                }


            }


            return errorLog;
        }

        private static string ParToString(Parameter par, string parType)
        {
            switch (parType)
            {
                case "String":
                    return par.AsString();
                case "Integer":
                    return Convert.ToString(par.AsInteger());
                case "Double":
                    return Convert.ToString(par.AsDouble());
                case "ElementId":
                    return Convert.ToString(par.AsElementId());
            }
            return null;
        }
    }
}
