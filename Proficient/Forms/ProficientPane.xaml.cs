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
using System.Windows.Navigation;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Proficient.Forms
{
    /// <summary>
    /// Interaction logic for ProficientPanel.xaml
    /// </summary>
    public partial class ProficientPane : Page, IDockablePaneProvider
    {
        public ProficientPane()
        {
            InitializeComponent();
            InitializeAsync();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right,
                MinimumWidth = 200
            };
            data.VisibleByDefault = true;
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);
        }
        private async void InitializeAsync()
        {
            var env = await CoreWebView2Environment.CreateAsync(
                userDataFolder: Path.Combine(Path.GetTempPath(), "ProficientPanel"),
                options: new CoreWebView2EnvironmentOptions(allowSingleSignOnUsingOSPrimaryAccount: true));
            await WebView.EnsureCoreWebView2Async(env);

            WebView.Source = new Uri("https://www.google.com/");
            WebView.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
