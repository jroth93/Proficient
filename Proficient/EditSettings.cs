using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Windows;
using Proficient.Forms;
using Newtonsoft.Json;


namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class EditSettings : IExternalCommand
    {
        public Result Execute(ExternalCommandData revit, ref string message, ElementSet elements)
        {
            Document doc = revit.Application.ActiveUIDocument.Document;
            
            SettingsForm sf = new SettingsForm();
            sf.HideDesignNotes.IsOn = Main.Settings.hideDesignNotes;
            sf.PipeDist.Value = Main.Settings.pipeDist;
            sf.SwitchEnlWorkset.IsOn = Main.Settings.switchEnlarged;

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
                Main.Settings.switchEnlarged = sf.SwitchEnlWorkset.IsOn;
                Main.Settings.pipeDist = Convert.ToInt32(sf.PipeDist.Value);
                Main.Settings.defFont = sf.DefaultFont.SelectedItem.ToString();
                Main.Settings.hideDesignNotes = sf.HideDesignNotes.IsOn;

                string jsonSettings = JsonConvert.SerializeObject(Main.Settings);
                File.WriteAllText(Names.File.UserSettings, jsonSettings);
            }


            sf.Close();

            return Result.Succeeded;
        }

    }
}
