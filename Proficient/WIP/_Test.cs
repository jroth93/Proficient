using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
//using System.Windows.Controls;
//using System.Windows.Input;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using System.Windows;
using System.Windows.Automation;
//using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Proficient.Forms;
using Proficient.Utilities;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Windows.Interop;

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
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;
            //app.GetDockablePane(Main.PaneId).Hide();
            //ElementId eid = uidoc.Selection.GetElementIds().First();
            //Element el = doc.GetElement(eid);
            var tags = new FilteredElementCollector(doc, view.Id).OfClass(typeof(IndependentTag)).ToElementIds();
            uidoc.Selection.SetElementIds(tags);


            /*
            HwndSource hwndSource = HwndSource.FromHwnd(app.MainWindowHandle);
            string version = (hwndSource.RootVisual as Window).Title.Split(' ')[2];
            TaskDialog.Show(version, version);*/

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
