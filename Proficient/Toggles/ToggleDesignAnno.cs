using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ToggleDesignAnno : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {

            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            bool curState = GetCurrentState(doc, view);

            var textEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TextNotes)
                .Where(x => x.Name.ToLower().Contains("design"))
                .Where(el => el.OwnerViewId == view.Id || el.OwnerViewId == view.GetPrimaryViewId())
                .Select(el => el.Id);

            var lineEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Lines)
                .Where(x => (x as CurveElement).LineStyle.Name.ToLower().Contains("design"))
                .Where(el => el.OwnerViewId == view.Id || el.OwnerViewId == view.GetPrimaryViewId())
                .Select(el => el.Id);

            var dimEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Dimensions)
                .Where(x => x.Name.ToLower().Contains("design"))
                .Where(el => el.OwnerViewId == view.Id || el.OwnerViewId == view.GetPrimaryViewId())
                .Select(el => el.Id);

            List<ElementId> designElIds = new List<ElementId>();

            designElIds.AddRange(textEl);
            designElIds.AddRange(lineEl);
            designElIds.AddRange(dimEl);

            if (designElIds.Count == 0)
            {
                return Result.Succeeded;
            }

            using (Transaction tx = new Transaction(doc, "Toggle Design Note Visibility"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    if (curState)
                    {
                        view.HideElements(designElIds);
                    }
                    else
                    {
                        view.UnhideElements(designElIds);
                    }

                }

                tx.Commit();
            }

            return Result.Succeeded;
        }

        private bool GetCurrentState(Document doc, View view)
        {
            bool curState = false;

            using (Transaction tx = new Transaction(doc, "Set Visibility Field"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    Schema viewSchema = Schema.Lookup(Names.Guids.ViewSchema);

                    if (viewSchema == null)
                    {
                        SchemaBuilder sb = new SchemaBuilder(Names.Guids.ViewSchema);
                        sb.SetReadAccessLevel(AccessLevel.Public);
                        sb.SetWriteAccessLevel(AccessLevel.Public);

                        sb.SetSchemaName("ViewSchema");

                        FieldBuilder fb = sb.AddSimpleField("DesignNoteVisibility", typeof(bool));
                        fb.SetDocumentation("Denotes current visibility of design notes");

                        viewSchema = sb.Finish();
                    }

                    Entity ent = view.GetEntity(viewSchema);
                    Field field = viewSchema.GetField("DesignNoteVisibility");

                    if (ent.Schema == null)
                    {
                        ent = new Entity(viewSchema);
                        curState = true;
                    }
                    else
                    {
                        curState = ent.Get<bool>(field);
                    }


                    ent.Set(field, !curState);
                    view.SetEntity(ent);
                }

                tx.Commit();
            }

            return curState;
        }
    }


}
