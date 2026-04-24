using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Proficient.Forms;

/// <summary>
/// Interaction logic for Blank.xaml
/// </summary>
public class BlankViewModel : INotifyPropertyChanged
{
    public BlankViewModel()
    {
        WrapPanelItems = [];
        Blank = new Blank()
        {
            DataContext = this
        };
        Blank.PreviewKeyDown += HandleEsc;
    }

    private const string WrapPanelProperty = "WrapPanelItems";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged(string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Blank Blank { get; set; }
    public Orientation WrapPanelOrientation { get; set; } = Orientation.Vertical;

    public ObservableCollection<FrameworkElement> WrapPanelItems { get; set; }

    public void AddButton(string style, string content, Action clickEvent, bool closeOnClick, bool? dialogResult)
    {
        var s = Blank.FindResource(style) as Style;
        var btn = new Button()
        {
            Content = content,
            Style = s
        };
        btn.Click += (sender, args) =>
        {
            clickEvent();
            Blank.DialogResult = dialogResult;
            if (closeOnClick)
            {
                Blank.Close();
            }
        };
        WrapPanelItems.Add(btn);
        NotifyPropertyChanged(WrapPanelProperty);
    }

    public void AddLabel(string content)
    {
        var lbl = new Label()
        {
            Content = content
        };
        WrapPanelItems.Add(lbl);
    }

    public void SetLocation(int x, int y)
    {
        Blank.Loaded += (sender, args) =>
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
        if (e.Key != Key.Escape) return;
        Blank.DialogResult = false;
        Blank.Close();
    }

}