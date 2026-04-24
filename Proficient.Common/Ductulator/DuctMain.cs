using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;
using Point = System.Drawing.Point;
using Settings = Proficient.Utilities.Settings;

namespace Proficient.Ductulator;
public partial class DuctMain : Form
{
    public DuctMain()
    {
        InitializeComponent();
        Main.Settings ??= new Settings();

        TopMost = Main.Settings.AppOnTop;
        ActiveControl = Airflowtxt1;
        if (Main.Settings.AppVert) VerticalApplication();
        Depthmaxtxt1.Text = Convert.ToString(Main.Settings.DefDepthMax);
        Depthmintxt1.Text = Convert.ToString(Main.Settings.DefDepthMin);
        Frictiontxt1.Text = Convert.ToString(Main.Settings.DefFriction, CultureInfo.CurrentCulture);
        Veltxt1.Text = Convert.ToString(Main.Settings.DefVelocity);
        Depthmaxtxt2.Text = Convert.ToString(Main.Settings.DefDepthMax);
        Depthmintxt2.Text = Convert.ToString(Main.Settings.DefDepthMin);
        depthmaxtxt3.Text = Convert.ToString(Main.Settings.DefDepthMax);
        depthmintxt3.Text = Convert.ToString(Main.Settings.DefDepthMin);
    }

    private void Tab1Control(object sender, EventArgs e)
    {
        TotalCFM.Text = "";
        Output1.Text = "";
        Output2.Text = "";
        Output10.Text = "";

        var inputs = new List<string>() { Airflowtxt1.Text, Frictiontxt1.Text, Depthmintxt1.Text, Depthmaxtxt1.Text };

        if (Parser(inputs) == false)
        {
            TotalCFM.Text = @"Not Valid Input";
            return;
        }
        var airflow = Convert.ToInt32(new DataTable().Compute(inputs[0], null));
        var friction = Convert.ToDouble(new DataTable().Compute(inputs[1], null));
        var minDepth = Convert.ToInt32(new DataTable().Compute(inputs[2], null));
        var maxDepth = Convert.ToInt32(new DataTable().Compute(inputs[3], null));

        var result = Regex.Match(inputs[0], Constants.NumPattern);
        TotalCFM.Text = result.Success ? "" : $"{Convert.ToInt32(airflow)} CFM Total";

        var outputs = Backend.AirflowFriction(airflow, friction, minDepth, maxDepth);

        Output1.Text = outputs[0];
        Output2.Text = outputs[1];
        Output10.Text = outputs[2];
        WindowSize(Output1.Height);
    }

    private void Tab2Control(object sender, EventArgs e)
    {
        TotalCFM2.Text = "";
        Output3.Text = "";
        Output4.Text = "";
        Output11.Text = "";

        var inputs = new List<string>
        {
            Airflowtxt2.Text,
            Veltxt1.Text,
            Depthmintxt2.Text,
            Depthmaxtxt2.Text
        };

        if (Parser(inputs) == false)
        {
            TotalCFM2.Text = @"Not Valid Input";
            return;
        }

        var airflow = Convert.ToInt32(new DataTable().Compute(inputs[0], null));
        var velocity = Convert.ToInt32(new DataTable().Compute(inputs[1], null));
        var minDepth = Convert.ToInt32(new DataTable().Compute(inputs[2], null));
        var maxDepth = Convert.ToInt32(new DataTable().Compute(inputs[3], null));

        var result = Regex.Match(inputs[0], Constants.NumPattern);
        TotalCFM2.Text = result.Success ? "" : $"{Convert.ToInt32(airflow)} CFM Total";

        var outputs = Backend.AirflowVelocity(airflow, velocity, minDepth, maxDepth);

        Output3.Text = outputs[0];
        Output4.Text = outputs[1];
        Output11.Text = outputs[2];

        WindowSize(Output3.Height);
    }

    private void Tab3Control(object sender, EventArgs e)
    {

        var isRnd = radiornd1.Checked;
        Depthlbl1.Visible = !isRnd;
        Depthtxt1.Visible = !isRnd;
        Widthlbl1.Visible = !isRnd;
        Widthtxt1.Visible = !isRnd;
        label3.Visible = !isRnd;
        Depthlbl3.Visible = !isRnd;
        label5.Visible = isRnd;
        Diatxt1.Visible = isRnd;

        if (isRnd && Main.Settings is not null && Main.Settings.AppVert)
        {
            Output5.Location = new Point(40, 130);
        }
        else if (!isRnd && Main.Settings is not null && Main.Settings.AppVert)
        {
            Output5.Location = new Point(40, 170);
        }

        TotalCFM3.Text = "";
        Output5.Text = "";

        var inputs = isRnd ? new List<string> { Airflowtxt3.Text, Diatxt1.Text } : [Airflowtxt3.Text, Widthtxt1.Text, Depthtxt1.Text];

        if (Parser(inputs) == false)
        {
            Output5.Text = @"Not Valid Input";
            return;
        }

        var airflow = Convert.ToInt32(new DataTable().Compute(inputs[0], null));
        var dia = isRnd ? Convert.ToInt32(new DataTable().Compute(inputs[1], null)) : 0;
        var width = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[1], null));
        var depth = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[2], null));

        var result = Regex.Match(inputs[0], Constants.NumPattern);
        TotalCFM3.Text = result.Success ? "" : $"{Convert.ToInt32(airflow)} CFM Total";

        var velocity = Convert.ToInt32(Functions.VelocitySolver(airflow, dia, width, depth, isRnd));
        var friction = Math.Ceiling(Functions.FrictionSolver(airflow, dia, width, depth, isRnd) * Constants.Fprecision) / Constants.Fprecision;

        Output5.Text = $"{friction} In./100 ft.\n\n{velocity} FPM";
    }

    private void Tab4Control(object sender, EventArgs e)
    {
        var isRnd = radiornd2.Checked;
        depthlbl4.Visible = !isRnd;
        widthlbl2.Visible = !isRnd;
        depthtxt2.Visible = !isRnd;
        widthtxt2.Visible = !isRnd;
        dialbl2.Visible = isRnd;
        diatxt2.Visible = isRnd;
        label6.Visible = !isRnd;
        label7.Visible = !isRnd;

        if (isRnd && Main.Settings is not null && Main.Settings.AppVert)
            Output6.Location = new Point(40, 130);
        else if (!isRnd && Main.Settings is not null && Main.Settings.AppVert)
            Output6.Location = new Point(40, 170);

        Output6.Text = "";

        var inputs = isRnd ? new List<string> { frictiontxt2.Text, diatxt2.Text } : [frictiontxt2.Text, widthtxt2.Text, depthtxt2.Text];

        if (Parser(inputs) == false)
        {
            Output6.Text = @"Not Valid Input";
            return;
        }

        var friction = Convert.ToDouble(new DataTable().Compute(inputs[0], null));
        var dia = isRnd ? Convert.ToInt32(new DataTable().Compute(inputs[1], null)) : 0;
        var width = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[1], null));
        var depth = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[2], null));

        var airflow = Convert.ToInt32(Functions.AirflowSolver(friction, dia, width, depth, isRnd));
        var velocity = Convert.ToInt32(Functions.VelocitySolver(airflow, dia, width, depth, isRnd));

        Output6.Text = $"{airflow} CFM \n\n{velocity} FPM";
    }

    private void Tab5Control(object sender, EventArgs e)
    {
        var isRnd = Radiornd3.Checked;
        widthtxt3.Visible = !isRnd;
        widthlbl3.Visible = !isRnd;
        depthlbl5.Visible = !isRnd;
        depthtxt3.Visible = !isRnd;
        label11.Visible = !isRnd;
        dialbl3.Visible = isRnd;
        diatxt3.Visible = isRnd;

        if (isRnd && Main.Settings is not null && Main.Settings.AppVert)
            Output8.Location = new Point(40, 130);
        else if (!isRnd && Main.Settings is not null && Main.Settings.AppVert)
            Output8.Location = new Point(40, 170);

        Output8.Text = "";

        List<string> inputs = isRnd ? [velocitytxt2.Text, diatxt3.Text ] : [velocitytxt2.Text, widthtxt3.Text, depthtxt3.Text];

        if (Parser(inputs) == false)
        {
            Output8.Text = @"Not Valid Input";
            return;
        }

        var velocity = Convert.ToDouble(new DataTable().Compute(inputs[0], null));
        var dia = isRnd ? Convert.ToInt32(new DataTable().Compute(inputs[1], null)) : 0;
        var width = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[1], null));
        var depth = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[2], null));

        var airflow = Convert.ToInt32(isRnd ? velocity * Math.PI * Math.Pow(dia, 2) / 576 : velocity * width * depth / 144.0);
        var friction = Convert.ToInt32(Functions.FrictionSolver(airflow, dia, width, depth, isRnd) * Constants.Fprecision) / Constants.Fprecision;

        Output8.Text = $"{airflow} CFM\n\n{friction} In./100 ft.";
    }

    private void Tab6Control(object sender, EventArgs e)
    {
        var isRnd = radiornd4.Checked;
        dialbl4.Visible = isRnd;
        diatxt4.Visible = isRnd;
        widthlbl4.Visible = !isRnd;
        widthtxt4.Visible = !isRnd;
        depthtxt4.Visible = !isRnd;
        depthlbl6.Visible = !isRnd;
        label18.Top = isRnd ? 89 : 127;
        depthmintxt3.Top = isRnd ? 92 : 130;
        depthmaxtxt3.Top = isRnd ? 92 : 130;
        label15.Top = isRnd ? 95 : 133;
        label13.Top = isRnd ? 95 : 133;

        if (isRnd && Main.Settings is not null && Main.Settings.AppVert)
            Output9.Location = new Point(40, 130);
        else if (!isRnd && Main.Settings is not null && Main.Settings.AppVert)
            Output9.Location = new Point(40, 170);

        Output9.Text = "";

        var inputs = isRnd ? new List<string> { diatxt4.Text, "1", depthmintxt3.Text, depthmaxtxt3.Text } : [widthtxt4.Text, depthtxt4.Text, depthmintxt3.Text, depthmaxtxt3.Text];

        if (Parser(inputs) == false)
        {
            Output9.Text = @"Not Valid Input";
            return;
        }

        var dia = isRnd ? Convert.ToInt32(new DataTable().Compute(inputs[0], null)) : 0;
        var width = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[0], null));
        var depth = isRnd ? 0 : Convert.ToInt32(new DataTable().Compute(inputs[1], null));
        var minDepth = Convert.ToInt32(new DataTable().Compute(inputs[2], null));
        var maxDepth = Convert.ToInt32(new DataTable().Compute(inputs[3], null));

        var output = Backend.EquivalentDuct(dia, width, depth, minDepth, maxDepth, isRnd);

        Output9.Text = output;

        WindowSize(Output9.Height);
    }

    public static bool Parser(List<string> inputs)
    {

        foreach (var input in inputs)
        {
            if (!Regex.Match(input, Constants.Pattern).Success)
                return false;
            var inputVal = Convert.ToDouble(new DataTable().Compute(input, null));
            if (inputVal is <= 0 or > 2147483647)
                return false;
        }

        return inputs.Count != 0;
    }

    private void WindowSize(int size)
    {
        if (Main.Settings is null) return;

        Height = Main.Settings.AppVert ? 
            size > 170 ? size + 300 : 450 : 
            size > 150 ? size + 130 : 275;
    }

    private void SettingsButtonClick(object sender, EventArgs e)
    {
        new UserSettings().Show();
    }

    public void VerticalApplication()
    {
        //form size
        this.Width = 330;
        this.Height = 450;
        //tab1
        pictureBox1.Location = new Point(255, 5);
        Output1.Location = new Point(40, 165);
        Output2.Location = new Point(110, 165);
        Output10.Location = new Point(170, 165);
        //tab2
        Output3.Location = new Point(40, 165);
        Output4.Location = new Point(105, 165);
        Output11.Location = new Point(175, 165);
        //tab3
        Output5.Location = new Point(40, 170);
        //tab4
        Output6.Location = new Point(40, 170);
        //tab5
        Output8.Location = new Point(40, 170);
        //tab6
        Output9.Location = new Point(40, 170);
        ;
    }

    // Drag from any point on form
    public const int WM_NCLBUTTONDOWN = 0xA1;
    public const int HT_CAPTION = 0x2;
    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    private void Form1_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        ReleaseCapture();
        _ = SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
    }
}