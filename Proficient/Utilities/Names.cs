using System;

namespace Proficient
{
    class Names
    {
        public class Family
        {
            public const string DuctFittingTag = "MEI Mech Tag Duct Fitting";
            public const string DuctTag = "MEI Mech Tag Duct";
            public const string DuctTagRotating = "MEI Mech Tag Duct - Rotating";

            public const string PipeFittingTag = "MEI Mech Tag Pipe Fitting";
            public const string PipeTag = "MEI Mech Tag Pipe";
            public const string PipeTagRotating = "MEI Mech Tag Pipe - Rotating";

            public const string RoundDuct = "Round Duct";
        }
        public class Parameter
        {
            public const string FittingUpDn = "MEI Display UP or DN";
            public const string ProjectFolder = "MEI Project Folder";
            public const string ProjectNumber = "MEI Project Number";
            public const string ViewSubdiscipline = "MEI Discipline-Sub";
        }
        public class Workset
        {
            public const string MechEnlarged = "M-Enlarged Plans";
            public const string ElecEnlarged = "E-Enlarged Plans";
            public const string ElecSite = "E-Site";
        }
        public class File
        {
            public const string SharedParameters = @"Z:\Revit MEI Content\Shared Parameters\MEI Shared Parameters.txt";
            public const string ServerDll = @"Z:\Revit\Custom Add Ins\Proficient.bundle\Contents\Proficient.dll";
            public const string ServerAddinFolder = @"Z:\Revit\Custom Add Ins";
            public const string UserSettings = @"C:\ProgramData\Autodesk\ApplicationPlugins\Proficient.bundle\Contents\config.json";
            public const string CommonSettings = @"Z:\Revit\Custom Add Ins\Proficient Config Files\appsettings.json";
            public const string UserDllFolder = @"C:\ProgramData\Autodesk\ApplicationPlugins\Proficient.bundle\Contents";
        }

        public class Guids
        {
            public static Guid ViewSchema = new Guid("B65973F8-FED4-44BF-9351-215D24F3DCF1");
        }

    }
}
