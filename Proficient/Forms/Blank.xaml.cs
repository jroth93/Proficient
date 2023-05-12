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
using Autodesk.Revit.DB.ExtensibleStorage;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Proficient.Forms;

/// <summary>
/// Interaction logic for Blank.xaml
/// </summary>
public partial class Blank : Window
{
    public Blank()
    {
        InitializeComponent();          
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

}