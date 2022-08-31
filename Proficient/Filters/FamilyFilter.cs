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
    class FamilyFilter : IExternalCommand
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

            IEnumerable<string> fams = selectedIds
                .Select(id => doc.GetElement(id))
                .Where(el => el.GetTypeId() != null && el.GetTypeId() != ElementId.InvalidElementId)
                .Select(el => (doc.GetElement(el.GetTypeId()) as ElementType).FamilyName)
                .Distinct()
                .OrderBy(fam => fam);

            BlankViewModel bvm = new BlankViewModel();

            System.Windows.Point mousePos = Mouse.GetCursorPosition();
            bvm.SetLocation(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y));
            string selectedFam = string.Empty;

            foreach (string fam in fams)
            {
                bvm.AddButton("smallBtn", fam, () => selectedFam = fam, true, true);
            }

            if (!bvm.ShowWindow(true) ?? false)
            {
                return Result.Cancelled;
            }


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
