using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class OpenSectionView : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;
            KeynoteTable knt = KeynoteTable.GetKeynoteTable(doc);
            KeyBasedTreeEntries kbte = knt.GetKeyBasedTreeEntries();
            IList<ElementId> selectedIds = uidoc.Selection.GetElementIds() as IList<ElementId>;

            foreach (ElementId id in selectedIds)
            {
                ElementId idView = new ElementId(id.IntegerValue + 1);
                View vw = doc.GetElement(idView) as View;
                if (vw != null)
                {
                    uidoc.RequestViewChange(vw);
                }
            }

            return Result.Succeeded;
        }
    }
}
