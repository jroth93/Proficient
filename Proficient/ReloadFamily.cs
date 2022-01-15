using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ReloadFamily : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            foreach (ElementId eId in revit.Application.ActiveUIDocument.Selection.GetElementIds())

            {
                FamilySymbol famSymb = doc.GetElement(doc.GetElement(eId).GetTypeId()) as FamilySymbol;

                string famPath = doc.EditFamily(famSymb.Family).PathName;

                using (Transaction tx = new Transaction(doc, "Reload Family"))
                {
                    if (tx.Start() == TransactionStatus.Started)
                    {
                        bool success = doc.LoadFamily(famPath, new FamilyOption(), out Family fam);
                    }

                    tx.Commit();
                }
            }
            return Result.Succeeded;
        }
    }

    class FamilyOption : IFamilyLoadOptions
    {
        public bool OnFamilyFound(
          bool familyInUse,
          out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            return true;
        }

        public bool OnSharedFamilyFound(
          Family sharedFamily,
          bool familyInUse,
          out FamilySource source,
          out bool overwriteParameterValues)
        {
            overwriteParameterValues = true;
            source = FamilySource.Family;
            return true;
        }
    }
}
