using System;
using System.Windows;
using System.Windows.Input;

namespace Proficient.Forms
{
    /// <summary>
    /// Interaction logic for SettingsForm.xaml
    /// </summary>
    [CLSCompliant(false)]
    public partial class SettingsForm : Window
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Hide();
        }

        private void Increment(object sender, RoutedEventArgs e)
        {
            PipeDist.Text = Convert.ToString(Convert.ToInt32(PipeDist.Text) + 1);
        }

        private void Decrement(object sender, RoutedEventArgs e)
        {
            PipeDist.Text = Convert.ToString(Convert.ToInt32(PipeDist.Text) - 1);
        }
    }
}
