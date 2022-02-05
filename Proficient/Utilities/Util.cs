using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Test.Input;
using System.Windows.Forms;

namespace Proficient
{
    class Util
    {
        public static bool IsTagged(Document doc, ElementId viewId, Element el)
        {
            var fec = new FilteredElementCollector(doc).OfClass(typeof(IndependentTag)).Where(tg => tg.OwnerViewId == viewId);
            foreach (IndependentTag it in fec)
            {
                if (it.TaggedLocalElementId == el.Id)
                {
                    return true;
                }
            }
            return false;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        public static void ToggleLeader()
        {
            PropertyCondition typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.CheckBox);
            PropertyCondition nameCond = new PropertyCondition(AutomationElement.NameProperty, "Leader");
            AndCondition ac = new AndCondition(typeCond, nameCond);

            AutomationElement curWindow = AutomationElement.FromHandle(GetForegroundWindow());

            if (!curWindow.Current.Name.Contains("Autodesk Revit 20"))
            {
                return;
            }

            AutomationElement check = curWindow.FindFirst(TreeScope.Descendants, ac);

            TogglePattern tp = check.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
            tp.Toggle();
        }

        public static void ToggleLeaderEnd()
        {
            PropertyCondition typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ComboBox);

            AutomationElement curWindow = AutomationElement.FromHandle(GetForegroundWindow());

            if(!curWindow.Current.Name.Contains("Autodesk Revit 20"))
            {
                return;
            }

            var aec = curWindow.FindAll(TreeScope.Descendants, typeCond);
            foreach (AutomationElement ae in aec)
            {
                if (ae.Current.Name == "Free End")
                {
                    AutomationElement listItem = ae.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, "Attached End"));
                    SelectionItemPattern sip = listItem.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                    sip.Select();
                }
                else if (ae.Current.Name == "Attached End")
                {
                    AutomationElement listItem = ae.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, "Free End"));
                    SelectionItemPattern sip = listItem.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
                    sip.Select();
                }
            }
        }

        public static void CycleLeaderDistance()
        {
            string[] vals = new string[] { "1/4\"", "1/2\"", "3/4\"", "1\"" };
            PropertyCondition typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
            AutomationElement curWindow = AutomationElement.FromHandle(GetForegroundWindow());
            var aec = curWindow.FindAll(TreeScope.Descendants, typeCond);

            if (!curWindow.Current.Name.Contains("Autodesk Revit 20"))
            {
                return;
            }

            foreach (AutomationElement ae in aec)
            {
                ValuePattern vp = ae.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
                int index = Array.IndexOf(vals, vp.Current.Value);
                if (vp.Current.Value.Contains("\""))
                {
                    if (index != -1 && index != vals.Length - 1)
                    {
                        vp.SetValue(vals[index + 1]);
                    }
                    else
                    {
                        vp.SetValue(vals[0]);
                    }
                    break;
                }
            }

            PropertyCondition vpCond = new PropertyCondition(AutomationElement.NameProperty, "Xceed.Wpf.AvalonDock.Layout.LayoutDocument");
            PropertyCondition paneCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane);
            var vpaec = curWindow.FindAll(TreeScope.Descendants, vpCond);

            foreach(AutomationElement ae in vpaec)
            {
                if(ae.FindFirst(TreeScope.Descendants, paneCond) != null)
                {
                    ae.SetFocus();
                    break;
                }
            }

            System.Drawing.Point origin = Cursor.Position;
            Mouse.MoveTo(new System.Drawing.Point(origin.X, origin.Y + 1));
            Mouse.MoveTo(origin);
            Mouse.Click(MouseButton.Middle);

        }

        public enum ViewPlane
        {
            Top = 1,
            Bottom = 2
        }
        public static double GetViewBound(Document doc, Autodesk.Revit.DB.View view, ViewPlane vp)
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

        public static string GetProjectFolder(ExternalCommandData revit)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;
            string pn = doc.Title[5] == '.' ? doc.Title.Substring(0, 7) : doc.Title.Substring(0, 5);
            var pfPars = doc.ProjectInformation.GetParameters(Names.Parameter.ProjectFolder);
            string pfPar = pfPars.Count == 0 ? null : pfPars[0].AsString();

            if(pfPar == null || pfPar == String.Empty)
            {
                string pfPath;
                var dirs = Directory.GetDirectories($@"K:\20{pn.Substring(0, 2)}\").Where(d => d.Contains(pn));

                if (dirs.Any())
                {
                    pfPath = dirs.First();
                }
                else
                {
                    string parentDir = Directory.GetDirectories($@"K:\20{pn.Substring(0, 2)}\").Where(d => d.Contains(pn.Substring(0, 5))).First();
                    pfPath = Directory.GetDirectories(parentDir).Where(d => d.Contains(pn)).First();
                }

                if(pfPar == null)
                {
                    AddSharedParameter(doc, revit.Application, BuiltInCategory.OST_ProjectInformation, BuiltInParameterGroup.PG_GENERAL, "Titleblock", Names.Parameter.ProjectFolder);
                }

                string knPath = pfPath + @"\Construction Documents\Drawings\_MEP Revit";


                using (Transaction tx = new Transaction(doc, "Assign Project Folder Parameter"))
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        doc.ProjectInformation.GetParameters(Names.Parameter.ProjectFolder)[0].Set(knPath);
                    }
                    tx.Commit();
                }
                return knPath;
            }
            return pfPar;
        }

        public static string GetProjectNumber(ExternalCommandData revit)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;
            string pn = doc.Title[5] == '.' ? doc.Title.Substring(0, 7) : doc.Title.Substring(0, 5);

            return pn;
        }

        public static void AddSharedParameter(Document doc, UIApplication uiapp, BuiltInCategory bic, BuiltInParameterGroup bipg, string defGroup, string parName)
        {
            CategorySet cset = uiapp.Application.Create.NewCategorySet();
            cset.Insert(doc.Settings.Categories.get_Item(bic));

            DefinitionFile spFile = uiapp.Application.OpenSharedParameterFile();

            ExternalDefinition eDef = spFile.Groups.Where(dg => dg.Name == defGroup)
                                                    .FirstOrDefault().Definitions
                                                    .Where(ed => ed.Name == parName)
                                                    .FirstOrDefault() as ExternalDefinition;

            using (Transaction tx = new Transaction(doc, $"Add {parName} Parameter"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    doc.ParameterBindings.Insert(eDef, uiapp.Application.Create.NewInstanceBinding(cset), bipg);
                }
                tx.Commit();
            }
        }
        public static void AddSharedParameter(Document doc, UIApplication uiapp, CategorySet cset, BuiltInParameterGroup bipg, string defGroup, string parName)
        {
            DefinitionFile spFile = uiapp.Application.OpenSharedParameterFile();

            ExternalDefinition eDef = spFile.Groups.Where(dg => dg.Name == defGroup)
                                                    .FirstOrDefault().Definitions
                                                    .Where(ed => ed.Name == parName)
                                                    .FirstOrDefault() as ExternalDefinition;

            using (Transaction tx = new Transaction(doc, $"Add {parName} Parameter"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    doc.ParameterBindings.Insert(eDef, uiapp.Application.Create.NewInstanceBinding(cset), bipg);
                }
                tx.Commit();
            }
        }

    }
}
