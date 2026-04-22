namespace Proficient.Utilities;

internal class Names
{
    public class Family
    {
        //Tags
        public const string PipeFittingTag = "MEI Mech Tag Pipe Fitting";
        public const string PipeTag = "MEI Mech Tag Pipe";
        public const string PipeTagRotating = "MEI Mech Tag Pipe - Rotating";
        public const string DuctFittingTag = "MEI Mech Tag Duct Fitting";
        public const string DuctTag = "MEI Mech Tag Duct";
        public const string DuctTagRotating = "MEI Mech Tag Duct - Rotating";

        //Ducts
        public const string RoundDuct = "Round Duct";
        public const string MiteredElbow = "MEI Rect Elbow Mitered";

        //Annotations
        public const string BlankLeader = "MEI Blank Leader";
        public const string KeynoteTag = "MEI Keynote Tag";
    }
    public class Parameter
    {
        public const string FittingUpDn = "MEI Display UP or DN";
        public const string ProjectFolder = "MEI Project Folder";
        public const string ProjectNumber = "MEI Project Number";
        public const string ViewSubdiscipline = "MEI Discipline-Sub";
        public const string DisplaySeparation = "MEI Display Separation";
        public const string BreakerOptions = "MEI Breaker Options";
        public const string DisplayVanes = "MEI Display Vanes";
        public const string AtReturnAirTag = "Display Return Air Tag";
        public const string AtRhTag = "Display Righthand Tag";
        public const string AtLdrLength = "Leader Line Length";

    }
    public class Workset
    {
        public const string MechEnlarged = "M-Enlarged Plans";
        public const string ElecEnlarged = "E-Enlarged Plans";
        public const string ElecSite = "E-Site";
    }
    public class File
    {
        public static string UserSettings = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\ApplicationPlugins\Proficient.bundle\Settings.json";
        public const string SharedParameters = @"Z:\Revit MEI Content\Shared Parameters\MEI Shared Parameters.txt";
        public const string ServerDll = @"Z:\Revit\Proficient\Proficient.bundle\Contents\R24\Proficient.dll";
        public const string CommonSettings = @"Z:\Revit\Proficient\Proficient Config Files\appsettings.json";
        public const string KnTempFile = @"Z:\Revit\Keynotes Template.xlsx";
        public const string ProficientInstaller = @"Z:\Revit\Proficient\ProficientInstaller.exe";
    }

    public class Guids
    {
        public static Guid ProficientSchema = new ("7487EBF8-6F17-41BA-B531-C93DB46F2DD9");
        public static Guid ElecLoadDmu = new ("088EE9E1-8EFA-438D-9287-F180436519BD");
        public static Guid BreakerDmu = new ("86168304-BB36-4B81-B498-D19FFF96377B");
        public static Guid DuctFittingDmu = new("76364FDC-B97B-4D3A-BDA7-EC6DD273B60F");
    }

}