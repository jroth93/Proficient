using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Proficient.Forms;

/// <summary>
/// Interaction logic for ProficientPanel.xaml
/// </summary>
public partial class ProficientPane : Page, IDockablePaneProvider
{
    public ProficientPane()
    {
        InitializeComponent();
    }

    public void SetupDockablePane(DockablePaneProviderData data)
    {
        data.FrameworkElement = this;
        data.InitialState = new DockablePaneState
        {
            DockPosition = DockPosition.Right,
#if PRE20
#else
            MinimumWidth = 200
#endif
        };
        data.VisibleByDefault = true;
        data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);
    }
}