using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Proficient.Forms;
using Proficient.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            if (selectedIds.Count == 0)
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

            BlankViewModel bvm = new BlankViewModel();

            System.Windows.Point mousePos = Mouse.GetCursorPosition();
            bvm.SetLocation(Convert.ToInt32(mousePos.X), Convert.ToInt32(mousePos.Y));
            string selectedFamily = string.Empty;
            string selectedType = string.Empty;

            foreach (Element type in types)
            {
                string fam = (doc.GetElement(type.GetTypeId()) as ElementType).FamilyName;
                string display = fam + " - " + type.Name;
                bvm.AddButton("smallBtn", display, () => { selectedFamily = fam; selectedType = type.Name; }, true, true);
            }

            if (!bvm.ShowWindow(true) ?? false)
            {
                return Result.Cancelled;
            }


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
