using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System;
using Proficient.Utilities;

namespace Proficient.Archive
{
    [Transaction(TransactionMode.Manual)]
    internal class ClearUpDn : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
#if R19
            return Result.Failed;
#else
            var uiApp = revit.Application;
            var uiDoc = uiApp.ActiveUIDocument;
            var doc = uiDoc.Document;

            var elIds = uiDoc.Selection.GetElementIds();
            if (elIds.Any())
            {
                foreach (var elId in elIds)
                {
                    try
                    {
                        var el = doc.GetElement(elId);
                        using var tx = new Transaction(doc, "Clear Up/Dn");
                        if (tx.Start() != TransactionStatus.Started) 
                            continue;
                        el.LookupParameter(Names.Parameter.FittingUpDn).ClearValue();
                        tx.Commit();
                    }
                    catch (NullReferenceException) { }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return Result.Cancelled;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        ReplaceOldSp(uiApp, elId);
                    }
                }
                return Result.Succeeded;
            }
            while (true)
            {
                Element el = null;     
                try
                {
                    el = doc.GetElement(uiDoc.Selection.PickObject(ObjectType.Element));
                    using var tx = new Transaction(doc, "Clear Up/Dn");
                    if (tx.Start() != TransactionStatus.Started)
                        return Result.Failed;
                    el.LookupParameter(Names.Parameter.FittingUpDn).ClearValue();
                    tx.Commit();
                }
                catch (NullReferenceException){ }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    if(el != null)
                        ReplaceOldSp(uiApp, el.Id);
                }         
            }
#endif
        }

        private static void ReplaceOldSp(UIApplication uiApp, ElementId elId)
        {
            var doc = uiApp.ActiveUIDocument.Document;
            var def = doc.GetElement(elId).LookupParameter(Names.Parameter.FittingUpDn).Definition;

            var cs = uiApp.Application.Create.NewCategorySet();
            cs.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_DuctFitting));
            cs.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_DuctCurves));
            cs.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeCurves));
            cs.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeFitting));

            var spUpDn = new FilteredElementCollector(doc)
                .OfClass(typeof(SharedParameterElement))
                .Cast<SharedParameterElement>()
                .First(sp => sp.GetDefinition().Name.Equals(Names.Parameter.FittingUpDn));

            using var tx = new Transaction(doc, "Remove old up/dn parameter");
            if (tx.Start() != TransactionStatus.Started)
                return;
            doc.ParameterBindings.Remove(def);
            doc.Delete(spUpDn.Id);
            tx.Commit();

            Util.AddSharedParameter(doc, uiApp, cs, BuiltInParameterGroup.PG_GRAPHICS, "Display Controllers", Names.Parameter.FittingUpDn);


        }
    }
}
