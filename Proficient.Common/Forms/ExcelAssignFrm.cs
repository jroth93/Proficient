using System.Windows.Forms;
using static Proficient.General.ExcelAssign;
using ComboBox = System.Windows.Forms.ComboBox;
using Form = System.Windows.Forms.Form;

namespace Proficient.Forms;

[Transaction(TransactionMode.Manual)]
public partial class ExcelAssignFrm : Form
{
    private int _parCnt = 1;
    private string[] _cols = [];
    private bool _byType;
    private readonly List<ComboBox> _colDrops = [];
    private readonly List<ComboBox> _parDrops = [];

    public ExcelAssignFrm()
    {
        InitializeComponent();
        getColsBtn.Visible = false;
        catDrop.Items.AddRange([.. GetCategories()]);
        catDrop.SelectedIndex = 0;
        _colDrops.Add(sc1);
        _parDrops.Add(dp1);
    }

    private static int DropDownWidth(ComboBox myCombo)
    {
        var maxWidth = 0;
        var label1 = new Label();

        foreach (var obj in myCombo.Items)
        {
            label1.Text = Convert.ToString(obj);
            var temp = label1.PreferredWidth;
            if (temp > maxWidth)
            {
                maxWidth = temp;
            }
        }
        label1.Dispose();
        return maxWidth + SystemInformation.VerticalScrollBarWidth;
    }

    private void XlFileBtn_Click(object sender, EventArgs e)
    {
        var fd = new OpenFileDialog
        {
            Title = @"Browse to Excel file.",
            ValidateNames = false,
        };
        fd.ShowDialog();
        if (fd.FileName == string.Empty) 
            return;
        filelocationtxt.Text = fd.FileName;
        var xlSheets = OpenExcel(fd.FileName).ToArray<object>();
        wkshtDrop.Items.AddRange(xlSheets);
        wkshtDrop.SelectedIndex = 0;
    }

    private void AssnBtn_Click(object sender, EventArgs e)
    {
        var errorLog = string.Empty;
        for (var i = 1; i <= _parCnt; i++)
        {
            var parName = Convert.ToString(_parDrops[i - 1].SelectedItem);
            parName = parName?.Substring(0, parName.Length - 7) ?? string.Empty;
            var familyName = Convert.ToString(familyDrop.SelectedItem) ?? string.Empty;
            var keyCol = KeyColDrop.SelectedIndex + 1;
            var parCol = KeyColDrop.Items.IndexOf(_colDrops[i - 1].SelectedItem) + 1;
            var startRow = Convert.ToInt32(hdrRowCtrl.Value) + 1;

            errorLog += _byType ? 
                AssignTypeParameters(familyName, parName, keyCol, startRow, parCol) :
                AssignInstParameters(familyName, parName, keyCol, startRow, parCol);

        }

        if (errorLog != string.Empty)
        {
            MessageBox.Show(@"There were errors in the parameter assignment. Please see the error log in the Excel file directory for details.", @"Assignment Errors", MessageBoxButtons.OK);
            WriteErrorFile(errorLog);
            return;
        }
            
        MessageBox.Show(@"Parameters have been assigned.", @"Success!", MessageBoxButtons.OK);

    }

    private void CloseBtn_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void WsDrop_SelectedIndexChanged(object sender, EventArgs e)
    {
        var wsIndex = wkshtDrop.SelectedIndex + 1;
        var hdrRow = Convert.ToInt32(hdrRowCtrl.Value);
        _cols = GetExcelColumns(wsIndex, hdrRow);

        KeyColDrop.Items.Clear();
        KeyColDrop.Items.AddRange([.. _cols]);
        KeyColDrop.SelectedIndex = 0;

    }

    private void CatDrop_SelectedIndexChanged(object sender, EventArgs e)
    {
        var catName = Convert.ToString(catDrop.SelectedItem) ?? string.Empty;
        familyDrop.Items.Clear();
        familyDrop.Items.AddRange([.. GetFamiliesOfCategory(catName)]);
    }

    private void KeyColumnDrop_SelectedIndexChanged(object sender, EventArgs e)
    {
        sc1.Items.Clear();
        sc1.Items.AddRange([.. _cols]);
        sc1.Items.Remove(KeyColDrop.SelectedItem);
        if (_cols.Length > 1)
            sc1.SelectedIndex = 0;
    }

    private void FamilyDrop_SelectedIndexChanged(object sender, EventArgs e)
    {
        dp1.Items.Clear();

        var familyName = Convert.ToString(familyDrop.SelectedItem) ?? string.Empty;

        dp1.Items.AddRange([.. GetFamilyParameters(familyName)]);
        dp1.SelectedIndex = 0;
        dp1.DropDownWidth = DropDownWidth(dp1);
    }

    private void DP1_SelectedIndexChanged(object sender, EventArgs e)
    {
        var curItem = Convert.ToString(dp1.SelectedItem);
        var typeInst = curItem?.Substring(curItem.Length - 5, 4) ?? string.Empty;

        _byType = typeInst != "inst";
        typeInstLbl.Text = _byType ? @"Assigning by Type" : @"Assigning by Instance";

        foreach (var cb in _parDrops)
        {
            if (_parDrops.IndexOf(cb) == 0) 
                continue;
            cb.Items.Clear();
            foreach (string par in dp1.Items)
            {
                typeInst = par.Substring(par.Length - 5, 4);
                if (_byType && typeInst == "type")
                    cb.Items.Add(par);
                else if (_byType && typeInst == "inst")
                    cb.Items.Add(par);
            }
            cb.SelectedIndex = 0;

        }
    }

    private void AddBtn_Click(object sender, EventArgs e)
    {

        Height += 35;
        _colDrops.Add(new ComboBox());
        _colDrops[_parCnt].Size = _colDrops[_parCnt - 1].Size;
        Controls.Add(_colDrops[_parCnt]);
        _colDrops[_parCnt].Location = _colDrops[_parCnt - 1].Location;
        _colDrops[_parCnt].Width = _colDrops[_parCnt - 1].Width;
        _colDrops[_parCnt].DropDownStyle = ComboBoxStyle.DropDownList;
        _colDrops[_parCnt].Top += 35;
        _colDrops[_parCnt].DropDownWidth = _colDrops[_parCnt - 1].DropDownWidth;

        _parDrops.Add(new ComboBox());
        _parDrops[_parCnt].Size = _parDrops[_parCnt - 1].Size;
        Controls.Add(_parDrops[_parCnt]);
        _parDrops[_parCnt].Location = _parDrops[_parCnt - 1].Location;
        _parDrops[_parCnt].Width = _parDrops[_parCnt - 1].Width;
        _parDrops[_parCnt].DropDownStyle = ComboBoxStyle.DropDownList;
        _parDrops[_parCnt].Top += 35;
        _parDrops[_parCnt].DropDownWidth = _parDrops[_parCnt - 1].DropDownWidth;

        _colDrops[_parCnt].Items.AddRange([.. _cols]);
        _colDrops[_parCnt].Items.Remove(KeyColDrop.SelectedItem);

        foreach (string par in dp1.Items)
        {
            var typeInst = par.Substring(par.Length - 5, 4);
            if (_byType && typeInst == "type")
                _parDrops[_parCnt].Items.Add(par);
            else if (!_byType && typeInst == "inst")
                _parDrops[_parCnt].Items.Add(par);
        }
        if (_parDrops[_parCnt].Items.Count > 0)
            _parDrops[_parCnt].SelectedIndex = 0;

        if (_cols.Length >= _parCnt + 2)
            _colDrops[_parCnt].SelectedIndex = _parCnt;
        else if (_cols.Length > 2)
            _colDrops[_parCnt].SelectedIndex = _cols.Length - 2;
        else if (_cols.Length > 1)
            _colDrops[_parCnt].SelectedIndex = 0;

        _parCnt++;
    }

    private void SubtractBtn_Click(object sender, EventArgs e)
    {
        if (_parCnt == 1) 
            return;
        Height -= 35;
        Controls.Remove(_colDrops[_parCnt - 1]);
        Controls.Remove(_parDrops[_parCnt - 1]);
        _colDrops.RemoveAt(_colDrops.Count - 1);
        _parDrops.RemoveAt(_parDrops.Count - 1);
        _parCnt--;
    }
}