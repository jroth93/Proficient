using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.ExtensibleStorage;
using Proficient.Forms;


namespace Proficient.Utilities
{
    public class NotesHandler : IExternalEventHandler
    {
        private NotesRequest m_request = new NotesRequest();
        public NotesRequest Request
        {
            get { return m_request; }
        }
        public string GetName()
        {
            return "Notes Request Handler";
        }

        public void Execute(UIApplication uiapp)
        {
            NotesPane.NotesTab tab = m_request.Take();

            switch (tab)
            {
                case NotesPane.NotesTab.View:
                    SaveViewNotes(uiapp);
                    break;
                case NotesPane.NotesTab.Project:
                    break;
            }

            
        }

        public void SaveViewNotes(UIApplication uiapp)
        {
            using (Transaction tx = new Transaction(uiapp.ActiveUIDocument.Document, "Set View Notes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
                    Entity ent = NotesPane.Pane.currentView.GetEntity(pSchema);
                    IDictionary<string, string> stringDict = new Dictionary<string, string>();

                    if (ent.Schema == null)
                    {
                        ent = new Entity(pSchema);
                    }
                    else
                    {
                        stringDict = ent.Get<IDictionary<string, string>>(ESKeys.StringDict);
                    }


                    stringDict[ESKeys.MarkdownText] = NotesPane.Pane.MarkdownCache;
                    NotesPane.Pane.MarkdownCache = string.Empty;

                    ent.Set("StringDict", stringDict);

                    NotesPane.Pane.currentView.SetEntity(ent);
                }

                tx.Commit();
            }
        }
    }
}
