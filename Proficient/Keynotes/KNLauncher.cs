using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class KNLauncher : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;

            string pn = doc.Title[5] == '.' ? doc.Title.Substring(0, 7) : doc.Title.Substring(0, 5);

            string filePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath()) ?? doc.PathName;
            string fileDir =filePath.Substring(0, 7) == "BIM 360" || filePath.Substring(0, 8) == "Autodesk" ? Util.GetProjectFolder(revit) : Path.GetDirectoryName(filePath);

            string xlPath = File.Exists($"{fileDir}\\{pn} Keynotes.xlsm") ? $"{fileDir}\\{pn} Keynotes.xlsm" : $"{fileDir}\\{pn} Keynotes.xlsx";

            if(!File.Exists(xlPath))
            {
                File.Copy(Names.File.KnTempFile, xlPath);
            }

            bool isOpen = false;
            Excel.Application xl;
            try
            {
                xl = Marshal.GetActiveObject("Excel.Application") as Excel.Application;
                foreach (Excel.Workbook wb in xl.Workbooks)
                {
                    if (wb.Name == Path.GetFileName(xlPath))
                    {
                        isOpen = true;
                    }
                }
            }
            catch (COMException)
            {
                xl = new Excel.Application();
            }

            if (!isOpen) xl.Workbooks.Open(xlPath);

            SetForegroundWindow(FindWindow(null, xl.Caption));
            xl.Visible = true;

            return Result.Succeeded;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}
