using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Proficient.Forms;
using Proficient.Utilities;
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

            BlankViewModel bvm = new BlankViewModel();
            System.Windows.Point mousePos = Mouse.GetCursorPosition();
            bvm.SetLocation(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y));
            string selectedCat = string.Empty;

            foreach (Category cat in cats)
            {
                bvm.AddButton("smallBtn", cat.Name, () => selectedCat = cat.Name, true, true);
            }

            if (!bvm.ShowWindow(true) ?? false)
            {
                return Result.Cancelled;
            }

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
