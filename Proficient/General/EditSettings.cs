using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Proficient.Forms;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;


namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class EditSettings : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;

            SettingsForm sf = new SettingsForm();
            sf.HideDesignNotes.IsChecked = Main.Settings.hideDesignNotes;
            sf.PipeDist.Text = Convert.ToString(Main.Settings.pipeDist);
            sf.SwitchEnlWorkset.IsChecked = Main.Settings.switchEnlarged;

            string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            sf.Version.Content = $"Proficient Version {assemblyVersion}";

            sf.Loaded += (object sender, RoutedEventArgs e) =>
            {
                Rectangle mwe = revit.Application.MainWindowExtents;
                sf.Left = (mwe.Left + mwe.Right) / 2 - sf.Width / 2;
                sf.Top = (mwe.Top + mwe.Bottom) / 2 - sf.Height / 2;
            };

            sf.DefaultWorkset.ItemsSource = new FilteredWorksetCollector(doc).ToWorksets()
                .Where(ws => ws.Kind == WorksetKind.UserWorkset)
                .Select(ws => ws.Name);
            sf.DefaultWorkset.SelectedItem = Main.Settings.defWorkset;

            sf.DefaultFont.ItemsSource = new FilteredElementCollector(doc)
                .WherePasses(new ElementClassFilter(typeof(TextNoteType)))
                .Select(txt => txt.Name)
                .ToList();
            sf.DefaultFont.SelectedItem = Main.Settings.defFont;

            if (sf.ShowDialog() ?? false)
            {
                Main.Settings.defWorkset = sf.DefaultWorkset.SelectedItem as string;
                Main.Settings.switchEnlarged = (bool)sf.SwitchEnlWorkset.IsChecked;
                Main.Settings.pipeDist = Convert.ToInt32(sf.PipeDist.Text);
                Main.Settings.defFont = sf.DefaultFont.SelectedItem.ToString();
                Main.Settings.hideDesignNotes = (bool)sf.HideDesignNotes.IsChecked;

                string jsonSettings = JsonConvert.SerializeObject(Main.Settings);
                File.WriteAllText(Names.File.UserSettings, jsonSettings);
            }


            sf.Close();

            return Result.Succeeded;
        }

    }
}
