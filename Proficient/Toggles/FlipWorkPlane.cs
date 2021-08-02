using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class FlipWorkPlane : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<ElementId> selectedIds = uidoc.Selection.GetElementIds() as IList<ElementId>;
            if (selectedIds.Count > 0)
            {

                foreach (ElementId elemid in selectedIds)
                {
                    using (Transaction tx = new Transaction(doc, "Flip"))
                    {
                        if (tx.Start() == TransactionStatus.Started)
                        {
                            FamilyInstance faminst = doc.GetElement(elemid) as FamilyInstance;

                            if (faminst.IsWorkPlaneFlipped == true)
                            {
                                faminst.IsWorkPlaneFlipped = false;
                            }
                            else
                            {
                                faminst.IsWorkPlaneFlipped = true;
                            }

                        }
                        tx.Commit();
                    }
                }
                return Autodesk.Revit.UI.Result.Succeeded;
            }
            while (true)
            {
                Reference reference = null;
                FamilyInstance faminst = null;
                try
                {
                    reference = uidoc.Selection.PickObject(ObjectType.Element);
                    faminst = doc.GetElement(reference) as FamilyInstance;
                    using (Transaction tx = new Transaction(doc, "Flip"))
                    {
                        if (tx.Start() == TransactionStatus.Started)
                        {
                            if (faminst.IsWorkPlaneFlipped == true)
                            {
                                faminst.IsWorkPlaneFlipped = false;
                            }
                            else
                            {
                                faminst.IsWorkPlaneFlipped = true;
                            }
                        }
                        tx.Commit();
                    }
                }
                catch (NullReferenceException)
                {
                    continue;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Autodesk.Revit.UI.Result.Succeeded;
                }
            }
        }
    }
}