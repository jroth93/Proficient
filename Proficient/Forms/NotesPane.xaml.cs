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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Diagnostics;
using System.IO;
using Proficient.Utilities;


namespace Proficient.Forms
{
    /// <summary>
    /// Interaction logic for NotesPane.xaml
    /// </summary>


    public partial class NotesPane : Page, IDockablePaneProvider
    {
        public static readonly DockablePaneId PaneId = new DockablePaneId(new Guid("D39CCF32-627E-4075-B1E6-68D3F1DAC780"));
        public static NotesPane Pane { get; set; }
        private MarkdownParser Mdp { get; }
        public View currentView;
        private NotesHandler m_Handler;
        private ExternalEvent m_ExEvent;
        public string MarkdownCache { get; set; } = string.Empty;

        public NotesPane(ExternalEvent exEvent, NotesHandler handler)
        {
            InitializeComponent();
            Pane = this;
            m_Handler = handler;
            m_ExEvent = exEvent;

            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, (sender, e) => 
            {
                try
                {
                    Process.Start((string)e.Parameter);
                }
                catch
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            Arguments = (string)((Hyperlink)e.OriginalSource).ToolTip,
                            FileName = "explorer.exe"
                        };

                        Process.Start(startInfo);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString());
                    }
                }
            }));

            Mdp = new MarkdownParser()
            {
                DocumentStyle = TryFindResource("DocumentStyle") as Style,
                Heading1Style = TryFindResource("H1Style") as Style,
                Heading2Style = TryFindResource("H2Style") as Style,
                Heading3Style = TryFindResource("H3Style") as Style,
                Heading4Style = TryFindResource("H4Style") as Style,
                LinkStyle = TryFindResource("LinkStyle") as Style,
                ImageStyle = TryFindResource("ImageStyle") as Style,
                SeparatorStyle = TryFindResource("SeparatorStyle") as Style,
                TableStyle = TryFindResource("TableStyle") as Style,
                TableHeaderStyle = TryFindResource("TableHeaderStyle") as Style,
                AssetPathRoot = @"Z:\Revit\Proficient\Images"

            };
        }

        public enum NotesTab
        {
            View = 1,
            Project = 2,
            Global = 3
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState()
            {
                DockPosition = DockPosition.Right,
            };
            data.VisibleByDefault = false;
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);
        }

        public void SetMarkup(NotesTab nt, string markup)
        {
            switch (nt)
            {
                case NotesTab.View:
                    viewFdsv.Document = Mdp.Transform(markup);
                    break;
                case NotesTab.Project:

                    break;
                case NotesTab.Global:

                    break;
            }
        }
        
        public void Save(NotesTab nt, string markup)
        {
            switch (nt)
            {
                case NotesTab.View:
                    SaveViewNotes(markup);
                    break;
                case NotesTab.Project:

                    break;
                case NotesTab.Global:

                    break;
            }
        }
        public void Reset(NotesTab nt)
        {
            switch (nt)
            {
                case NotesTab.View:
                    viewFdsv.Document = Mdp.Transform(GetViewNotes());
                    break;
                case NotesTab.Project:

                    break;
                case NotesTab.Global:

                    break;
            }
            //EQIConnection.GetDesignNoteEntry();
        }


        public void ViewChange(View view)
        {
            currentView = view;
            viewFdsv.Document = Mdp.Transform(GetViewNotes());
        }

        private string GetViewNotes()
        {
            Schema pSchema = Schema.Lookup(Names.Guids.ProficientSchema);
            Entity ent = currentView.GetEntity(pSchema);

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
            m_Handler.Request.Make(NotesTab.View);
            m_ExEvent.Raise();

        }

        private void GlobalEditButton_OnClick(object sender, RoutedEventArgs e)
        {
            new MarkdownEditor(this, NotesTab.Global, "").Show();
        }

        private void AddLinkButton_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void EditLinkButton_OnClick(object sender, RoutedEventArgs e)
        {
        }
        private void RemoveLinkButton_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void ViewEditButton_OnClick(object sender, RoutedEventArgs e)
        {
            new MarkdownEditor(this, NotesTab.View, GetViewNotes()).Show();
        }
    }
}
