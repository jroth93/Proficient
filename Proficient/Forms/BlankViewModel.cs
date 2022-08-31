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
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Proficient.Forms
{
    /// <summary>
    /// Interaction logic for Blank.xaml
    /// </summary>

    public class BlankViewModel : INotifyPropertyChanged
    {
        public BlankViewModel()
        {
            StackPanelItems = new ObservableCollection<FrameworkElement>();
            Blank = new Blank()
            {
                DataContext = this
            };
            Blank.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Blank Blank { get; set; }
        public Orientation StackPanelOrientation { get; set; } = Orientation.Vertical;

        public ObservableCollection<FrameworkElement> StackPanelItems { get; set; }

        public void AddButton(string style, string content, Action clickEvent, bool closeOnClick, bool? dialogResult)
        {
            Style s = Blank.FindResource(style) as Style;
            Button btn = new Button()
            {
                Content = content,
                Style = s
            };
            btn.Click += new RoutedEventHandler((object sender, RoutedEventArgs args) =>
            {
                clickEvent();
                Blank.DialogResult = dialogResult;
                if (closeOnClick)
                {
                    Blank.Close();
                }
            });
            StackPanelItems.Add(btn);
            NotifyPropertyChanged("StackPanelItems");
        }

        public void AddLabel(string content)
        {
            Label lbl = new Label()
            {
                Content = content
            };
            StackPanelItems.Add(lbl);
        }

        public void SetLocation(int x, int y)
        {
            Blank.Loaded += (object sender, RoutedEventArgs args) =>
            {
                Blank.Left = x - Blank.Width / 2;
                Blank.Top = y - Blank.Height / 2;
            };
        }

        public bool? ShowWindow(bool dialog)
        {
            if (dialog)
            {
                return Blank.ShowDialog();
            }
            else
            {
                Blank.Show();
                return null;
            }
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Blank.DialogResult = false;
                Blank.Close();
            }
        }

    }
}
