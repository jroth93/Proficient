using System;
using System.Windows.Forms;

namespace Proficient
{
    public partial class UserSettings : Form
    {
        public UserSettings()
        {
            InitializeComponent();
            TopMost = Main.Settings.appOnTop;
            radiovert.Checked = Main.Settings.appVert;
            radiohor.Checked = !Main.Settings.appVert;

            numericUpDown1.Value = Main.Settings.fricPrec;
            maxdepthtxt.Text = Convert.ToString(Main.Settings.defDepthMax);
            mindepthtxt.Text = Convert.ToString(Main.Settings.defDepthMin);
            Velocitytxt.Text = Convert.ToString(Main.Settings.defVelocity);
            Frictiontxt.Text = Convert.ToString(Main.Settings.defFriction);
        }

        private void UserSettings_Load(object sender, EventArgs e)
        {

        }

        private void Savebutton_Click(object sender, EventArgs e)
        {
            Main.Settings.defFriction = Convert.ToDouble(Frictiontxt.Text);
            Main.Settings.defVelocity = Convert.ToInt32(Velocitytxt.Text);
            Main.Settings.defDepthMax = Convert.ToInt32(maxdepthtxt.Text);
            Main.Settings.defDepthMin = Convert.ToInt32(mindepthtxt.Text);
            Main.Settings.fricPrec = Convert.ToInt32(numericUpDown1.Value);
            Main.Settings.appOnTop = checkBox1.Checked;
            Main.Settings.appVert = radiovert.Checked;
            MessageBox.Show("Please restart program for new settings to take effect.", "Restart Program");
            this.Close();
        }

    }
}
