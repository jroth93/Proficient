using Autodesk.Revit.DB.Analysis;
using Proficient.Utilities;
using Proficient.Forms;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using UIFramework;
using Autodesk.Revit.UI.Selection;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using DTEU = Proficient.Utilities.DocumentTabEventUtils;
using PFRF = Autodesk.Revit.DB.ParameterFilterRuleFactory;
using Autodesk.Revit.DB;
using Orientation = System.Windows.Controls.Orientation;


namespace Proficient.WIP;

[Transaction(TransactionMode.Manual)]
class _Test : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var app = revit.Application;
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        /*
        //var eid = uiDoc.Selection.GetElementIds().First();
        var linkRef = uiDoc.Selection.PickObject(ObjectType.LinkedElement, "Pick an element");
        if (doc.GetElement(linkRef) is not RevitLinkInstance linkInst) return Result.Cancelled;
        var link = linkInst.GetLinkDocument();
        
        var el = link.GetElement(linkRef.LinkedElementId);

        if (view.ViewTemplateId != ElementId.InvalidElementId)
            view = (Autodesk.Revit.DB.View) doc.GetElement(view.ViewTemplateId);
        var cat = el.Category;
        string name = ((FamilyInstance) el).Symbol.FamilyName;
        var rule = PFRF.CreateEqualsRule(new ElementId(BuiltInParameter.ALL_MODEL_FAMILY_NAME),name,true);
        */


        var sfm = SpatialFieldManager.GetSpatialFieldManager(view);

        var wndRoot = (MainWindow)UIAppEventUtils.GetWindowRoot(app);
        if (wndRoot == null)
            return Result.Failed;

        var dm = MainWindow.FindFirstChild<DockingManager>(wndRoot);
        var lp = dm.Layout.Children.First(c => c is LayoutPanel) as LayoutPanel;
        var ldpg = lp.Children.First(c => c is LayoutDocumentPaneGroup) as LayoutDocumentPaneGroup;
        var ldps = ldpg.Children;
        var ld = ldps[0].Children.First();

        //ldpg.Orientation = Orientation.Horizontal;
        //ldps[0].RemoveChild(ld);
        //(ldps[1] as LayoutDocumentPane).Children.Add(ld as LayoutDocument);

        dm.UpdateLayout();
        dm.UpdateDefaultStyle();

        //(view as ViewSchedule).GetTableData().GetSectionData(0).GetCellText(1, 0);


        var ms = view.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
        //MethodInfo getMFCDocMethod = doc.GetType().GetMethod("getMFCDoc", BindingFlags.Instance | BindingFlags.NonPublic);
        //object mfcDoc = getMFCDocMethod.Invoke(doc, new object[] { });
        //ViewTemplatesDlgUtil();
        //ControlHost



#if CLFollower
            Follower f = new Forms.Follower(revit);
            System.Windows.Controls.TextBox tb = new System.Windows.Controls.TextBox
            {
                Margin = new System.Windows.Thickness(3, 0, 3, 10)
            };

            f.lbl1.Content = "Enter a number";
            f.wrapper.Children.Add(tb);
            tb.Focus();

            f.KeyDown += (object sender, KeyEventArgs e) =>
            {
                if(e.Key == Key.Enter)
                {
                    f.DialogResult = true;
                    f.Close();
                }
                else if(e.Key == Key.Escape)
                {
                    f.DialogResult = false;
                    f.Close();
                }
            };

            f.ShowDialog();
#endif
        /*
        using Transaction tx = new (doc, "commandname");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;
        var filter = ParameterFilterElement.Create(doc, name, new List<ElementId> { cat.Id }, new ElementParameterFilter(rule));
        view.AddFilter(filter.Id);
        view.SetFilterVisibility(filter.Id, false);
        tx.Commit();
        */
        return Result.Succeeded;
            
    }

    static string GetTabUniqueId(TabItem tab)
    {
        return $"{((LayoutDocument)tab.Header).Title}+{tab.GetHashCode()}+{tab.IsSelected}";
    }

    static long GetTabDocumentId(LayoutContent tab)
    {
        return(
            (MFCMDIFrameHost)(
                (MFCMDIChildFrameControl)tab.Content
            ).Content
        ).document.ToInt64();
    }
    static long GetTabDocumentId(TabItem tab)
    {
        return (
            (MFCMDIFrameHost)(
                (MFCMDIChildFrameControl)(
                    (LayoutDocument)tab.Content
                ).Content
            ).Content
        ).document.ToInt64();
    }

    static long GetAPIDocumentId(Document doc)
    {
        MethodInfo getMFCDocMethod = doc.GetType().GetMethod("getMFCDoc", BindingFlags.Instance | BindingFlags.NonPublic);
        object mfcDoc = getMFCDocMethod.Invoke(doc, new object[] { });
        MethodInfo ptfValMethod = mfcDoc.GetType().GetMethod("GetPointerValue", BindingFlags.Instance | BindingFlags.NonPublic);
        return ((IntPtr)ptfValMethod.Invoke(mfcDoc, new object[] { })).ToInt64();
    }

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("RevitMFC.dll")]
    static extern IntPtr ViewTemplatesDlgUtil();


}