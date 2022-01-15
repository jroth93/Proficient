using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using System.Linq;

namespace Proficient.Mech
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class HardBreak : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            UIApplication app = revit.Application;
            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            var sel = uidoc.Selection.GetElementIds();
            ElementId eId = sel.Count() > 0 ? sel.First() : uidoc.Selection.PickObject(ObjectType.Element, "Pick Element").ElementId;
            XYZ pt = uidoc.Selection.PickPoint();
            
            using (Transaction tx = new Transaction(doc, "hardbreak"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    Element el = doc.GetElement(eId);

                    if((el as Pipe) != null)
                    {
                        PlumbingUtils.BreakCurve(doc, eId, pt);
                    }
                    else if(el as Duct != null)
                    {
                        MechanicalUtils.BreakCurve(doc, eId, pt);
                    }
                }

                tx.Commit();
            }



            return Result.Succeeded;
        }
    }


}
