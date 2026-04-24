using Proficient.Forms;
using System.Globalization;
using XL = Microsoft.Office.Interop.Excel;

namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class ExcelAssign : IExternalCommand
{
    private static Document? _doc;
    private static readonly XL.Application _xl = new();
    private static XL.Workbook _wb = new();
    private static XL.Worksheet _ws = new();
    private static bool _xlReadOnly = false;
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        _doc = revit.Application.ActiveUIDocument.Document;
        var eaFrm = new ExcelAssignFrm();
        eaFrm.ShowDialog();

        if (_xlReadOnly) 
        {
            File.Delete(_wb.FullName);
        }
        else 
        {
            _wb.Close(false); 
        }


        return Result.Succeeded;
    }

    public static string[] GetExcelColumns(int wsIndex, int hdrRow)
    {
        var cols = new List<string>();
        _ws = (XL.Worksheet)_wb.Worksheets.Item[wsIndex];

        var totCols = _ws.UsedRange.Columns.Count;

        if (totCols == 0) 
            return [.. cols];

        for (var i = 1; i <= totCols; i++)
        {
            var cellValObj = ((XL.Range)_ws.Cells[hdrRow, i])?.Value;

            if (Convert.ToString(cellValObj) is string cellVal)
            {
                cols.Add(cellVal);
            }
            else
            {
                cols.Add(string.Empty);
            }
        }

        return [.. cols];
    }

    public static string[] OpenExcel(string xlPath)
    {
        try
        {
            var stream = File.Open(xlPath, FileMode.Open, FileAccess.Read);
            stream.Close();
        }
        catch (IOException ex)
        {
            if (ex.Message.Contains("being used by another process"))
            {
                var newPath =
                    Path.GetExtension(xlPath) == ".xlsx" ?
                        Path.GetDirectoryName(xlPath) + @"\" + Path.GetFileNameWithoutExtension(xlPath) + "-temp.xlsx" :
                        Path.GetDirectoryName(xlPath) + @"\" + Path.GetFileNameWithoutExtension(xlPath) + "-temp.xlsm";
                File.Copy(xlPath, newPath);
                xlPath = newPath;
                _xlReadOnly = true;
            }
        }

        _wb = _xl.Workbooks.Open(Filename: xlPath, ReadOnly: true);
        string[] ws = [.. _wb.Worksheets.Cast<XL.Worksheet>().Select(ws => ws.Name)];

        return ws;
    }
#if PRE21
        private static dynamic GetCellVal(int row, int col, string parType, DisplayUnitType dispUnit)
        {
            try
            {
                switch (parType)
                {
                    case "String":
                        return Convert.ToString(_ws.Cells[row, col].Value);
                    case "Integer":
                        int iVal = Convert.ToInt32(_ws.Cells[row, col].Value);
                        iVal = (int)UnitUtils.ConvertToInternalUnits(iVal, dispUnit);
                        return iVal;
                    case "Double":
                        double dVal = Convert.ToDouble(_ws.Cells[row, col].Value);
                        dVal = UnitUtils.ConvertToInternalUnits(dVal, dispUnit);
                        return dVal;
                    case "ElementId":
                        return new ElementId(Convert.ToInt32(_ws.Cells[row, col].Value));
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }
#else
        private static dynamic? GetCellVal(int row, int col, string parType, ForgeTypeId dispUnit)
        {
            try
            {
                var val = ((XL.Range)_ws.Cells[row, col]).Value;

                return parType switch
                {
                    "String" => Convert.ToString(val),
                    "Integer" => (int)UnitUtils.ConvertToInternalUnits(Convert.ToInt32(val), dispUnit),
                    "Double" => UnitUtils.ConvertToInternalUnits(Convert.ToDouble(val), dispUnit),
#if PRE24
                    "ElementId" => new ElementId(Convert.ToInt32(val)),
#else
                    "ElementId" => new ElementId(Convert.ToInt64(val)),
#endif
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }
#endif
        private static dynamic? GetCellVal(int row, int col, string parType)
        {
            try
            {
                var val = ((XL.Range)_ws.Cells[row, col]).Value;
                
                return parType switch
                {
                    "String" => Convert.ToString(val),
                    "Integer" => Convert.ToInt32(val),
                    "Double" => Convert.ToDouble(val),
    #if PRE24
                    "ElementId" => new ElementId(Convert.ToInt32(val)),
    #else
                    "ElementId" => new ElementId(Convert.ToInt64(val)),
    #endif
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

    public static void WriteErrorFile(string errorLog)
    {
        if(_wb.Path == string.Empty) return;

        var logFilePath = _wb.Path + "\\ExcelToRevitErrorLog.txt";
        File.CreateText(logFilePath).Write(errorLog);
    }

    public static string[] GetCategories()
    {
        if (_doc is null) return [];

        var cList = _doc.Settings.Categories
            .Cast<Category>()
            .Where(c => c.AllowsBoundParameters)
            .Where(c => new FilteredElementCollector(_doc).OfCategoryId(c.Id).ToList().Count > 0)
            .Select(c => c.Name)
            .ToList();

        cList.Sort();


        return [.. cList];
    }

    public static string[] GetFamiliesOfCategory(string catName)
    {
        var fn = new FilteredElementCollector(_doc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .Where(q => q.FamilyCategory.Name == catName)
            .Select(q => q.Name)
            .ToArray();

        return fn;
    }

    public static string[] GetFamilyParameters(string familyName)
    {
        var pars = new List<string>();

        var fsPars = new FilteredElementCollector(_doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(q => q.Family.Name == familyName)
            ?.Parameters
            .Cast<Parameter>()
            .Where(p => !p.IsReadOnly)
            .Select(p => p.Definition.Name + " (type)")
            .ToList();

        if(fsPars != null)
            pars.AddRange(fsPars);
                
        var fiPars = new FilteredElementCollector(_doc)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .FirstOrDefault(q => q.Symbol.Family.Name == familyName)
            ?.Parameters
            .Cast<Parameter>()
            .Where(p => !p.IsReadOnly)
            .Select(p => p.Definition.Name + " (inst)")
            .ToList();

        if(fiPars != null)
            pars.AddRange(fiPars);

        return [.. pars];
    }

    public static string AssignTypeParameters(string familyName, string parName, int keyCol, int startRow, int parCol)
    {

        var errorLog = string.Empty;
        var totRows = _ws.UsedRange.Rows.Count;

        var fsList = new FilteredElementCollector(_doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .Where(fs => fs.Family.Name == familyName)
            .ToList();

        var parType = Convert.ToString(fsList.First().LookupParameter(parName).StorageType);
        if (parType is null) 
            return $"No Parameter Type for {parName}";

        foreach (var fs in fsList)
        {
            var typeMark = fs.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString();
            for (var r = startRow; r <= totRows; r++)
            {
                var keyCellVal = Convert.ToString(((XL.Range)_ws.Cells[r, keyCol]).Value);
                if (typeMark != keyCellVal) 
                    continue;
                    
                bool hasUnits;

                try
                {
#if PRE21
                    var _ = fs.LookupParameter(parName).DisplayUnitType;
#else
                    var _ = fs.LookupParameter(parName).GetUnitTypeId();
#endif
                    hasUnits = true;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    hasUnits = false;
                }

#if PRE21
                var newVal = hasUnits ? GetCellVal(r, parCol, parType, fs.LookupParameter(parName).DisplayUnitType) : GetCellVal(r, parCol, parType);
#else
                dynamic? newVal = hasUnits ? GetCellVal(r, parCol, parType, fs.LookupParameter(parName).GetUnitTypeId()) : GetCellVal(r, parCol, parType);
#endif

                if (newVal == null)
                {
                    errorLog += $"Incorrect data type in Excel File for element '{typeMark}' parameter '{parName}.' Parameter will not be assigned.\n";
                    continue;
                }

                using var tx = new Transaction(_doc, "Assign Type Parameter");
                if (tx.Start() != TransactionStatus.Started)
                    continue;
                fs.LookupParameter(parName).Set(newVal);
                tx.Commit();

            }
        }

        return errorLog;
    }

    public static string AssignInstParameters(string familyName, string parName, int keyCol, int startRow, int parCol)
    {
        var errorLog = string.Empty;

        var totRows = _ws.UsedRange.Rows.Count;

        var fis = new FilteredElementCollector(_doc)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .Where(q => q.Symbol.Family.Name == familyName)
            .ToList();

        var parType = Convert.ToString(fis.First().LookupParameter(parName).StorageType);
        if (parType is null)
            return $"No Parameter Type for {parName}";

        var rvtMarks = fis
            .Select(x => x.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString())
            .ToList();
        var markRowIndex = new Dictionary<string, int>();

        for (var r = startRow; r <= totRows; r++)
        {
            var curXlMark = Convert.ToString(((XL.Range)_ws.Cells[r, keyCol]).Value) ?? string.Empty;
            if (rvtMarks.Contains(curXlMark))
                markRowIndex.Add(curXlMark, r);
        }

        var fisMatch = fis.Where(f =>
            markRowIndex.ContainsKey(f.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString()));


        foreach (var fi in fisMatch)
        {
            var mark = fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
            var row = markRowIndex[mark];
            var par = fi.LookupParameter(parName);
            var hasUnits = true;
            try
            {
#if PRE21
                var _ = fi.LookupParameter(parName).DisplayUnitType;
#else
                var _ = fi.LookupParameter(parName).GetUnitTypeId();
#endif
            }
            catch (Autodesk.Revit.Exceptions.InvalidOperationException)
            {
                hasUnits = false;
            }

#if PRE21
            var newVal = hasUnits ? GetCellVal(row, parCol, parType, par.DisplayUnitType) : GetCellVal(row, parCol, parType);
#else
            dynamic? newVal = hasUnits ? GetCellVal(row, parCol, parType, par.GetUnitTypeId()) : GetCellVal(row, parCol, parType);
#endif
            if (newVal == null)
            {
                errorLog += $"Incorrect data type in Excel file for element '{mark}' parameter '{parName}.' Expecting type of {par.StorageType}. Parameter will not be assigned.\n";
                continue;
            }
            if (ParameterToString(par, parType) == Convert.ToString(newVal)) 
                continue;

            using var tx = new Transaction(_doc, "Assign Instance Parameter");
            if (tx.Start() != TransactionStatus.Started) 
                continue;
            par.Set(newVal);
            tx.Commit();
        }

        return errorLog;
    }

    private static string? ParameterToString(Parameter par, string parType)
    {
        return parType switch
        {
            "String" => par.AsString(),
            "Integer" => Convert.ToString(par.AsInteger()),
            "Double" => Convert.ToString(par.AsDouble(), CultureInfo.CurrentCulture),
            "ElementId" => Convert.ToString(par.AsElementId()),
            _ => null
        };
    }
}