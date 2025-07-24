using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace Proficient.Forms;

public partial class ViewForm : Form
{
    public int SelectedViewIndex { get; private set; }
    public ViewForm(string[] calloutViews)
    {
        InitializeComponent();
        viewdropdown.Items.AddRange(calloutViews.ToArray<object>());
        StartPosition = FormStartPosition.CenterScreen;
        SelectedViewIndex = 0;
    }

    private void OkButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }
    private void CancelButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void ViewDropdown_SelectedIndexChanged(object sender, EventArgs e)
    {
        SelectedViewIndex = viewdropdown.SelectedIndex;
    }
}