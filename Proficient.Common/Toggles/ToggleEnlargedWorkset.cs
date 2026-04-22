namespace Proficient.Toggles;

[Transaction(TransactionMode.Manual)]
internal class ToggleEnlargedWorkset : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var uiDoc = revit.Application.ActiveUIDocument;
        var doc = uiDoc.Document;
        var view = uiDoc.ActiveView;

        if (view.ViewTemplateId != ElementId.InvalidElementId)
            view = doc.GetElement(view.ViewTemplateId) as View;

        if (view is null || Main.Settings is null) return Result.Failed;

        var wsList = new FilteredWorksetCollector(doc).ToWorksets();

        var enlWs = Main.Settings.DefWorkset[0] == 'M' ? 
            wsList.FirstOrDefault(ws => ws.Name.Contains("M-Enlarged")) : 
            wsList.FirstOrDefault(ws => ws.Name.Contains("E-Enlarged"));

        using Transaction tx = new (doc, "Toggle Enlarged Workset");
        if (tx.Start() != TransactionStatus.Started)
            return Result.Failed;

        if (enlWs != null)
        {
            var isHidden = view.GetWorksetVisibility(enlWs.Id) == WorksetVisibility.UseGlobalSetting;
            var alert = isHidden ? "on" : "off";
            view.SetWorksetVisibility(enlWs.Id,
                isHidden 
                ? WorksetVisibility.Visible
                : WorksetVisibility.UseGlobalSetting);
            Utilities.Util.BalloonTip("Enlarged Workset", $"Enlarged workset is {alert}.","");
        }
            
        tx.Commit();

        return Result.Succeeded;
    }
}