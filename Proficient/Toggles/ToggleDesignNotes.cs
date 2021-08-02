using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ToggleDesignNotes : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {

            UIDocument uidoc = revit.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.GetElement(uidoc.ActiveView.Id) as View;

            bool curState = GetCurrentState(doc, view);

            var designNotes = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TextNotes)
                .Where(el => el.Name.Contains("Design Notes"))
                .Where(el => el.OwnerViewId == view.Id || el.OwnerViewId == view.GetPrimaryViewId())
                .Select(el => el.Id).ToList();

            if(designNotes.Count == 0)
            {
                return Result.Succeeded;
            }

            using (Transaction tx = new Transaction(doc, "Toggle Design Note Visibility"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    if (curState)
                    {
                        view.HideElements(designNotes);
                    }
                    else
                    {
                        view.UnhideElements(designNotes);
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

                    if (ent.Schema == null)
                    {
                        ent = new Entity(viewSchema);
                    }

                    Field field = viewSchema.GetField("DesignNoteVisibility");

                    try
                    {
                        curState = ent.Get<bool>(field);
                    }
                    catch
                    {
                        curState = false;
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
