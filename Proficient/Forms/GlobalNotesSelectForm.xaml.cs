using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Proficient.Forms
{
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
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }

    public class NotesSelectViewModel : INotifyPropertyChanged
    {
        public NotesSelectViewModel(int curId)
        {
            NoteEntries = MEIDBConn.GetEQINotes();
            NoteEntries.Sort((x,y) => x.Description.CompareTo(y.Description));

            if(curId != 0)
            {
                NoteEntry = NoteEntries.Where(x => x.Id == curId).FirstOrDefault();
            }
        }

        public List<EQINote> NoteEntries { get; set; }

        private EQINote _noteEntry;

        public EQINote NoteEntry
        {
            get { return _noteEntry; }
            set
            {
                if (_noteEntry == value) return;
                _noteEntry = value;
                
                NotifyPropertyChanged("NoteEntry");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
