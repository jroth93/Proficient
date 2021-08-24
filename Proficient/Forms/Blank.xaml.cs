using MahApps.Metro.Controls;
using System;
using System.Windows.Input;

namespace Proficient.Forms
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>
    [CLSCompliant(false)]
    public partial class Blank : MetroWindow
    {
        public Blank()
        {
            InitializeComponent();
            PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
