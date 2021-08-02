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
    class FamilyFilter : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            if(selectedIds.Count == 0)
            {
                return Result.Succeeded;
            }

            IEnumerable<string> fams = selectedIds
                .Select(id => doc.GetElement(id))
                .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
                .Select(el => (doc.GetElement(el.GetTypeId()) as ElementType).FamilyName)
                .Distinct()
                .OrderBy(fam => fam);

            Blank frm = new Blank();
            frm.sp.Orientation = Orientation.Vertical;

            frm.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Rectangle mwe = revit.Application.MainWindowExtents;
                frm.Left = (mwe.Left + mwe.Right) / 2 - frm.Width / 2;
                frm.Top = (mwe.Top + mwe.Bottom) / 2 - frm.Height / 2;
            };

            List<Button> btnList = new List<Button>();
            string selectedFam = string.Empty;

            foreach (string fam in fams)
            {
                btnList.Add(new Button { Content = fam, Margin = new Thickness(0, 3, 0, 0) });
                frm.sp.Children.Add(btnList.Last());

                btnList.Last().Click += (object sender, RoutedEventArgs e) =>
                {
                    frm.DialogResult = true;
                    selectedFam = Convert.ToString((sender as Button).Content);
                    frm.Close();
                };
            }

            frm.ShowDialog();

            ICollection<ElementId> filteredIds = selectedIds
                .Select(id => doc.GetElement(id))
                .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
                .Where(el => (doc.GetElement(el.GetTypeId()) as ElementType).FamilyName == selectedFam)
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

}
