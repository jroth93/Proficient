using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Controls;
//using System.Windows.Input;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Proficient.Forms;
using Microsoft.Test.Input;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class _Test : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            //View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            PropertyCondition typeCond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane);
            PropertyCondition nameCond = new PropertyCondition(AutomationElement.NameProperty, "Properties");

            AndCondition ac = new AndCondition(typeCond, nameCond);

            AutomationElement revitElement = AutomationElement.FromHandle(GetForegroundWindow());
            AutomationElement propDock = revitElement.FindFirst(TreeScope.Descendants, ac);

            PropertyCondition custNameCond = new PropertyCondition(AutomationElement.NameProperty, "Custom1");

            AutomationElement propPane = propDock.FindFirst(TreeScope.Descendants, custNameCond);

            TreeWalker tw = TreeWalker.ControlViewWalker;

            System.Windows.Rect br = (System.Windows.Rect)propPane.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            bool hReq = br.Height > 800.0;
            int yCoord = hReq? Convert.ToInt32(br.Top) + 760 : Convert.ToInt32(br.Bottom) - 280;
            double scrollDist = hReq ? 200.0 : -200.0;
            System.Drawing.Point origin = Cursor.Position;
            Mouse.Reset();
            Mouse.MoveTo(new System.Drawing.Point(Convert.ToInt32((br.Left + br.Right)/2), yCoord));
            Mouse.Scroll(scrollDist);

            Mouse.Click(MouseButton.Left);
            Mouse.MoveTo(origin);

            // ElementId eid = uidoc.Selection.GetElementIds().First();
            // Element el = doc.GetElement(eid);

            #region follower entry box
            /*
            Forms.Follower f = new Forms.Follower(revit);
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
            */
            #endregion

            using (Transaction tx = new Transaction(doc, "commandname"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    
                }

                tx.Commit();
            }

            return Result.Succeeded;
            
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
    }


}
