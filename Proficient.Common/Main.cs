using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI.Events;
using MySql.Data.EntityFramework;
using Newtonsoft.Json;
using Proficient.Electrical;
using Proficient.Forms;
using Proficient.Mechanical;
using Proficient.Utilities;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ASApp = Autodesk.Revit.ApplicationServices.Application;
using Settings = Proficient.Utilities.Settings;

namespace Proficient;

[Transaction(TransactionMode.Manual)]
public class Main : IExternalApplication
{
    public static Main? Instance { get; set; }
    public static Settings? Settings { get; set; }
    public static UIControlledApplication App { get; set; }
    public static string? ProficientVersion { get; set; }
    public static string? CurrentUser { get; set; }

    public static readonly Guid AppId = new("339af853-36e4-461f-9171-c5dceda4e721");

    private static ElecLoadDmu? _elecLoadDmu;
    private static BreakerDmu? _breakerDmu;
    private static DuctFittingDmu? _ductFittingDmu;
    private KeyListener? _listener;

    public Result OnStartup(UIControlledApplication uicApp)
    {
        Instance = this;
        App = uicApp;
        AddEventListeners();
        InitializeSettings();
        AddCommandBindings();
        Ribbon.Create();
        AddExternalService();
        CheckToolbarVersion();

        RegisterDmus();
        //RegisterPanes();
        BuildSchemas();

        DbConfiguration.SetConfiguration(new MySqlEFConfiguration());

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            const string path = @"Z:\Revit\Proficient\Proficient Config Files\errorLog.txt";
            using var sw = File.AppendText(path);
            var e = (Exception) eventArgs.ExceptionObject;
            StringBuilder sb = new();
            sb.Append(CurrentUser).Append(" ").AppendLine(DateTime.Now.ToString(CultureInfo.CurrentCulture)).AppendLine(e.Message).AppendLine(e.StackTrace);
            sw.WriteLine(sb.ToString());
        };

        return Result.Succeeded;
    }
    public Result OnShutdown(UIControlledApplication uicApp)
    {
        RemoveEventListeners();
        return Result.Succeeded;
    }

    private static void App_DocumentOpened(object sender, DocumentOpenedEventArgs args)
    {
        var doc = args.Document;
        //NotesModel.NM.CurrentDoc = doc;
        if (doc == null) return;

        //change workset to user setting
        if (doc.IsWorkshared)
        {
            doc.DocumentClosing += App_DocumentClosing;
            var wst = doc.GetWorksetTable();

            var ws = new FilteredWorksetCollector(doc)
                .FirstOrDefault(e => e.Name.Equals(Settings.DefWorkset));

            using var tx = new Transaction(doc, "Change Workset");
            if (tx.Start() == TransactionStatus.Started && ws != null) wst.SetActiveWorksetId(ws.Id);
            tx.Commit();
        }

        //add display separation parameter listeners for Elec Load DMU
        var mec = new FilteredElementCollector(args.Document)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .Where(e => e is FamilyInstance);

        foreach (var el in mec)
        {
            var par = el.LookupParameter(Names.Parameter.DisplaySeparation);
            if (par != null)
            {
                UpdaterRegistry.AddTrigger(
                    _elecLoadDmu.GetUpdaterId(), 
                    new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment), 
                    Element.GetChangeTypeParameter(par));
            }
        }

        //add listeners for Breaker DMU
        var wec = new FilteredElementCollector(args.Document)
            .OfCategory(BuiltInCategory.OST_Wire)
            .Where(el => el is Wire);

        foreach (var el in wec)
        {
            var par = el.LookupParameter(Names.Parameter.BreakerOptions);
            if (par != null)
            {
                UpdaterRegistry.AddTrigger(
                    _breakerDmu.GetUpdaterId(),
                    new ElementCategoryFilter(BuiltInCategory.OST_Wire), 
                    Element.GetChangeTypeParameter(par));
            }
        }

        var cec = new FilteredElementCollector(args.Document)
            .OfCategory(BuiltInCategory.OST_ElectricalCircuit);

        foreach (var el in cec)
        {
            var par = el.LookupParameter(Names.Parameter.BreakerOptions);

            if (par != null)
            {
                UpdaterRegistry.AddTrigger(
                    _breakerDmu.GetUpdaterId(), 
                    new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit), 
                    Element.GetChangeTypeParameter(par));
            }
        }

    }
    private void App_Idling(object sender, IdlingEventArgs args)
    {
        if (sender is not UIApplication uiApp) return;

        _listener?.UnHookKeyboard();
        var app = uiApp.Application;
        uiApp.GetRibbonPanels("Proficient").First(pnl => pnl.Name == "Electrical").Visible = app.IsElectricalEnabled;
        uiApp.GetRibbonPanels("Proficient").First(pnl => pnl.Name == "Mechanical").Visible = app.IsMechanicalEnabled;

        if(ASApp.IsLoggedIn && CurrentUser == null) CurrentUser = app.Username;

        try
        {
            var elDmu = new ElecLoadDmu();
            if (!UpdaterRegistry.IsUpdaterEnabled(elDmu.GetUpdaterId()))
            {
                UpdaterRegistry.EnableUpdater(elDmu.GetUpdaterId());
            }

            var bDmu = new BreakerDmu();

            if (!UpdaterRegistry.IsUpdaterEnabled(bDmu.GetUpdaterId()))
            {
                UpdaterRegistry.EnableUpdater(bDmu.GetUpdaterId());
            }

            var dfDmu = new DuctFittingDmu();
            if (!UpdaterRegistry.IsUpdaterEnabled(dfDmu.GetUpdaterId()))
            {
                UpdaterRegistry.EnableUpdater(dfDmu.GetUpdaterId());
            }
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Error Enabling Updaters", ex.ToString(), TaskDialogCommonButtons.Ok);
        }
    }
    public void App_ViewActivated(object sender, EventArgs args)
    {
        if (sender is not UIApplication uiApp) return;
        var doc = uiApp.ActiveUIDocument.Document;
        var view = doc.ActiveView;

        //Notes Model Changer
        //NotesModel.NM.ViewChange(view);
        //if(!NotesModel.NM.CurrentDoc.Equals(doc)) 
            //NotesModel.NM.ProjectChange(doc);


        //Workset Changer
        using var tx = new Transaction(doc, "Change Workset");
        if (!doc.IsWorkshared || tx.Start() != TransactionStatus.Started) return;

        var wst = doc.GetWorksetTable();
        string viewSub = view.GetParameters(Names.Parameter.ViewSubdiscipline).Any() ? 
                view.GetParameters(Names.Parameter.ViewSubdiscipline).First().AsString() : 
                string.Empty;

        bool isEnlarged = view.Name.ToLower().Contains("enlarged") || viewSub.ToLower().Contains("enlarged");
        bool isSite = (view.Name.ToLower().Contains("site") || viewSub.ToLower().Contains("site")) &&
                      Settings.DefWorkset[0] == 'E';
        if (isEnlarged && Settings.SwitchEnlarged)
        {
            string ewName = Settings.DefWorkset[0] == 'M' ? 
                Names.Workset.MechEnlarged : 
                Names.Workset.ElecEnlarged;
            var enWs = new FilteredWorksetCollector(doc).FirstOrDefault(e => e.Name.Equals(ewName));

            if (enWs != null) 
                wst.SetActiveWorksetId(enWs.Id);
                
        }
        else if (isSite && Settings.SwitchEnlarged)
        {
            var siteWs = new FilteredWorksetCollector(doc).FirstOrDefault(e => e.Name.Equals(Names.Workset.ElecSite));

            if (siteWs != null)
                wst.SetActiveWorksetId(siteWs.Id);
        }
        else
        {
            var defWs = new FilteredWorksetCollector(doc).FirstOrDefault(e => e.Name.Equals(Settings.DefWorkset));

            if (defWs != null) 
                wst.SetActiveWorksetId(defWs.Id);
        }

        tx.Commit();


    }
    public void App_DialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
    {
        if (args is not TaskDialogShowingEventArgs e) return;
            
        if (e.Message.StartsWith("This change will be applied to all elements of type") && Settings.SuppressSchWarning)
            e.OverrideResult((int)TaskDialogCommonButtons.Ok);
        else if (e.Message.Contains("references Revit add-ins that are not installed"))
            e.OverrideResult((int)TaskDialogCommonButtons.Close);
        else if (e.Message.StartsWith("The following resources are not up to date"))
            e.OverrideResult(1001);

    }
    public void App_DocumentPrinting(object sender, DocumentPrintingEventArgs args)
    {
        if (!Settings.HideDesignNotes) return;
            
        var printViewIds = args.GetViewElementIds().ToList();
        DesignNotes.Hide(args.Document, printViewIds);
    }
    public void App_DocumentPrinted(object sender, DocumentPrintedEventArgs args)
    {
        if (Settings.HideDesignNotes)
            DesignNotes.Unhide(args.Document);
    }
    private static void App_DocumentClosing(object sender, DocumentClosingEventArgs args)
    {
        if (args.Document.Application.Documents.Size == 1)
            args.Document.DocumentClosing -= App_DocumentClosing;
    }
    private static void App_ApplicationClosing(object sender, ApplicationClosingEventArgs e)
    {
        if (!File.Exists(Names.File.ServerDll)) return;
            
        string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion;
        if (ProficientVersion != currentVersion)
            Process.Start(Names.File.SilentUpdateExe);
    }
    private static void App_ApplicationInitialized(object sender, ApplicationInitializedEventArgs args)
    {
        if (sender is not ASApp app || !ASApp.IsLoggedIn) return;
        CurrentUser = app.Username;
        var u = MeiDbConn.GetUserByAdId(CurrentUser);
        if (u != null && u.ProficientVersion != ProficientVersion)
        {
            u.ProficientVersion = ProficientVersion;
            MeiDbConn.SetUserProficientVersion(u);
        }
        else
        {
            Thread.GetDomain().SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            var principal = (WindowsPrincipal)Thread.CurrentPrincipal;
            using var pc = new PrincipalContext(ContextType.Domain);
            var up = UserPrincipal.FindByIdentity(pc, principal.Identity.Name);
                
            if(up != null)
            {
                var ti = new CultureInfo("en-US", false).TextInfo;
                u = MeiDbConn.CreateUser(CurrentUser, ProficientVersion, ti.ToTitleCase(up.GivenName), ti.ToTitleCase(up.Surname));
            }
            else
            {
                u = MeiDbConn.CreateUser(CurrentUser, ProficientVersion); 
            }
        }

        var hwndSource = HwndSource.FromHwnd(App.MainWindowHandle);
        if (hwndSource is not { RootVisual: Window w }) return;
        string version = Convert.ToInt32(app.VersionNumber) > 2021 ?
            app.SubVersionNumber :
            w.Title.Split(' ')[2];

        MeiDbConn.SetUserRevitVersion(u.Id, Convert.ToInt32(app.VersionNumber), version);
    }

    public void AddEventListeners()
    {
        App.ControlledApplication.DocumentOpened += App_DocumentOpened;
        App.Idling += App_Idling;
        App.ViewActivated += App_ViewActivated;
        App.DialogBoxShowing += App_DialogBoxShowing;
        App.ControlledApplication.DocumentPrinting += App_DocumentPrinting;
        App.ControlledApplication.DocumentPrinted += App_DocumentPrinted;
        App.ApplicationClosing += App_ApplicationClosing;
        App.ControlledApplication.ApplicationInitialized += App_ApplicationInitialized;


        //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }
    public void InitializeSettings()
    {
        const string configPath = Names.File.UserSettings;
        Settings = new Settings();

        if (File.Exists(configPath) && File.ReadAllText(configPath) != string.Empty)
        {
            string configTxt = File.ReadAllText(configPath);
            Settings = JsonConvert.DeserializeObject<Settings>(configTxt);
        }
        else
        {
            string jsonSettings = JsonConvert.SerializeObject(Settings);
            File.WriteAllText(configPath, jsonSettings);
        }
    }
    public void AddCommandBindings()
    {
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.TagByCategory)).BeforeExecuted += BeforeTag;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ElementKeynote)).BeforeExecuted += BeforeTag;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.UserKeynote)).BeforeExecuted += BeforeTag;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.RoomTag)).BeforeExecuted += BeforeTag;

        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SynchronizeAndModifySettings)).BeforeExecuted += BeforeSync;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SynchronizeNow)).BeforeExecuted += BeforeSync;
    }
    public void AddExternalService()
    {
        ExternalServiceRegistry
            .GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService)
            .AddServer(new Keynotes.ExternalResourceDBServer());
        ExternalServiceRegistry.
            GetService(ExternalServices.BuiltInExternalServices.ExternalResourceUIService)
            .AddServer(new Keynotes.ExternalResourceUIServer());
    }
    public void CheckToolbarVersion()
    {
        if (!File.Exists(Names.File.ServerDll)) return;
            
        ProficientVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion;
        if (ProficientVersion != currentVersion)
        {
            Util.BalloonTip("Proficient", "New version of Proficient available.\nVersion will be updated on Revit close", "Proficient Out Of Date");
        }
    }
    private static void RegisterDmus()
    {

        _elecLoadDmu = new ElecLoadDmu();
        UpdaterRegistry.RegisterUpdater(_elecLoadDmu);
        var fme = new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);
        var fec = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);

        UpdaterRegistry.AddTrigger(_elecLoadDmu.GetUpdaterId(), fme, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ALL_MODEL_MARK)));
        UpdaterRegistry.AddTrigger(_elecLoadDmu.GetUpdaterId(), fme, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_MARK)));
        UpdaterRegistry.AddTrigger(_elecLoadDmu.GetUpdaterId(), fme, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ELEM_TYPE_PARAM)));
        UpdaterRegistry.AddTrigger(_elecLoadDmu.GetUpdaterId(), fme, Element.GetChangeTypeElementAddition());
        UpdaterRegistry.AddTrigger(_elecLoadDmu.GetUpdaterId(), fec, Element.GetChangeTypeElementAddition());

        _breakerDmu = new BreakerDmu();
        UpdaterRegistry.RegisterUpdater(_breakerDmu);
        var fw = new ElementCategoryFilter(BuiltInCategory.OST_Wire);
        var fc = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);

        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fw, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM)));
        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fw, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_WIRE_CIRCUITS)));
        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fw, Element.GetChangeTypeElementAddition());
        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fc, Element.GetChangeTypeElementAddition());

        _ductFittingDmu = new DuctFittingDmu();
        UpdaterRegistry.RegisterUpdater(_ductFittingDmu);
        var fdf = new ElementCategoryFilter(BuiltInCategory.OST_DuctFitting);

        UpdaterRegistry.AddTrigger(_ductFittingDmu.GetUpdaterId(), fdf, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ELEM_TYPE_PARAM)));
        UpdaterRegistry.AddTrigger(_ductFittingDmu.GetUpdaterId(), fdf, Element.GetChangeTypeElementAddition());
    }
    private static void RegisterPanes()
    {
        var handler = new NotesHandler();
        var exEvent = ExternalEvent.Create(handler);
            
        var np = new NotesPane(exEvent, handler);
        App.RegisterDockablePane(NotesPane.PaneId, "Proficient Notes", np);
    }
    private static void BuildSchemas()
    {
        var sb = new SchemaBuilder(Names.Guids.ProficientSchema)
            .SetReadAccessLevel(AccessLevel.Public)
            .SetWriteAccessLevel(AccessLevel.Public)
            .SetSchemaName("ProficientSchema");

        sb.AddMapField(SchemaKeys.BoolDict, typeof(string), typeof(bool));
        sb.AddMapField(SchemaKeys.IntDict, typeof(string), typeof(int));
        sb.AddMapField(SchemaKeys.DoubleDict, typeof(string), typeof(double))
#if PRE21
            .SetUnitType(UnitType.UT_Custom);
#else
            .SetSpec(SpecTypeId.Custom);
#endif
        sb.AddMapField(SchemaKeys.StringDict, typeof(string), typeof(string));

        sb.Finish();
    }
    private void RemoveEventListeners()
    {
        App.ControlledApplication.DocumentOpened -= App_DocumentOpened;
        App.ViewActivated -= App_ViewActivated;
        App.DialogBoxShowing -= App_DialogBoxShowing;
        App.ControlledApplication.DocumentPrinting -= App_DocumentPrinting;
        App.ControlledApplication.DocumentPrinted -= App_DocumentPrinted;
        App.Idling -= App_Idling;
    }
    private void BeforeTag(object sender, BeforeExecutedEventArgs arg)
    {
        _listener?.UnHookKeyboard();
        _listener = new KeyListener();
        _listener.OnKeyPressed += OnKeyPressed;
        _listener.HookKeyboard();
    }
    private static void OnKeyPressed(object sender, KeyPressedArgs e)
    {

        try
        {
            switch (e.KeyPressed)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    Util.ToggleLeader();
                    break;
                case Key.E:
                    Util.ToggleLeaderEnd();
                    break;
                case Key.Add:
                case Key.OemPlus:
                    Util.ChangeLeaderDistance(true);
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    Util.ChangeLeaderDistance(false);
                    break;
            }
                
        }
        catch (Exception)
        {/*
                if (!ex.Message.Contains("non-enabled element"))
                {
                    TaskDialog.Show("Exception", ex.Message + "\n" + ex.StackTrace);
                }
            */
        }
    }
    private static void BeforeSync(object sender, BeforeExecutedEventArgs arg)
    {
        try
        {
            UpdaterRegistry.DisableUpdater(new ElecLoadDmu().GetUpdaterId());
            UpdaterRegistry.DisableUpdater(new BreakerDmu().GetUpdaterId());
            UpdaterRegistry.DisableUpdater(new DuctFittingDmu().GetUpdaterId());
        }
        catch(Exception ex)
        {
            TaskDialog.Show("Error Disabling Updaters", ex.ToString(), TaskDialogCommonButtons.Ok);
        }
    }

}