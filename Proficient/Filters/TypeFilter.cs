using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Proficient.Forms;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TypeFilter : IExternalCommand
    {
        public static Document doc;
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            if(selectedIds.Count == 0)
            {
                return Result.Succeeded;
            }

            TypeEqualityComparer tec = new TypeEqualityComparer();

            IEnumerable<Element> types = selectedIds
                .Select(id => doc.GetElement(id))
                .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
                .Distinct(tec)
                .OrderBy(el => (doc.GetElement(el.GetTypeId()) as ElementType).FamilyName)
                .ThenBy(el => el.Name);

            Blank frm = new Blank();
            frm.sp.Orientation = Orientation.Vertical;

            frm.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Rectangle mwe = revit.Application.MainWindowExtents;
                frm.Left = (mwe.Left + mwe.Right) / 2 - frm.Width / 2;
                frm.Top = (mwe.Top + mwe.Bottom) / 2 - frm.Height / 2;
            };

            List<Button> btnList = new List<Button>();
            string selectedFamily = string.Empty;
            string selectedType = string.Empty;

            foreach (Element type in types)
            {
                string fam = (doc.GetElement(type.GetTypeId()) as ElementType).FamilyName;
                string display = fam + " - " + type.Name;
                btnList.Add(new Button { Content = display, Margin = new Thickness(0, 3, 0, 0) });
                frm.sp.Children.Add(btnList.Last());

                btnList.Last().Click += (object sender, RoutedEventArgs e) =>
                {
                    frm.DialogResult = true;
                    selectedFamily = fam;
                    selectedType = type.Name;
                    frm.Close();
                };
            }

            frm.ShowDialog();

            ICollection<ElementId> filteredIds = selectedIds
                .Select(id => doc.GetElement(id))
                .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
                .Where(el => el.Name == selectedType && (doc.GetElement(el.GetTypeId()) as ElementType).FamilyName == selectedFamily)
                .Select(el => el.Id)
                .ToList();

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

    internal class TypeEqualityComparer : IEqualityComparer<Element>
    {
        public bool Equals(Element e1, Element e2)
        {
            string f1 = (TypeFilter.doc.GetElement(e1.GetTypeId()) as ElementType).FamilyName;
            string f2 = (TypeFilter.doc.GetElement(e2.GetTypeId()) as ElementType).FamilyName;

            if (e1 == null && e2 == null)
                return true;
            else if (e1 == null || e2 == null)
                return false;
            else if (e1.Name == e2.Name && f1 == f2)
                return true;
            else
                return false;
        }

        public int GetHashCode(Element e)
        {
            int hCode = TypeFilter.doc.GetElement(e.Id).GetTypeId().IntegerValue;
            return hCode.GetHashCode();
        }
    }
}
