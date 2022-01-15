using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace Proficient.Keynotes
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class KNLauncher : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;

            string pn = doc.Title[5] == '.' ? doc.Title.Substring(0, 7) : doc.Title.Substring(0, 5);

            var ps = Process.GetProcessesByName("excel").Where(p => p.MainWindowTitle.Contains($"{pn} Keynotes.xlsx"));

            if (ps.Any())
            {
                SetForegroundWindow(ps.First().MainWindowHandle);
            }          
            else
            {
                string filePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath()) ?? doc.PathName;
                string fileDir = filePath.Substring(0, 7) == "BIM 360" || filePath.Substring(0, 8) == "Autodesk" ?
                    Util.GetProjectFolder(revit) :
                    Path.GetDirectoryName(filePath);
                string xlPath = $"{fileDir}\\{pn} Keynotes.xlsx";

                if (!File.Exists(xlPath))
                {
                    File.Copy(Names.File.KnTempFile, xlPath);
                }

                Application xlApp = new Application();
                xlApp.Workbooks.Open(xlPath);
                xlApp.Visible = true;
                SetForegroundWindow(new IntPtr(xlApp.ActiveWindow.Hwnd));
                
            }

            return Result.Succeeded;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
