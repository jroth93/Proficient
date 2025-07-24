using Autodesk.Revit.UI.Selection;
using Proficient.Utilities;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class ToggleUpDn : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var selIds = uiDoc.Selection.GetElementIds();

        if (selIds is not null && selIds.Any())
        {
            foreach (var elId in selIds)
            {
                using Transaction tx = new(doc, "Toggle Up/Down");
                if (tx.Start() != TransactionStatus.Started)
                    return Result.Failed;
                try
                {
                    var par = doc.GetElement(elId).LookupParameter(Names.Parameter.FittingUpDn);
                    par.Set(Math.Abs(par.AsInteger() - 1));
                }
                catch (NullReferenceException)
                {
                    continue;
                }

                tx.Commit();
            }
            return Result.Succeeded;
        }
        while (true)
        {
            using Transaction tx = new(doc, "Toggle Up/Down");
            try
            {
                if (tx.Start() != TransactionStatus.Started)
                    return Result.Failed;
                
                var par = doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element)).LookupParameter(Names.Parameter.FittingUpDn);
                par.Set(Math.Abs(par.AsInteger() - 1));
                
                tx.Commit();
            }
            catch (NullReferenceException)
            {
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Succeeded;
            }
        }
    }
}