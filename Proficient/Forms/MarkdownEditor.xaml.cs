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
        private NotesPane.NotesTab tab;
        private bool isSaved = false;
        public MarkdownEditor(NotesPane sender, NotesPane.NotesTab nt, string initial)
        {
            InitializeComponent();
            np = sender;
            tab = nt;
            tb.Text = initial;
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            np.SetMarkup(tab, tb.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            isSaved = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (isSaved)
            {
                np.Save(tab, tb.Text);
            }
            else 
            { 
                np.Reset(tab);
            }
        }
    }
}
