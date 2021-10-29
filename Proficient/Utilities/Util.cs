using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Proficient
{
    class Util
    {
        public static bool IsTagged(Document doc, ElementId viewid, Element el)
        {
            FilteredElementCollector fec = new FilteredElementCollector(doc, viewid).OfClass(typeof(IndependentTag));
            foreach (IndependentTag it in fec)
            {
                if (it.TaggedLocalElementId == el.Id)
                {
                    return true;
                }
            }
            return false;
        }

        public enum ViewPlane
        {
            Top = 1,
            Bottom = 2
        }
        public static double GetViewBound(Document doc, View view, ViewPlane vp)
        {
            if (view is ViewPlan)
            {
                ViewPlan viewPlan = view as ViewPlan;
                PlanViewRange viewRange = viewPlan.GetViewRange();

                PlanViewPlane pvp = vp == ViewPlane.Top ? PlanViewPlane.TopClipPlane : PlanViewPlane.BottomClipPlane;
                double elev = (doc.GetElement(viewRange.GetLevelId(pvp)) as Level).Elevation;
                double offset = viewRange.GetOffset(pvp);

                return elev + offset;
            }
            else
            {
                return 0;
            }

        }

        public static void BalloonTip(string category, string title, string text)
        {
            Autodesk.Internal.InfoCenter.ResultItem ri = new Autodesk.Internal.InfoCenter.ResultItem();

            ri.Category = category;
            ri.Title = title;
            ri.TooltipText = text;

            Autodesk.Windows.ComponentManager.InfoCenterPaletteManager.ShowBalloon(ri);
        }

        public static void BalloonTip(string category, string title, string text, string uri)
        {
            Autodesk.Internal.InfoCenter.ResultItem ri = new Autodesk.Internal.InfoCenter.ResultItem();

            ri.Category = category;
            ri.Title = title;
            ri.TooltipText = text;
            ri.Uri = new Uri(uri);

            Autodesk.Windows.ComponentManager.InfoCenterPaletteManager.ShowBalloon(ri);
        }


        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int SetWindowText(IntPtr hWnd, string lpString);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        public static void SetStatusText(string text)
        {
            IntPtr mainWindow = Process.GetCurrentProcess().MainWindowHandle;
            IntPtr statusBar = FindWindowEx(mainWindow, IntPtr.Zero, "msctls_statusbar32", "");

            if (statusBar != IntPtr.Zero)
            {
                SetWindowText(statusBar, text);
            }
        }

        public static string GetProjectFolder(ExternalCommandData revit)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;
            string pn = doc.Title[5] == '.' ? doc.Title.Substring(0, 7) : doc.Title.Substring(0, 5);
            bool parExists = doc.ProjectInformation.GetParameters(Names.Parameter.ProjectFolder).Count > 0;
            string projFolder = parExists ? doc.ProjectInformation.GetParameters(Names.Parameter.ProjectFolder)[0].AsString() : String.Empty;

            if (String.IsNullOrEmpty(projFolder))
            {
                string projDir = Directory.GetDirectories($@"K:\20{pn.Substring(0, 2)}\").ToList().Where(d => d.Contains(pn)).First();

                if (string.IsNullOrEmpty(projDir))
                {
                    string parentProjDir = Directory.GetDirectories($@"K:\20{pn.Substring(0, 2)}\").ToList().Where(d => d.Contains(pn.Substring(0, 5))).First();
                    projDir = Directory.GetDirectories(parentProjDir).Where(d => d.Contains(pn)).First();
                }

                projFolder = $@"{projDir}\Construction Documents\Drawings\_MEP Revit";

                if (!parExists)
                {
                    AddSharedParameter(doc, revit.Application, BuiltInCategory.OST_ProjectInformation, BuiltInParameterGroup.PG_GENERAL, "Titleblock", Names.Parameter.ProjectFolder);
                }

                using (Transaction tx = new Transaction(doc, "Assign Project Folder Parameter"))
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        doc.ProjectInformation.GetParameters(Names.Parameter.ProjectFolder)[0].Set(projFolder);
                    }
                    tx.Commit();
                }
            }

            return projFolder;

        }

        public static string GetProjectNumber(ExternalCommandData revit)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;
            string pn = doc.Title[5] == '.' ? doc.Title.Substring(0, 7) : doc.Title.Substring(0, 5);

            return pn;
        }

        private static void AddSharedParameter(Document doc, UIApplication uiapp, BuiltInCategory bic, BuiltInParameterGroup bipg, string defGroup, string parName)
        {
            CategorySet cset = uiapp.Application.Create.NewCategorySet();
            cset.Insert(doc.Settings.Categories.get_Item(bic));
            uiapp.Application.SharedParametersFilename = Names.File.SharedParameters;
            DefinitionFile spFile = uiapp.Application.OpenSharedParameterFile();

            ExternalDefinition eDef = spFile.Groups.Where(dg => dg.Name == defGroup).FirstOrDefault().Definitions.Where(ed => ed.Name == parName).FirstOrDefault() as ExternalDefinition;

            using (Transaction tx = new Transaction(doc, $"Add {parName} Parameter"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    doc.ParameterBindings.Insert(eDef, uiapp.Application.Create.NewInstanceBinding(cset), bipg);
                }
                tx.Commit();
            }
        }

        public static string GetKNXLPath(string fileDir, string pn)
        {
            string xlPath;

            if (File.Exists($"{fileDir}\\{pn} Keynotes.xlsx"))
            {
                return $"{fileDir}\\{pn} Keynotes.xlsx";
            }
            else if (File.Exists($"{fileDir}\\{pn} Keynotes.xlsm"))
            {
                return $"{fileDir}\\{pn} Keynotes.xlsm";
            }
            else
            {
                string spDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Morrissey Engineering, Inc\All Morrissey - Documents\Keynotes\";
                xlPath = $"{spDir}{pn}.xlsx";
                //create new file or generate error if file does not exist
                if (!File.Exists(xlPath))
                {
                    try
                    {
                        File.Copy($"{spDir}Template.xlsx", xlPath);
                    }
                    catch
                    {
                        TaskDialog td = new TaskDialog("SharePoint Sync Required");
                        td.MainContent = @"All Morrissey Team SharePoint sync is required to use this feature.";
                        td.Show();
                        return String.Empty;
                    }

                    if (!File.Exists(fileDir + $"\\Keynotes.lnk"))
                    {
                        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                        IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(fileDir + $"\\Keynotes.lnk");
                        shortcut.TargetPath = xlPath;
                        shortcut.Save();
                    }
                }
            }
            return xlPath;
        }
    }
}
