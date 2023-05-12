using Autodesk.Revit.UI.Selection;

namespace Proficient.Toggles;

[Transaction(TransactionMode.Manual)]
internal class FlipWorkPlane : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var selIds = uiDoc.Selection.GetElementIds();
        if (selIds is not null && selIds.Any())
        {
            var fis = selIds
                .Select(doc.GetElement)
                .Where(el => el is FamilyInstance {CanFlipWorkPlane: true})
                .Cast<FamilyInstance>();

            foreach (var fi in fis)
            {
                using Transaction tx = new(doc, "Flip Workplane");
                if (tx.Start() == TransactionStatus.Started)
                    fi.IsWorkPlaneFlipped = !fi.IsWorkPlaneFlipped;
                tx.Commit();
            }
            return Result.Succeeded;
        }
        while (true)
        {
            try
            {
                var id = uiDoc.Selection.PickObject(ObjectType.Element).ElementId;
                if (doc.GetElement(id) is not FamilyInstance {CanFlipWorkPlane: true} fi) 
                    continue;

                using Transaction tx = new(doc, "Flip Workplane");
                if (tx.Start() == TransactionStatus.Started)
                    fi.IsWorkPlaneFlipped = !fi.IsWorkPlaneFlipped;
                tx.Commit();
            }
            catch (NullReferenceException) {}
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
        }
    }
}