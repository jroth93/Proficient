using Proficient.Utilities;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;


namespace Proficient.Forms;

public class NotesPaneViewModel : INotifyPropertyChanged
{
    private NotesModel NM { get; set; }
    public NotesPaneViewModel(ExternalEvent exEvent, NotesHandler handler)
    {
        NM = new NotesModel(exEvent, handler)
        {
            NPVM = this
        };

        SetUserPermissions();

        EditNotesCommand = new RelayCommand(EditNotes);
        SaveNotesCommand = new RelayCommand(SaveNotes);
        ResetNotesCommand = new RelayCommand(ResetNotes);
        AddLinkCommand = new RelayCommand(AddLink);
        EditLinkCommand = new RelayCommand(EditLink);
        RemoveLinkCommand = new RelayCommand(RemoveLink);

    }

    private static void SetUserPermissions()
    {
        var users = MeiDbConn.GetUsers();
        var user = users.FirstOrDefault(u => u.AutodeskUser == Main.CurrentUser);

        if (user == null) return;
            
        GlobalNotesEditor = user.GlobalNoteEditor;
        GlobalNotesManager = user.GlobalNoteManager;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static bool GlobalNotesEditor { get; set; }
    public static bool GlobalNotesManager { get; set; }

    private NotesType _currentType;
    public NotesType CurrentType
    {
        get => _currentType;
        set
        {
            if (_currentType == value || value != NotesType.Global) return;
                
            _currentType = value;
            Markdown = NM.GetMarkdown(_currentType);
            SetUserPermissions();
            NotifyPropertyChanged();
        }
    }
    private string _markdown = string.Empty;
    public string Markdown
    {
        get => _markdown;
        set
        {
            if (_markdown == value) return;
                
            _markdown = value;
            NotifyPropertyChanged();

        }
    }
    public int CursorPosition { get; set; }
        
    public ICommand EditNotesCommand { get; private set; }
    private void EditNotes()
    {
        if (CurrentType != NotesType.Global || NotesModel.NM.GetDbId() != 0)
        {
            new MarkdownEditor
            {
                DataContext = this
            }.Show();
        }
        else
        {
            TaskDialog.Show("Global Link Required", "Add new link or edit link to select existing notes.", TaskDialogCommonButtons.Ok);
        }
    }

    public ICommand SaveNotesCommand { get; private set; }
    private void SaveNotes()
    {
        NM.SaveMarkdown(CurrentType, Markdown);
    }

    public ICommand ResetNotesCommand { get; }
    private void ResetNotes()
    {
        Markdown = NM.GetMarkdown(CurrentType);
    }

    public ICommand AddLinkCommand {get;}
    private void AddLink()
    {
        var defDesc = NotesModel.NM.CurrentView.Name;

        var ef = new EntryForm("Enter Description for Notes Entry", defDesc);
        if (!ef.ShowDialog() ?? true) return;
        NotesModel.NM.AddGlobalNotes(ef.Entry);
        Markdown = string.Empty;

    }

    public ICommand EditLinkCommand {get; }
    private void EditLink()
    {
        var gnsf = new GlobalNotesSelectForm(NotesModel.NM.GetDbId());
        if (!gnsf.ShowDialog() ?? true) return;
        var dn = ((NotesSelectViewModel)gnsf.DataContext).NoteEntry;
        NotesModel.NM.SaveDbId(dn.Id);
        Markdown = dn.Markdown;
    }

    public ICommand RemoveLinkCommand {get; }
    private void RemoveLink()
    {
        NotesModel.NM.SaveDbId(0);
        Markdown = string.Empty;
    }
}

[ValueConversion(typeof(bool), typeof(GridLength))]
public class GlobalTabButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool globalTab = value != null && (bool)value;
        return globalTab && NotesPaneViewModel.GlobalNotesManager ? new GridLength(25) : new GridLength(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

[ValueConversion(typeof(bool), typeof(GridLength))]
public class EditButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool globalTab = value != null && (bool)value;
        return (globalTab && NotesPaneViewModel.GlobalNotesEditor) || !globalTab ? new GridLength(35) : new GridLength(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}


[ValueConversion(typeof(NotesType), typeof(int))]
public class TabConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (int)value;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (NotesType)value;
}