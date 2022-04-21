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
using System.Windows.Shapes;
using DocumentFormat.OpenXml.ExtendedProperties;
using Proficient.Utilities;

namespace Proficient.Forms
{
    /// <summary>
    /// Interaction logic for MarkdownEditor.xaml
    /// </summary>
    public partial class MarkdownEditor : Window
    {
        private NotesPane np;
        private bool isSaved = false;
        public MarkdownEditor(NotesPane sender)
        {
            InitializeComponent();
            np = sender;
            tb.Text = np.State;
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            np.State = tb.Text;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EQIConnection.SetDesignNoteEntry(tb.Text);
            isSaved = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isSaved)
            {
                np.Reset();
            }
        }
    }
}
