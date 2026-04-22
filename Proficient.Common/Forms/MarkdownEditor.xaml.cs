using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using DocumentFormat.OpenXml.ExtendedProperties;
using Proficient.Utilities;


namespace Proficient.Forms;

/// <summary>
/// Interaction logic for MarkdownEditor.xaml
/// </summary>
public partial class MarkdownEditor : Window
{
    public MarkdownEditor()
    {
        InitializeComponent();


        var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
        Action setAlignmentValue = () => {
            if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null) menuDropAlignmentField.SetValue(null, false);
        };
        setAlignmentValue();
        SystemParameters.StaticPropertyChanged += (sender, e) => { setAlignmentValue(); };
    }

    private void Button_Click(object sender, RoutedEventArgs e) => Close();

    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        string? snippet = ((System.Windows.Controls.MenuItem)sender).Tag as string;
        int pos = tb.CaretIndex;
        string newMd = tb.Text.Insert(pos, snippet ?? string.Empty);
        tb.Text = newMd;
        tb.CaretIndex = pos + (snippet?.Length ?? 0);
    }

}