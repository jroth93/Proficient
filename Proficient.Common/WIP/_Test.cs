using Autodesk.Revit.DB.Analysis;
using Proficient.Utilities;
using Proficient.Forms;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Controls;
//using System.Windows.Forms;
//using System.Windows.Input;
using UIFramework;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;
using DTEU = Proficient.Utilities.DocumentTabEventUtils;
using PFRF = Autodesk.Revit.DB.ParameterFilterRuleFactory;
using Autodesk.Revit.DB;
using Orientation = System.Windows.Controls.Orientation;

using System.Text.RegularExpressions;
using Autodesk.Revit.ApplicationServices;
using Application = Autodesk.Revit.Creation.Application;


namespace Proficient.WIP;

[Transaction(TransactionMode.Manual)]
class _Test : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;
        //var vs = view as ViewSchedule;
        //var td = vs?.GetTableData();
        //var id = (view as ViewSchedule).GetTableData().GetSectionData(1).GetCellParamId(4, 3);
        //var par = doc.GetElement(id);

        var selIds = uiDoc.Selection.GetElementIds();
        var id = selIds.First();
        var el = doc.GetElement(id);


        using var tx = new Transaction(doc, "Test");
        if (tx.Start() != TransactionStatus.Started) return Result.Failed;

        tx.Commit();


#if taborganizer
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
#endif

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

        return Result.Succeeded;
            
    }


    public static void GetAllMethods(Type t)
    {


        var st = new StringBuilder();
        var logger = new StreamWriter(@"C:\Users\jroth\Desktop\methods.txt");

        MethodInfo[] docInternalMethods = t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (MethodInfo m in docInternalMethods)
        {
            var rep1 = "";
            try
            {
                rep1 = m.GetParameters().ToList().Select(x => x.Name).Aggregate((a, x) => a + ", " + x);
            }
            catch
            {

            }

            var rep = " Method: " + m.Name + ", Attribute: " + m.Attributes + ", CallingConvention: " +
                         m.CallingConvention
                         + ", Contains Generic Parameters: " + m.ContainsGenericParameters + ", Custom Attribute: " +
                         m.CustomAttributes
                         + ", Declaring Type: " + m.DeclaringType + ", Member Type: " + m.MemberType +
                         ", Parameters: " + rep1;
            logger.WriteLine(rep);
        }

    }

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

}
