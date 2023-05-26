using Autodesk.Internal.InfoCenter;
using System.Data;
using System.Globalization;
using System.Windows.Automation;
using System.Windows.Forms;

namespace Proficient.Utilities;

internal class Util
{
    public static bool IsTagged(Document doc, ElementId viewId, Element el)
    {
        var fec = 
            new FilteredElementCollector(doc, viewId)
                .OfClass(typeof(IndependentTag))
                .Cast<IndependentTag>()
                .Where(e => e.Category.Id.IntegerValue != (int)BuiltInCategory.OST_KeynoteTags);

#if R19 || R20 || R21
        return fec.Any(it => it.TaggedLocalElementId == el.Id);
#else
        return fec.Any(it => it.GetTaggedLocalElementIds().Contains(el.Id));
#endif

    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    public static void ToggleLeader()
    {
        PropertyCondition typeCond = new (AutomationElement.ControlTypeProperty, ControlType.CheckBox);
        PropertyCondition nameCond = new (AutomationElement.NameProperty, "Leader");
        AndCondition ac = new (typeCond, nameCond);

        var curWindow = AutomationElement.FromHandle(GetForegroundWindow());

        if (!curWindow.Current.Name.Contains("Autodesk Revit 20")) return;

        var check = curWindow.FindFirst(TreeScope.Descendants, ac);

        var tp = check.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
        tp?.Toggle();
    }
    public static void ToggleLeaderEnd()
    {
        PropertyCondition typeCond = new (AutomationElement.ControlTypeProperty, ControlType.ComboBox);

        var curWindow = AutomationElement.FromHandle(GetForegroundWindow());

        if (!curWindow.Current.Name.Contains("Autodesk Revit 20")) return;

        var endDrop = curWindow
            .FindAll(TreeScope.Descendants, typeCond)
            .Cast<AutomationElement>()
            .FirstOrDefault(ae => ae.Current.Name is "Free End" or "Attached End");

        if (endDrop is null) return;

        string newEnd = endDrop.Current.Name == "Attached End" ? "Free End" : "Attached End";

        (endDrop.FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, newEnd))
            .GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern)?.Select();

    }
    public static void ChangeLeaderDistance(bool increase)
    {
        var typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
        var curWindow = AutomationElement.FromHandle(GetForegroundWindow());
        var aec = curWindow.FindAll(TreeScope.Descendants, typeCond).OfType<AutomationElement>();

        if (!curWindow.Current.Name.Contains("Autodesk Revit 20")) return;

        var tbVp = aec
            .Where(ae => ae.GetCurrentPattern(ValuePattern.Pattern) is ValuePattern vp && vp.Current.Value.Contains("\""))
            .Select(ae => ae.GetCurrentPattern(ValuePattern.Pattern))
            .Cast<ValuePattern>()
            .First();
            
        string tbVal = tbVp.Current.Value;
        string exp = tbVal.Substring(0, tbVal.Length - 1).Replace(" ","+");
        var curVal = Convert.ToDouble(new DataTable().Compute(exp, null));
        int sign = increase ? 1 : -1;
        double newVal = curVal + sign / 8.0;
        if (newVal <= 0) newVal = 0.125;
        tbVp.SetValue(Convert.ToString(newVal, CultureInfo.CurrentCulture));

        var vpCond = new PropertyCondition(AutomationElement.NameProperty, "Xceed.Wpf.AvalonDock.Layout.LayoutDocument");
        var paneCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane); 
        curWindow
            .FindAll(TreeScope.Descendants, vpCond)
            .OfType<AutomationElement>()
            .First(ae => ae.FindFirst(TreeScope.Descendants, paneCond) != null)
            .SetFocus();

        var origin = Cursor.Position;
        Mouse.MoveTo(origin with {Y = origin.Y + 1});
        Mouse.MoveTo(origin);
    }

    public enum ViewPlane
    {
        Top = 1,
        Bottom = 2
    }
    public static double GetViewBound(Document doc, Autodesk.Revit.DB.View view, ViewPlane vp)
    {
        if (view is not ViewPlan viewPlan) return 0;
        
        var viewRange = viewPlan.GetViewRange();

        var pvp = vp == ViewPlane.Top ? PlanViewPlane.TopClipPlane : PlanViewPlane.BottomClipPlane;
        double elev = (doc.GetElement(viewRange.GetLevelId(pvp)) is Level l) ? l.ProjectElevation : 0;
        double offset = viewRange.GetOffset(pvp);

        return elev + offset;
    }

    public static void BalloonTip(string category, string title, string text)
    {
        ResultItem ri = new()
        {
            Category = category,
            Title = title,
            TooltipText = text
        };

        Autodesk.Windows.ComponentManager.InfoCenterPaletteManager.ShowBalloon(ri);
    }
    public static void BalloonTip(string category, string title, string text, string uri)
    {
        ResultItem ri = new ()
        {
            Category = category,
            Title = title,
            TooltipText = text,
            Uri = new Uri(uri)
        };

        Autodesk.Windows.ComponentManager.InfoCenterPaletteManager.ShowBalloon(ri);
    }

    public static string GetProjectFolder(ExternalCommandData revit)
    {
        var doc = revit.Application.ActiveUIDocument.Document;
        string pn = doc.Title[5] == '.' ? doc.Title.Substring(0, 7) : doc.Title.Substring(0, 5);
        var pfPar = doc.ProjectInformation.GetParameters(Names.Parameter.ProjectFolder).FirstOrDefault();

        if (pfPar is null)
        {
            AddSharedParameter(doc, revit.Application, BuiltInCategory.OST_ProjectInformation,
                BuiltInParameterGroup.PG_GENERAL, "Titleblock", Names.Parameter.ProjectFolder);
        }
        else if (pfPar.AsString() is not null && pfPar.AsString() != string.Empty)
        {
            return pfPar.AsString();
        }

        string pfPath;
        var dirs = Directory.GetDirectories($@"K:\20{pn.Substring(0, 2)}\").Where(d => d.Contains(pn)).ToList();

        if (dirs.Any())
        {
            pfPath = dirs.First();
        }
        else
        {
            string parentDir = Directory.GetDirectories($@"K:\20{pn.Substring(0, 2)}\").First(d => d.Contains(pn.Substring(0, 5)));
            pfPath = Directory.GetDirectories(parentDir).First(d => d.Contains(pn));
        }

        pfPath += @"\Construction Documents\Drawings\_MEP Revit";

        using Transaction tx = new (doc, "Assign Project Folder Parameter");
        if (tx.Start() == TransactionStatus.Started)
            doc.ProjectInformation.GetParameters(Names.Parameter.ProjectFolder)[0].Set(pfPath);
        tx.Commit();
            
        return pfPath;
    }

    public static void AddSharedParameter(Document doc, UIApplication uiApp, BuiltInCategory bic, BuiltInParameterGroup pg, string defGroup, string parName)
    {
        var cs = uiApp.Application.Create.NewCategorySet();
        cs.Insert(doc.Settings.Categories.get_Item(bic));
        AddSharedParameter(doc, uiApp, cs, pg, defGroup, parName);
    }

    public static void AddSharedParameter(Document doc, UIApplication uiApp, CategorySet cs, BuiltInParameterGroup pg, string defGroup, string parName)
    {
        var spFile = uiApp.Application.OpenSharedParameterFile();

        if (spFile == null)
        {
            uiApp.Application.SharedParametersFilename = Names.File.SharedParameters;
            spFile = uiApp.Application.OpenSharedParameterFile();
        }

        var dg = spFile.Groups.FirstOrDefault(dg => dg.Name == defGroup);
        if (dg == null) return;

        var ed = dg.Definitions.FirstOrDefault(ed => ed.Name == parName)
            as ExternalDefinition;

        using Transaction tx = new(doc, $"Add {parName} Parameter");
        if (ed is null || tx.Start() != TransactionStatus.Started) return;
        doc.ParameterBindings.Insert(ed, uiApp.Application.Create.NewInstanceBinding(cs), pg);
        tx.Commit();
    }

}