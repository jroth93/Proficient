using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Controls;
using System.Windows.Input;

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



            using (Transaction tx = new Transaction(doc, "commandname"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {

                }

                tx.Commit();
            }



            return Result.Succeeded;
        }
    }


}
