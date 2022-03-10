using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Proficient.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class CategoryFilter : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            if (selectedIds.Count == 0)
            {
                return Result.Succeeded;
            }

            CategoryEqualityComparer cec = new CategoryEqualityComparer();

            IEnumerable<Category> cats = selectedIds
                .Select(id => doc.GetElement(id))
                .Select(el => el.Category)
                .Distinct(cec)
                .OrderBy(cat => cat.Name);

            Blank frm = new Blank();
            frm.sp.Orientation = Orientation.Vertical;

            frm.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Rectangle mwe = revit.Application.MainWindowExtents;
                frm.Left = (mwe.Left + mwe.Right) / 2 - frm.Width / 2;
                frm.Top = (mwe.Top + mwe.Bottom) / 2 - frm.Height / 2;
            };

            List<Button> btnList = new List<Button>();
            string selectedCat = string.Empty;
            Style s = frm.FindResource("smallBtn") as Style;
            foreach (Category cat in cats)
            {
                btnList.Add(new Button { Content = cat.Name, Style = s });
                frm.sp.Children.Add(btnList.Last());

                btnList.Last().Click += (object sender, RoutedEventArgs e) =>
                {
                    frm.DialogResult = true;
                    selectedCat = Convert.ToString((sender as Button).Content);
                    frm.Close();
                };
            }

            frm.ShowDialog();

            ICollection<ElementId> filteredIds = selectedIds.Where(id => doc.GetElement(id).Category.Name == selectedCat).ToList();

            using (Transaction tx = new Transaction(doc, "Quick Filter"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    uidoc.Selection.SetElementIds(filteredIds);
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }

    internal class CategoryEqualityComparer : IEqualityComparer<Category>
    {
        public bool Equals(Category c1, Category c2)
        {
            if (c1 == null && c2 == null)
                return true;
            else if (c1 == null || c2 == null)
                return false;
            else if (c1.Name == c2.Name)
                return true;
            else
                return false;
        }

        public int GetHashCode(Category c)
        {
            int hCode = c.Id.IntegerValue;
            return hCode.GetHashCode();
        }
    }
}
