using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ToggleEnlWkst : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = revit.Application.ActiveUIDocument.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            if (view.ViewTemplateId != ElementId.InvalidElementId)
                view = doc.GetElement(view.ViewTemplateId) as View;

            var wsList = new FilteredWorksetCollector(doc).ToWorksets();
            Workset enlWs;

            if (Main.Settings.defWorkset[0] == 'M')
                enlWs = wsList.Where(ws => ws.Name.Contains("M-Enlarged")).FirstOrDefault();
            else
                enlWs = wsList.Where(ws => ws.Name.Contains("E-Enlarged")).FirstOrDefault();

            using (Transaction tx = new Transaction(doc, "Toggle Enlarged Workset"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    if (view.GetWorksetVisibility(enlWs.Id) == WorksetVisibility.UseGlobalSetting)
                        view.SetWorksetVisibility(enlWs.Id, WorksetVisibility.Visible);
                    else
                        view.SetWorksetVisibility(enlWs.Id, WorksetVisibility.UseGlobalSetting);
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
