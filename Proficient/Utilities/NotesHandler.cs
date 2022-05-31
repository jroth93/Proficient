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
            NotesType tab = m_request.Take();

            switch (tab)
            {
                case NotesType.View:
                    SaveViewNotes(uiapp);
                    break;
                case NotesType.Project:
                    SaveProjectNotes(uiapp);
                    break;
                case NotesType.Global:
                    SaveDbId(uiapp);
                    break;
            }

            
        }

        public void SaveViewNotes(UIApplication uiapp)
        {
            using (Transaction tx = new Transaction(uiapp.ActiveUIDocument.Document, "Set View Notes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    View view = NotesModel.NM.CurrentView;
                    Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
                    Entity ent = view.GetEntity(pSchema);
                    IDictionary<string, string> stringDict = new Dictionary<string, string>();

                    if (ent.Schema == null)
                    {
                        ent = new Entity(pSchema);
                    }
                    else
                    {
                        stringDict = ent.Get<IDictionary<string, string>>(ESKeys.StringDict);
                    }


                    stringDict[ESKeys.MarkdownText] = NotesModel.NM.MarkdownCache;
                    NotesModel.NM.MarkdownCache = string.Empty;

                    ent.Set(ESKeys.StringDict, stringDict);

                    view.SetEntity(ent);
                }

                tx.Commit();
            }
        }

        public void SaveProjectNotes(UIApplication uiapp)
        {
            using (Transaction tx = new Transaction(uiapp.ActiveUIDocument.Document, "Set Project Notes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    Document doc = uiapp.ActiveUIDocument.Document;

                    var projectStorage =
                        new FilteredElementCollector(doc)
                        .OfClass(typeof(DataStorage))
                        .FirstElement();

                    Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
                    Entity entity = new Entity(pSchema);

                    if (projectStorage == null)
                    {
                        projectStorage = DataStorage.Create(doc);
                    }
                    else
                    {
                        Entity e = projectStorage.GetEntity(pSchema);
                        if (e.IsValid())
                        {
                            entity = e;
                        }
                    }


                    // Create entity which store created info

                    IDictionary<string, string> stringDict = new Dictionary<string, string>();
                    stringDict[ESKeys.MarkdownText] = NotesModel.NM.MarkdownCache;

                    entity.Set(ESKeys.StringDict,stringDict);

                    projectStorage.SetEntity(entity);
                    
                }

                tx.Commit();
            }
        }

        public void SaveDbId(UIApplication uiapp)
        {
            using (Transaction tx = new Transaction(uiapp.ActiveUIDocument.Document, "Set Db Id"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    View view = NotesModel.NM.CurrentView;

                    Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
                    Entity ent = view.GetEntity(pSchema);
                    IDictionary<string, int> intDict = new Dictionary<string, int>();

                    if (ent.Schema == null)
                    {
                        ent = new Entity(pSchema);
                    }
                    else
                    {
                        intDict = ent.Get<IDictionary<string, int>>(ESKeys.IntDict);
                    }


                    intDict[ESKeys.DbNotesId] = NotesModel.NM.IdCache;
                    NotesModel.NM.IdCache = 0;

                    ent.Set(ESKeys.IntDict, intDict);

                    view.SetEntity(ent);
                }

                tx.Commit();
            }
        }
    }
}
