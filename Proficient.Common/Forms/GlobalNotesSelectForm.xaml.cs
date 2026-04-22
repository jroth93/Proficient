using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using Proficient.Utilities;
using System.Globalization;

namespace Proficient.Forms;

/// <summary>
/// Interaction logic for EntryForm.xaml
/// </summary>
public partial class GlobalNotesSelectForm : Window
{
    public GlobalNotesSelectForm(int curId)
    {
        InitializeComponent();
        DataContext = new NotesSelectViewModel(curId);
    }


    private void OkButton_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
        {
            DragMove();
        }
    }
}

public class NotesSelectViewModel : INotifyPropertyChanged
{
    public NotesSelectViewModel(int curId)
    {
        NoteEntries = MeiDbConn.GetEqiNotes();
        NoteEntries.Sort((x,y) => x.Description.CompareTo(y.Description));

        if(curId != 0)
        {
            NoteEntry = NoteEntries.FirstOrDefault(x => x.Id == curId);
        }
    }

    public List<EqiNote> NoteEntries { get; set; }

    private EqiNote? _noteEntry;

    public EqiNote? NoteEntry
    {
        get => _noteEntry;
        set
        {
            if (_noteEntry == value) return;
            _noteEntry = value;

            NotifyPropertyChanged("NoteEntry");
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}