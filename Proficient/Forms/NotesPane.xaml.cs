using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Proficient.Utilities;


namespace Proficient.Forms;

/// <summary>
/// Interaction logic for NotesPane.xaml
/// </summary>
public partial class NotesPane : Page, IDockablePaneProvider
{
    public static readonly DockablePaneId PaneId = new DockablePaneId(new Guid("D39CCF32-627E-4075-B1E6-68D3F1DAC780"));

    public NotesPane(ExternalEvent exEvent, NotesHandler handler)
    {
        InitializeComponent();
        DataContext = new NotesPaneViewModel(exEvent, handler);
        CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, NavigationHandler));
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

    private void NavigationHandler(object sender, ExecutedRoutedEventArgs e)
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
    }
}