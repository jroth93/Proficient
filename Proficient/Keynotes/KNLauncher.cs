using Proficient.Utilities;

namespace Proficient.Keynotes;

[Transaction(TransactionMode.Manual)]
internal class KnLauncher : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var doc = revit.Application.ActiveUIDocument.Document;

        var pn = doc.Title[5] == '.' ? 
            doc.Title.Substring(0, 7) : 
            doc.Title.Substring(0, 5);

        var ps = Process.GetProcessesByName("excel")
            .Where(p => p.MainWindowTitle.Contains($"{pn} Keynotes.xlsx"))
            .ToList()
            .FirstOrDefault();

        if (ps is not null)
        {
            SetForegroundWindow(ps.MainWindowHandle);
            return Result.Succeeded;
        }       
            
        var filePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(doc.GetWorksharingCentralModelPath()) ?? doc.PathName;
        var fileDir = filePath.Substring(0, 7) == "BIM 360" || filePath.Substring(0, 8) == "Autodesk" ?
            Util.GetProjectFolder(revit) :
            Path.GetDirectoryName(filePath);
        var xlPath = $"{fileDir}\\{pn} Keynotes.xlsx";

        if (!File.Exists(xlPath))
            File.Copy(Names.File.KnTempFile, xlPath);

        try
        {
            if(Process.Start(xlPath) is { } p)
                SetForegroundWindow(p.MainWindowHandle);
        }
        catch(NullReferenceException)
        {
            TaskDialog.Show("Excel Error", "Error Launching Excel. Resolve by 1) Going to Add or Remove Programs, 2) Searching for Microsoft 365, 3) Click Modify, 4) Run the Quick Repair.");
            return Result.Failed;
        }

        return Result.Succeeded;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}