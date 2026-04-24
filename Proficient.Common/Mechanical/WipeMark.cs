using Autodesk.Revit.UI.Selection;

namespace Proficient.Mechanical;

[Transaction(TransactionMode.Manual)]
internal class WipeMark : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var selIds = uiDoc.Selection.GetElementIds();

        if (selIds is not null && selIds.Count > 0)
        {
            using Transaction tx = new (doc, "Wipe Mark");
            if (tx.Start() != TransactionStatus.Started)
                return Result.Failed;
            foreach (var elId in selIds)
                doc.GetElement(elId).get_Parameter(BuiltInParameter.ALL_MODEL_MARK)?.Set(string.Empty);
            tx.Commit();

            return Result.Succeeded;
        }

        while (true)
        {
            using Transaction tx = new(doc, "Wipe Mark");
            try
            {
                if (tx.Start() != TransactionStatus.Started)
                    return Result.Failed;

                doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element))
                    .get_Parameter(BuiltInParameter.ALL_MODEL_MARK)?.Set(string.Empty);
                
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