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
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Proficient.Utilities;


namespace Proficient.Forms
{
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

            EditNotesCommand = new RelayCommand(new Action(EditNotes));
            SaveNotesCommand = new RelayCommand(new Action(SaveNotes));
            ResetNotesCommand = new RelayCommand(new Action(ResetNotes));
            AddLinkCommand = new RelayCommand(new Action(AddLink));
            EditLinkCommand = new RelayCommand(new Action(EditLink));
            RemoveLinkCommand = new RelayCommand(new Action(RemoveLink));

        }

        private void SetUserPermissions()
        {
            List<User> users = MEIDBConn.GetUsers();
            User user = users.Where(u => u.AutodeskUser == Main.CurrentUser).FirstOrDefault();

            if(user != null)
            {
                GlobalNotesEditor = user.GlobalNoteEditor;
                GlobalNotesManager = user.GlobalNoteManager;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool GlobalNotesEditor { get; set; } = false;
        public static bool GlobalNotesManager { get; set; } = false;

        private NotesType currentType;
        public NotesType CurrentType
        {
            get
            {
                return currentType;
            }
            set
            {
                if (currentType != value)
                {
                    currentType = value;
                    Markdown = NM.GetMarkdown(currentType);

                    if(value == NotesType.Global)
                    {
                        SetUserPermissions();
                        NotifyPropertyChanged();
                    }
                }
            }
        }
        private string markdown = string.Empty;
        public string Markdown
        {
            get
            {
                return markdown;
            }
            set
            {
                if (markdown != value)
                {
                    markdown = value;
                    NotifyPropertyChanged();
                }

            }
        }
        public int CursorPosition { get; set; }
        
        public ICommand EditNotesCommand { get; private set; }
        private void EditNotes()
        {
            if (CurrentType != NotesType.Global || NotesModel.NM.GetDbId() != 0)
            {
                new MarkdownEditor()
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

        public ICommand ResetNotesCommand { get; private set; }
        private void ResetNotes()
        {
            Markdown = NM.GetMarkdown(CurrentType);
        }

        public ICommand AddLinkCommand {get; private set;}
        private void AddLink()
        {
            string defDesc = NotesModel.NM.CurrentView.Name;

            EntryForm ef = new EntryForm("Enter Description for Notes Entry", defDesc);
            if ((bool)ef.ShowDialog())
            {
                NotesModel.NM.AddGlobalNotes(ef.Entry);
                Markdown = string.Empty;
            }

        }

        public ICommand EditLinkCommand {get; private set;}
        private void EditLink()
        {
            GlobalNotesSelectForm gnsf = new GlobalNotesSelectForm(NotesModel.NM.GetDbId());
            if ((bool)gnsf.ShowDialog())
            {
                EQINote dn = ((NotesSelectViewModel)gnsf.DataContext).NoteEntry;
                NotesModel.NM.SaveDbId(dn.Id);
                Markdown = dn.Markdown;
            }
        }

        public ICommand RemoveLinkCommand {get; private set;}
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
            bool globalTab = (bool)value;
            return globalTab && NotesPaneViewModel.GlobalNotesManager ? new GridLength(25) : new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }

    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class EditButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool globalTab = (bool)value;
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

}
