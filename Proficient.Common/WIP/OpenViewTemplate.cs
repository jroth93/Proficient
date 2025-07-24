using Proficient.Utilities;
using System.Windows.Automation;
using System.Windows.Forms;


namespace Proficient.WIP;

[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
internal class OpenViewTemplate : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        PropertyCondition typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane);
        PropertyCondition nameCond = new PropertyCondition(AutomationElement.NameProperty, "Properties");

        AndCondition ac = new AndCondition(typeCond, nameCond);

        AutomationElement revitElement = AutomationElement.FromHandle(GetForegroundWindow());
        AutomationElement propDock = revitElement.FindFirst(TreeScope.Descendants, ac);

        PropertyCondition custNameCond = new PropertyCondition(AutomationElement.NameProperty, "Custom1");

        AutomationElement propPane = propDock.FindFirst(TreeScope.Descendants, custNameCond);

        TreeWalker tw = TreeWalker.ControlViewWalker;

        System.Windows.Rect br = (System.Windows.Rect)propPane.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
        bool hReq = br.Height > 650.0;
        int yCoord = hReq? Convert.ToInt32(br.Top) + 620 : Convert.ToInt32(br.Bottom) - 250;
        double scrollDist = hReq ? 200.0 : -200.0;
        System.Drawing.Point origin = Cursor.Position;
        Mouse.Reset();
        Mouse.MoveTo(new System.Drawing.Point(Convert.ToInt32((br.Left + br.Right)/2), yCoord));
        Mouse.Scroll(scrollDist);

        Mouse.Click(MouseButton.Left);
        Mouse.MoveTo(origin);

        return Result.Succeeded;
            
    }

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
}