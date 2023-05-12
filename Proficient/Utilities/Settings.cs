namespace Proficient.Utilities;

public class Settings
{
    public Settings()
    {
        //proficient settings
        SwitchEnlarged = true;
        DefWorkset = "M-Mechanical";
        PipeDist = 9;
        DefFont = "3/32\" Arial";
        HideDesignNotes = true;
        SuppressSchWarning = false;

        //ductulator settings
        DefFriction = 0.08;
        DefVelocity = 500;
        DefDepthMin = 6;
        DefDepthMax = 20;
        FricPrec = 3;
        AppOnTop = false;
        AppVert = false;
    }
    public bool SwitchEnlarged { get; set; }
    public string DefWorkset { get; set; }
    public int PipeDist { get; set; }
    public string DefFont { get; set; }
    public bool HideDesignNotes { get; set; }
    public bool SuppressSchWarning { get; set; }

    public double DefFriction { get; set; }
    public int DefVelocity { get; set; }
    public int DefDepthMin { get; set; }
    public int DefDepthMax { get; set; }
    public int FricPrec { get; set; }
    public bool AppOnTop { get; set; }
    public bool AppVert { get; set; }

}