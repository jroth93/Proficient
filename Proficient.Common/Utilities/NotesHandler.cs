using Autodesk.Revit.DB.ExtensibleStorage;

namespace Proficient.Utilities;

public class NotesHandler : IExternalEventHandler
{
    public NotesRequest Request { get; } = new ();

    public string GetName() => "Notes Request Handler";

    public void Execute(UIApplication uiapp)
    {
        var tab = Request.Take();

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

    public void SaveViewNotes(UIApplication uiApp)
    {
        using Transaction tx = new (uiApp.ActiveUIDocument.Document, "Set View Notes");
        if (tx.Start() != TransactionStatus.Started) return;
        
        var view = NotesModel.NM?.CurrentView;
        if (view is null) return;
        var pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
        var ent = view.GetEntity(pSchema);
        IDictionary<string, string> stringDict = new Dictionary<string, string>();

        if (ent.Schema is null)
            ent = new Entity(pSchema);
        else
            stringDict = ent.Get<IDictionary<string, string>>(SchemaKeys.StringDict);

        if(NotesModel.NM is not null)
        {
            stringDict[SchemaKeys.MarkdownText] = NotesModel.NM.MarkdownCache;
            NotesModel.NM.MarkdownCache = string.Empty;
        }
        
        ent.Set(SchemaKeys.StringDict, stringDict);

        view.SetEntity(ent);

        tx.Commit();
    }

    public void SaveProjectNotes(UIApplication uiApp)
    {
        using Transaction tx = new (uiApp.ActiveUIDocument.Document, "Set Project Notes");
        if (tx.Start() != TransactionStatus.Started) return;
        
        var doc = uiApp.ActiveUIDocument.Document;
        var projectStorage =
            new FilteredElementCollector(doc)
                .OfClass(typeof(DataStorage))
                .FirstElement();

        var pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
        var entity = new Entity(pSchema);

        if (projectStorage is null)
        {
            projectStorage = DataStorage.Create(doc);
        }
        else
        {
            var e = projectStorage.GetEntity(pSchema);
            if (e.IsValid())
                entity = e;
        }

        // Create entity which store created info
        IDictionary<string, string> stringDict = new Dictionary<string, string>
        {
            [SchemaKeys.MarkdownText] = NotesModel.NM?.MarkdownCache ?? string.Empty
        };
        entity.Set(SchemaKeys.StringDict,stringDict);
        projectStorage.SetEntity(entity);
        
        tx.Commit();
    }

    public void SaveDbId(UIApplication uiApp)
    {
        using Transaction tx = new (uiApp.ActiveUIDocument.Document, "Set Db Id");
        if (tx.Start() != TransactionStatus.Started) return;
        var view = NotesModel.NM?.CurrentView;
        if(view is null) return;
        var pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
        var ent = view.GetEntity(pSchema);
        IDictionary<string, int> intDict = new Dictionary<string, int>();

        if (ent.Schema == null)
        {
            ent = new Entity(pSchema);
        }
        else
        {
            intDict = ent.Get<IDictionary<string, int>>(SchemaKeys.IntDict);
        }

        if(NotesModel.NM is not null)
        {
            intDict[SchemaKeys.DbNotesId] = NotesModel.NM.IdCache;
            NotesModel.NM.IdCache = 0;
        }

        ent.Set(SchemaKeys.IntDict, intDict);

        view.SetEntity(ent);

        tx.Commit();
    }
}