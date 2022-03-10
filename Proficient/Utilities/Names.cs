using System;

namespace Proficient
{
    class Names
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
        }
        public class Workset
        {
            public const string MechEnlarged = "M-Enlarged Plans";
            public const string ElecEnlarged = "E-Enlarged Plans";
            public const string ElecSite = "E-Site";
        }
        public class File
        {
            public const string UserSettings = @"C:\ProgramData\Autodesk\ApplicationPlugins\Proficient.bundle\Settings.json";
            public const string DependencyDir = @"C:\ProgramData\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\Dependencies\";
            public const string SharedParameters = @"Z:\Revit MEI Content\Shared Parameters\MEI Shared Parameters.txt";
            public const string ServerDll = @"Z:\Revit\Proficient\Proficient.bundle\Contents\R22\Proficient.dll";
            public const string CommonSettings = @"Z:\Revit\Proficient\Proficient Config Files\appsettings.json";
            public const string KnTempFile = @"Z:\Revit\Keynotes Template.xlsx";
            public const string SilentUpdateExe = @"Z:\Revit\Proficient\ProficientSilentUpdate.exe";
        }

        public class Guids
        {
            public static Guid ViewSchema = new Guid("B65973F8-FED4-44BF-9351-215D24F3DCF1");
        }

    }
}
