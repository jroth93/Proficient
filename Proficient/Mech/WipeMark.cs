using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Proficient.Forms;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class WipeMark : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            var elIds = uidoc.Selection
                .GetElementIds()
                .Where(id => doc.GetElement(id).get_Parameter(BuiltInParameter.ALL_MODEL_MARK) != null);

            if (!elIds.Any())
            {
                return Result.Succeeded;
            }

            Blank frm = new Blank();
            frm.sp.Orientation = Orientation.Horizontal;
            Style s = frm.FindResource("smallBtn") as Style;


            Button btnTm = new Button
            {
                Content = "Type Mark",
                Style = s
            };
            btnTm.Content = "Type Mark";
            frm.sp.Children.Add(btnTm);

            Label spacer = new Label();
            spacer.Content = " ";
            frm.sp.Children.Add(spacer);

            Button btnM = new Button
            {
                Content = "Mark",
                Style = s
            };
            btnM.Content = "Mark";
            frm.sp.Children.Add(btnM);

            bool tm = true;

            frm.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Rectangle mwe = revit.Application.MainWindowExtents;
                frm.Left = (mwe.Left + mwe.Right) / 2 - frm.Width / 2;
                frm.Top = (mwe.Top + mwe.Bottom) / 2 - frm.Height / 2;
            };

            btnTm.Click += (object sender, RoutedEventArgs e) =>
            {
                frm.DialogResult = true;
                tm = true;
                frm.Close();
            };

            btnM.Click += (object sender, RoutedEventArgs e) =>
            {
                frm.DialogResult = true;
                tm = false;
                frm.Close();
            };

            if (!frm.ShowDialog() ?? false)
            {
                return Result.Cancelled;
            }

            using (Transaction tx = new Transaction(doc, "Wipe"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (ElementId elId in elIds)
                    {
                        if (tm)
                        {
                            Element el = doc.GetElement(doc.GetElement(elId).GetTypeId());
                            el.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).Set(string.Empty);
                        }
                        else
                        {
                            Element el = doc.GetElement(elId);
                            el.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(string.Empty);
                        }


                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
