using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;

namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ToggleUpDn : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var elIds = uidoc.Selection.GetElementIds();

            if (elIds.Count > 0)
            {
                foreach (ElementId elId in elIds)
                {
                    using (Transaction tx = new Transaction(doc, "clearupdn"))
                    {
                        if (tx.Start() == TransactionStatus.Started)
                        {
                            try
                            {
                                Parameter par = doc.GetElement(elId).LookupParameter(Names.Parameter.FittingUpDn);
                                par.Set(Math.Abs(par.AsInteger() - 1));
                            }
                            catch (NullReferenceException)
                            {
                                continue;
                            }
                        }
                        tx.Commit();
                    }
                }
                return Result.Succeeded;
            }
            while (true)
            {
                using (Transaction tx = new Transaction(doc, "clearupdn"))
                {
                    try { 
                        if (tx.Start() == TransactionStatus.Started)
                        {
                            Parameter par = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element)).LookupParameter(Names.Parameter.FittingUpDn);
                            par.Set(Math.Abs(par.AsInteger() - 1));
                        }
                        tx.Commit();
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return Result.Succeeded;
                    }
                }
                
            }
        }
    }
}
