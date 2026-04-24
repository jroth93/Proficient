namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class OpenSectionView : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var selectedIds = uiDoc.Selection.GetElementIds();

        foreach (var id in selectedIds)
        {
#if PRE24
            var idView = new ElementId(id.IntegerValue + 1);
#else
            var idView = new ElementId(id.Value + 1);
#endif
            if (doc.GetElement(idView) is not View vw) 
                continue;
            uiDoc.RequestViewChange(vw);
            break;
        }

        return Result.Succeeded;
    }
}