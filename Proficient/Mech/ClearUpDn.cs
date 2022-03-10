using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Linq;
using System;

namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ClearUpDn : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
#if R19
            return Result.Failed;
#else
            UIApplication uiapp = revit.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var elIds = uidoc.Selection.GetElementIds();
            if (elIds.Count > 0)
            {
                
                foreach (ElementId elId in elIds)
                {
                    Element el = null;
                    try
                    {
                        el = doc.GetElement(elId);
                        using (Transaction tx = new Transaction(doc, "clearupdn"))
                        {
                            if (tx.Start() == TransactionStatus.Started)
                            {

                                el.LookupParameter(Names.Parameter.FittingUpDn).ClearValue();
                                tx.Commit();
                            }
                        }
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return Result.Succeeded;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        ReplaceOldSP(uiapp, elId);
                    }
                }
                return Result.Succeeded;
            }
            while (true)
            {
                Element el = null;     
                try
                {
                    el = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element));
                    using (Transaction tx = new Transaction(doc, "clearupdn"))
                    {
                        if (tx.Start() == TransactionStatus.Started)
                        {

                            el.LookupParameter(Names.Parameter.FittingUpDn).ClearValue();
                            tx.Commit();
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    continue;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                {
                    ReplaceOldSP(uiapp, el.Id);
                }         
            }
#endif
        }

        private static void ReplaceOldSP(UIApplication uiapp, ElementId elId)
        {
            Document doc = uiapp.ActiveUIDocument.Document;
            Definition def = doc.GetElement(elId).LookupParameter(Names.Parameter.FittingUpDn).Definition;

            CategorySet cset = uiapp.Application.Create.NewCategorySet();
            cset.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_DuctFitting));
            cset.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_DuctCurves));
            cset.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeCurves));
            cset.Insert(doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeFitting));

            var spFec = new FilteredElementCollector(doc).OfClass(typeof(SharedParameterElement)).ToElements();
            SharedParameterElement spUpDn = spFec.Where(sp => (sp as SharedParameterElement).GetDefinition().Name.Equals(Names.Parameter.FittingUpDn)).First() as SharedParameterElement;

            using (Transaction tx = new Transaction(doc, "remove old updn par"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    doc.ParameterBindings.Remove(def);
                    doc.Delete(spUpDn.Id);
                    
                }
                tx.Commit();
            }

            Util.AddSharedParameter(doc, uiapp, cset, BuiltInParameterGroup.PG_GRAPHICS, "Display Controllers", Names.Parameter.FittingUpDn);


        }
    }
}
