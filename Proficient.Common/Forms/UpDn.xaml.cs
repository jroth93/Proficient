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

namespace Proficient.Forms;

/// <summary>
/// Interaction logic for UpDn.xaml
/// </summary>
public partial class UpDn : UserControl
{
    public UpDn()
    {
        InitializeComponent();
    }

    private void Increment(object sender, RoutedEventArgs e)
    {
        Value.Text = Convert.ToString(Convert.ToInt32(Value.Text) + 1);
    }

    private void Decrement(object sender, RoutedEventArgs e)
    {
        Value.Text = Convert.ToString(Convert.ToInt32(Value.Text) - 1);
    }
}