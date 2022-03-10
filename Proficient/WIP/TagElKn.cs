using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TagElKn : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = revit.Application.ActiveUIDocument.Document;
            ElementId viewid = uidoc.ActiveView.Id;
            View view = doc.GetElement(viewid) as View;

            FilteredElementCollector coll = new FilteredElementCollector(doc).WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
            ElementId knfamid = coll.Where(el => (el as FamilySymbol).FamilyName.Contains("MEI Keynote Tag")).First().Id as ElementId;
            var fsel = coll
                .Select(el => el.LookupParameter("Keynote"))
                .Where(el => el != null && el.HasValue == true)
                .Select(el => el.Element);

            FilteredElementCollector elcoll = GetMEPElements(doc);

            using (Transaction tx = new Transaction(doc, "Add Element Keynotes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (Element el in elcoll)
                    {
                        foreach (Element fel in fsel)
                        {
                            if (el.GetTypeId() == fel.Id)
                            {
                                Reference newref = new Reference(el);
                                IndependentTag newkn = IndependentTag.Create(doc, knfamid, viewid, newref, true, TagOrientation.Horizontal, (el.Location as LocationPoint).Point);
                                newkn.get_Parameter(BuiltInParameter.KEY_VALUE).Set(fel.LookupParameter("Keynote").AsString());
                                newkn.LeaderEndCondition = LeaderEndCondition.Attached;
                                newkn.TagHeadPosition = (el.Location as LocationPoint).Point + new XYZ(4, 2, 0);
#if R22
                                newkn.SetLeaderElbow(newref, newkn.TagHeadPosition + new XYZ(-2, -0.04, 0));
#else
                                newkn.LeaderElbow = newkn.TagHeadPosition + new XYZ(-2, -0.04, 0);
#endif
                            }
                        }
                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }

        static FilteredElementCollector GetMEPElements(Document doc)
        {

            BuiltInCategory[] bics = new BuiltInCategory[] {
                BuiltInCategory.OST_CableTray,
                BuiltInCategory.OST_CableTrayFitting,
                BuiltInCategory.OST_Conduit,
                BuiltInCategory.OST_ConduitFitting,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_ElectricalEquipment,
                BuiltInCategory.OST_ElectricalFixtures,
                BuiltInCategory.OST_LightingDevices,
                BuiltInCategory.OST_LightingFixtures,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_SpecialityEquipment,
                BuiltInCategory.OST_Sprinklers,
                BuiltInCategory.OST_Wire,
              };

            IList<ElementFilter> a
              = new List<ElementFilter>(bics.Count());

            foreach (BuiltInCategory bic in bics)
            {
                a.Add(new ElementCategoryFilter(bic));
            }

            LogicalOrFilter categoryFilter
              = new LogicalOrFilter(a);

            LogicalAndFilter familyInstanceFilter
              = new LogicalAndFilter(categoryFilter, new ElementClassFilter(typeof(FamilyInstance)));

            IList<ElementFilter> b
              = new List<ElementFilter>(6);

            b.Add(familyInstanceFilter);

            LogicalOrFilter classFilter
              = new LogicalOrFilter(b);

            FilteredElementCollector collector
              = new FilteredElementCollector(doc);

            collector.WherePasses(classFilter);

            return collector;
        }

    }
}
