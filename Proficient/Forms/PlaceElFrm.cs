using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace Proficient.Forms;

public partial class PlaceElFrm : Form
{
    public PlaceElFrm()
    {
        InitializeComponent();
        startoffsetlbl.Visible = false;
        startoffset.Visible = false;
        txtlabel.Text = @"Number of Elements:";
    }

    private void RadioNumber_CheckedChanged(object sender, EventArgs e)
    {
        if (!radionumber.Checked) return;
            
        txtlabel.Text = @"Number of Elements:";
        startoffsetlbl.Visible = false;
        startoffset.Visible = false;
    }

    private void RadioOffset_CheckedChanged(object sender, EventArgs e)
    {
        if (!radiooffset.Checked) return;
            
        txtlabel.Text = @"Distance between Elements (ft):";
        startoffset.Visible = true;
        startoffsetlbl.Visible = true;
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Hide();
    }

    private void PlaceElFrm_Load(object sender, EventArgs e)
    {

    }
}