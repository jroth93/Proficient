using Autodesk.Internal.InfoCenter;
using System.Data;
using System.Globalization;
using System.Threading;
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

#if PRE22
        return fec.Any(it => it.TaggedLocalElementId == el.Id);
#else
        return fec.Any(it => it.GetTaggedLocalElementIds().Contains(el.Id));
#endif

    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
    public static void ToggleLeader()
    {
#if PRE25
        PropertyCondition typeCond = new (AutomationElement.ControlTypeProperty, ControlType.CheckBox);
        PropertyCondition nameCond = new (AutomationElement.NameProperty, "Leader");
#else
        PropertyCondition typeCond = new (AutomationElement.ControlTypeProperty, ControlType.Button);
        PropertyCondition nameCond = new (AutomationElement.NameProperty, "Leader Line");
#endif

        AndCondition ac = new (typeCond, nameCond);

        var curWindow = AutomationElement.FromHandle(GetForegroundWindow());

        if (!curWindow.Current.Name.Contains("Autodesk Revit 20")) return;

        var leaderToggle = curWindow.FindFirst(TreeScope.Descendants, ac);

        var tp = leaderToggle.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
        tp?.Toggle();
    }
    public static void ToggleLeaderEnd()
    {
        PropertyCondition typeCond = new (AutomationElement.ControlTypeProperty, ControlType.ComboBox);

        var curWindow = AutomationElement.FromHandle(GetForegroundWindow());

        if (!curWindow.Current.Name.Contains("Autodesk Revit 20")) return;

#if PRE25
        var endDrop = curWindow
            .FindAll(TreeScope.Descendants, typeCond)
            .Cast<AutomationElement>()
            .FirstOrDefault(ae => ae.Current.Name is "Free End" or "Attached End");

        if (endDrop is null) return;

        string newEndValue = endDrop.Current.Name == "Attached End" ? "Free End" : "Attached End";


        var newSelection = endDrop
            .FindFirst(TreeScope.Subtree, new PropertyCondition(AutomationElement.NameProperty, newEndValue))
            .GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
        
        newSelection?.Select();
#else
        PropertyCondition nameCond = new(AutomationElement.NameProperty, "Leader Type");
        AndCondition ac = new(typeCond, nameCond);

        PropertyCondition liTypeCond = new(AutomationElement.ControlTypeProperty, ControlType.ListItem);

        var endDrop = curWindow.FindFirst(TreeScope.Descendants, ac);

        TreeWalker tw = TreeWalker.RawViewWalker;
        var li = tw.GetFirstChild(endDrop);
        var sip = (li.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern);

        if (sip is not null && !sip.Current.IsSelected)
        {
            sip.Select();
        }
        else
        {
            li = tw.GetNextSibling(li);
            sip = (li.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern);
            sip?.Select();
        }
#endif
    }
    public static void ChangeLeaderDistance(bool increase)
    {
        var typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit);
        var curWindow = AutomationElement.FromHandle(GetForegroundWindow());

        if (!curWindow.Current.Name.Contains("Autodesk Revit 20")) return;

#if PRE25
        var ldBox = curWindow
            .FindAll(TreeScope.Descendants, typeCond)
            .OfType<AutomationElement>()
            .Where(ae => ae.GetCurrentPattern(ValuePattern.Pattern) is ValuePattern vp && vp.Current.Value.Contains("\""))
            .First();
#else
        PropertyCondition nameCond = new(AutomationElement.NameProperty, "Leader Length");
        AndCondition ac = new(typeCond, nameCond);
        var ldBox = curWindow.FindFirst(TreeScope.Descendants, ac);
#endif

        if (ldBox.GetCurrentPattern(ValuePattern.Pattern) is not ValuePattern ldVp) return;

        string ldVal = ldVp.Current.Value;
        string exp = ldVal.Substring(0, ldVal.Length - 1).Replace(" ","+");
        var curVal = Convert.ToDouble(new DataTable().Compute(exp, null));
        int sign = increase ? 1 : -1;
        double newVal = curVal + sign / 8.0;
        if (newVal <= 0) newVal = 0.125;
        ldVp.SetValue(Convert.ToString(newVal, CultureInfo.CurrentCulture));

        //return focus and wiggle mouse
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
#if PRE24
            AddSharedParameter(doc, revit.Application, BuiltInCategory.OST_ProjectInformation,
                BuiltInParameterGroup.PG_GENERAL, "Titleblock", Names.Parameter.ProjectFolder);
#else
            AddSharedParameter(doc, revit.Application, BuiltInCategory.OST_ProjectInformation,
                GroupTypeId.General, "Titleblock", Names.Parameter.ProjectFolder);
#endif
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

#if PRE24
    public static void AddSharedParameter(Document doc, UIApplication uiApp, BuiltInCategory bic, BuiltInParameterGroup pg, string defGroup, string parName)
    {
        var cs = uiApp.Application.Create.NewCategorySet();
        cs.Insert(doc.Settings.Categories.get_Item(bic));

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

#else
    public static void AddSharedParameter(Document doc, UIApplication uiApp, BuiltInCategory bic, ForgeTypeId typeId, string defGroup, string parName)
    {
        AddSharedParameter(doc, uiApp, bic, typeId, defGroup, parName);
    }
#endif
}