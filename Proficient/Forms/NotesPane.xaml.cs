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
        private MarkdownParser Mdp { get; set; }
        private string _state;
        public string State
        {
            get => _state;
            set
            {
                _state = value;
                fdsv.Document = Mdp.Transform(value);
            }
        }

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
            State = EQIConnection.GetDesignNoteEntry();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right,
            };
            data.VisibleByDefault = true;
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);
        }

        

        public void Reset()
        {
            State = EQIConnection.GetDesignNoteEntry();
        }

        private void EQIEditButton_OnClick(object sender, RoutedEventArgs e)
        {
            new MarkdownEditor(this).Show();
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
