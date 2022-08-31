using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class SetFittingUpDn : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = uidoc.ActiveView;

            var selEls = 
                uidoc.Selection
                .GetElementIds()?
                .Select(id => doc.GetElement(id));

            IEnumerable<Element> fts = new List<Element>();
            if(selEls != null)
            {
                fts = selEls
                    .Where(el => el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting ||
                    el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting);
            }
            else
            {
                var df =
                new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_DuctFitting)
                .ToElements();
                var pf =
                    new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_PipeFitting)
                    .ToElements();
                fts = df.Concat(pf).Where(x => (x as FamilyInstance) != null);
            }

            using (Transaction tx = new Transaction(doc, "Set up/dn parameter"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (FamilyInstance fi in fts)
                    {
                        List<PartType> noSet = new List<PartType>() { PartType.Transition, PartType.Cap, PartType.TapAdjustable, PartType.TapPerpendicular };

                        MechanicalFitting mf = fi.MEPModel as MechanicalFitting;
                        if (mf.ConnectorManager != null && !noSet.Contains(mf.PartType))
                        {
                            foreach (Connector c in mf.ConnectorManager.Connectors)
                            {
                                Parameter fudPar = fi.LookupParameter(Names.Parameter.FittingUpDn);
                                if (c.CoordinateSystem.BasisZ.Z == 1 && fudPar != null)
                                {
                                    fudPar.Set(1);
                                }
                                else if (c.CoordinateSystem.BasisZ.Z == -1 && fudPar != null)
                                {
                                    fudPar.Set(0);
                                }
                            }
                        }
                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}
