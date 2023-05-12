using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

namespace Proficient.Forms;

public partial class EntryBox : Form
{
    public string Entry { get; private set; }
    private readonly Type _expectedType;
    private readonly string _errorMessage;
    private readonly string _cap;
    public EntryBox(string title, string caption, Type expType, string errorMsg)
    {
        InitializeComponent();
        _expectedType = expType;
        _errorMessage = errorMsg;
        _cap = caption;
        Text = title;
        label1.Text = _cap;
    }

    public EntryBox(string title, string caption, Type expType)
    {
        InitializeComponent();
        _expectedType = expType;
        _errorMessage = "Invalid Format. Please Try Again.";
        _cap = caption;
        Text = title;
        label1.Text = _cap;
    }

    public EntryBox(string title, string caption)
    {
        InitializeComponent();
        _expectedType = typeof(string);
        _errorMessage = "Invalid Format. Please Try Again.";
        _cap = caption;
        Text = title;
        label1.Text = _cap;
    }


    private void OkButton_Click(object sender, EventArgs e)
    {

        Entry = textBox1.Text;

        if (ValidateEntry())
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            DialogResult = DialogResult.None;
            label1.Text = _errorMessage + "\n\n" + _cap;
        }

    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private bool ValidateEntry()
    {
        if (_expectedType == typeof(double))
            return double.TryParse(Entry, out _);
        if (_expectedType == typeof(int))
            return int.TryParse(Entry, out _);

        return true;
    }
}