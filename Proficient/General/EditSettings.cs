using Newtonsoft.Json;
using Proficient.Forms;
using Proficient.Utilities;
using System.Reflection;

namespace Proficient.General;

[Transaction(TransactionMode.Manual)]
internal class EditSettings : IExternalCommand
{
    public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
    {
        var doc = revit.Application.ActiveUIDocument.Document;

        var sf = new SettingsForm
        {
            HideDesignNotes =
            {
                IsChecked = Main.Settings.HideDesignNotes
            },
            PipeDist =
            {
                Text = Convert.ToString(Main.Settings.PipeDist)
            },
            SwitchEnlWorkset =
            {
                IsChecked = Main.Settings.SwitchEnlarged
            }
        };

        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        sf.Version.Content = $"Proficient Version {assemblyVersion}";

        sf.Loaded += (_, _) =>
        {
            var mwe = revit.Application.MainWindowExtents;
            sf.Left = (mwe.Left + mwe.Right) / 2.0 - sf.Width / 2;
            sf.Top = (mwe.Top + mwe.Bottom) / 2.0 - sf.Height / 2;
        };

        sf.DefaultWorkset.ItemsSource = new FilteredWorksetCollector(doc).ToWorksets()
            .Where(ws => ws.Kind == WorksetKind.UserWorkset)
            .Select(ws => ws.Name);
        sf.DefaultWorkset.SelectedItem = Main.Settings.DefWorkset;

        sf.DefaultFont.ItemsSource = new FilteredElementCollector(doc)
            .WherePasses(new ElementClassFilter(typeof(TextNoteType)))
            .Select(txt => txt.Name)
            .ToList();
        sf.DefaultFont.SelectedItem = Main.Settings.DefFont;

        if (sf.ShowDialog() ?? false)
        {
            Main.Settings.DefWorkset = sf.DefaultWorkset.SelectedItem.ToString();
            Main.Settings.SwitchEnlarged = (bool)sf.SwitchEnlWorkset.IsChecked;
            Main.Settings.PipeDist = Convert.ToInt32(sf.PipeDist.Text);
            Main.Settings.DefFont = sf.DefaultFont.SelectedItem.ToString();
            Main.Settings.HideDesignNotes = (bool)sf.HideDesignNotes.IsChecked;

            var jsonSettings = JsonConvert.SerializeObject(Main.Settings);
            File.WriteAllText(Names.File.UserSettings, jsonSettings);
        }


        sf.Close();

        return Result.Succeeded;
    }

}