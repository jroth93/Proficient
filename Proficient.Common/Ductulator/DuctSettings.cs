using Proficient.Utilities;
using System.Globalization;
using System.Text.Json;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace Proficient.Ductulator;

public partial class UserSettings : Form
{
    public UserSettings()
    {
        InitializeComponent();

        Main.Settings ??= new();
        TopMost = Main.Settings.AppOnTop;
        radiovert.Checked = Main.Settings.AppVert;
        radiohor.Checked = !Main.Settings.AppVert;

        numericUpDown1.Value = Main.Settings.FricPrec;
        maxdepthtxt.Text = Convert.ToString(Main.Settings.DefDepthMax);
        mindepthtxt.Text = Convert.ToString(Main.Settings.DefDepthMin);
        Velocitytxt.Text = Convert.ToString(Main.Settings.DefVelocity);
        Frictiontxt.Text = Convert.ToString(Main.Settings.DefFriction, CultureInfo.CurrentCulture);
    }

    private void UserSettings_Load(object sender, EventArgs e)
    {

    }

    private void Savebutton_Click(object sender, EventArgs e)
    {
        Main.Settings ??= new();

        Main.Settings.DefFriction = Convert.ToDouble(Frictiontxt.Text);
        Main.Settings.DefVelocity = Convert.ToInt32(Velocitytxt.Text);
        Main.Settings.DefDepthMax = Convert.ToInt32(maxdepthtxt.Text);
        Main.Settings.DefDepthMin = Convert.ToInt32(mindepthtxt.Text);
        Main.Settings.FricPrec = Convert.ToInt32(numericUpDown1.Value);
        Main.Settings.AppOnTop = checkBox1.Checked;
        Main.Settings.AppVert = radiovert.Checked;
        MessageBox.Show(@"Please restart program for new settings to take effect.", @"Restart Program");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string jsonSettings = JsonSerializer.Serialize(Main.Settings, options);

        File.WriteAllText(Names.File.UserSettings, jsonSettings);
        this.Close();
    }

}