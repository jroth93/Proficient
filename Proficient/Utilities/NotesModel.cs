using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proficient.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;


namespace Proficient.Utilities
{
    public enum NotesType
    {
        View = 0,
        Project = 1,
        Global = 2
    }
    public class NotesModel
    {
        public static NotesModel NM { get; set; }
        public NotesPaneViewModel NPVM { get; set; }
        public View CurrentView { get; set; }
        public Document CurrentDoc { get; set; }
        public string MarkdownCache { get; set; } = string.Empty;
        public int IdCache { get; set; } = 0;

        private NotesHandler m_Handler;
        private ExternalEvent m_ExEvent;

        public NotesModel(ExternalEvent exEvent, NotesHandler handler)
        {
            NM = this;
            m_Handler = handler;
            m_ExEvent = exEvent;
        }

        public void SaveMarkdown(NotesType nt, string markdown)
        {
            switch (nt)
            {
                case NotesType.View:
                    SaveViewNotes(markdown);
                    break;
                case NotesType.Project:
                    SaveProjectNotes(markdown);
                    break;
                case NotesType.Global:
                    SaveGlobalNotes(markdown);
                    break;
            }
        }

        public string GetMarkdown(NotesType nt)
        {
            switch (nt)
            {
                case NotesType.View:
                    return GetViewNotes();
                case NotesType.Project:
                    return GetProjectNotes(); 
                case NotesType.Global:
                    return GetGlobalNotes();
                default:
                    return string.Empty;
            }
        }

        public void ViewChange(View view)
        {
            CurrentView = view;
            if(NPVM.CurrentType != NotesType.Project)
            {
                NPVM.Markdown = GetMarkdown(NPVM.CurrentType);
            }
        }

        private string GetViewNotes()
        {
            Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
            Entity ent = CurrentView.GetEntity(pSchema);

            if (ent.Schema != null)
            {
                IDictionary<string, string> stringDict = ent.Get<IDictionary<string, string>>(ESKeys.StringDict);
                stringDict.TryGetValue(ESKeys.MarkdownText, out string md);
                return md;
            }
            else
            {
                return string.Empty;
            }

        }
        private void SaveViewNotes(string notes)
        {
            MarkdownCache = notes;
            m_Handler.Request.Make(NotesType.View);
            m_ExEvent.Raise();
        }

        public void ProjectChange(Document doc)
        {
            CurrentDoc = doc;
            if(NPVM.CurrentType == NotesType.Project)
            {
                NPVM.Markdown = GetMarkdown(NotesType.Project);
            }
        }

        private string GetProjectNotes()
        {
            var projectStorage =
                new FilteredElementCollector(CurrentDoc)
                .OfClass(typeof(DataStorage))
                .FirstElement();

            if (projectStorage == null)
            {
                return string.Empty;
            }

            Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
            Entity ent = projectStorage.GetEntity(pSchema);

            if (ent.Schema != null)
            {
                IDictionary<string, string> stringDict = ent.Get<IDictionary<string, string>>(ESKeys.StringDict);
                stringDict.TryGetValue(ESKeys.MarkdownText, out string md);
                return md;
            }
            else
            {
                return string.Empty;
            }
        }

        private void SaveProjectNotes(string notes)
        {
            MarkdownCache = notes;
            m_Handler.Request.Make(NotesType.Project);
            m_ExEvent.Raise();
        }
        public void AddGlobalNotes(string desc)
        {
            int newId = MEIDBConn.CreateEQINote(desc);
            SaveDbId(newId);
        }

        private string GetGlobalNotes()
        {
            int id = GetDbId();

            if (id == 0)
            {
                return string.Empty;
            }
            
            EQINote dn = MEIDBConn.GetEQINote(id);

            if (dn != null)
            {
                return dn.Markdown;
            }
            else
            {
                SaveDbId(0);
                return string.Empty;
            }


        }

        private void SaveGlobalNotes(string markdown)
        {
            MEIDBConn.SetEQINote(GetDbId(), markdown);
        }

        public int GetDbId()
        {
            Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
            Entity ent = CurrentView.GetEntity(pSchema);

            if (ent.Schema != null)
            {
                IDictionary<string, int> intDict = ent.Get<IDictionary<string, int>>(ESKeys.IntDict);
                if (intDict.TryGetValue(ESKeys.DbNotesId, out int id))
                {
                    return id;
                }
            }
            return 0;
        }

        public void SaveDbId(int id)
        {
            IdCache = id;
            m_Handler.Request.Make(NotesType.Global);
            m_ExEvent.Raise();
        }


    }
}
