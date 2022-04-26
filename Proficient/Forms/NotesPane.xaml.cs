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
        private MarkdownParser Mdp { get; }
        private View currentView;

        public NotesPane()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, (sender, e) => Process.Start((string)e.Parameter)));

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

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState()
            {
                DockPosition = DockPosition.Right,
            };
            data.VisibleByDefault = true;
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);
        }

        public void SetMarkup(NotesTab nt, string markup)
        {
            switch (nt)
            {
                case NotesTab.View:
                    viewFdsv.Document = Mdp.Transform(markup);
                    break;
                case NotesTab.EQI:

                    break;
                case NotesTab.Project:

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
                case NotesTab.EQI:

                    break;
                case NotesTab.Project:

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
                case NotesTab.EQI:

                    break;
                case NotesTab.Project:

                    break;
            }
            //EQIConnection.GetDesignNoteEntry();
        }
        public enum NotesTab
        {
            View,
            EQI,
            Project
        }

        public void ViewChange(View view)
        {
            currentView = view;
            viewFdsv.Document = Mdp.Transform(GetViewNotes());
        }
        private string GetViewNotes()
        {
            Schema viewSchema = Schema.Lookup(Names.Guids.ViewSchema);
            Entity ent = currentView.GetEntity(viewSchema);
            Field field = viewSchema.GetField("MarkdownText");

            if (ent.Schema != null)
            {
                return ent.Get<string>(field);
            }

            ent = new Entity(viewSchema);
            ent.Set(field, string.Empty);
            return string.Empty;


        }
        private void SaveViewNotes(string notes)
        {
            Schema viewSchema = Schema.Lookup(Names.Guids.ViewSchema);
            Entity ent = currentView.GetEntity(viewSchema);
            Field field = viewSchema.GetField("MarkdownText");

            if (ent.Schema == null)
            {
                ent = new Entity(viewSchema);
            }

            ent.Set(field, notes);
        }


        private void EQIEditButton_OnClick(object sender, RoutedEventArgs e)
        {
            new MarkdownEditor(this, NotesTab.EQI, "").Show();
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
        }
    }
}
