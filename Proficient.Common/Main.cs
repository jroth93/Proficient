using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI.Events;
using MySql.Data.EntityFramework;
using Proficient.Electrical;
using Proficient.Forms;
using Proficient.Mechanical;
using Proficient.Utilities;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using ASApp = Autodesk.Revit.ApplicationServices.Application;
using Settings = Proficient.Utilities.Settings;

#if !PRE25
using System.Runtime.Loader;
#endif

namespace Proficient;

[Transaction(TransactionMode.Manual)]
public class Main : IExternalApplication
{
    public static Main? Instance { get; set; }
    public static Settings? Settings { get; set; }
    public static  UIControlledApplication? App { get; set; }
    public static string? ProficientVersion { get; set; }
    public static string? CurrentUser { get; set; }

    public static readonly Guid AppId = new("339af853-36e4-461f-9171-c5dceda4e721");

    private static ElecLoadDmu? _elecLoadDmu;
    //private static BreakerDmu? _breakerDmu;
    private static DuctFittingDmu? _ductFittingDmu;
    private KeyListener? _listener;

    public Result OnStartup(UIControlledApplication uicApp)
    {
        try
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

            //DbConfiguration.SetConfiguration(new MySqlEFConfiguration());

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            {
                const string path = @"Z:\Revit\Proficient\Proficient Config Files\errorLog.txt";
                using var sw = File.AppendText(path);
                var e = (Exception)eventArgs.ExceptionObject;
                StringBuilder sb = new();
                sb.Append(CurrentUser).Append(" ").AppendLine(DateTime.Now.ToString(CultureInfo.CurrentCulture)).AppendLine(e.Message).AppendLine(e.StackTrace);
                sw.WriteLine(sb.ToString());
            };
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            File.WriteAllText(
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop), "Proficient_error.txt"),
            ex.ToString());
            return Result.Failed;
        }
    }
    public Result OnShutdown(UIControlledApplication uicApp)
    {
        RemoveEventListeners();
        //AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        return Result.Succeeded;
    }

    private static void App_DocumentOpened(object? sender, DocumentOpenedEventArgs args)
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
                .FirstOrDefault(e => e.Name.Equals(Settings?.DefWorkset));

            using var tx = new Transaction(doc, "Change Workset");

            if (tx.Start() == TransactionStatus.Started && ws is not null) 
                wst.SetActiveWorksetId(ws.Id);
            tx.Commit();
        }

        //add display separation parameter listeners for Elec Load DMU
        var mePar = new FilteredElementCollector(args.Document)
            .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
            .Where(e => e is FamilyInstance)
            .Select(el => el.LookupParameter(Names.Parameter.DisplaySeparation))
            .First();

        if (mePar is not null && _elecLoadDmu is not null)
        {
            UpdaterRegistry.AddTrigger(
                _elecLoadDmu.GetUpdaterId(),
                new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment),
                Element.GetChangeTypeParameter(mePar));
        }

        /*
        //add listeners for Breaker DMU
        var wirePar = new FilteredElementCollector(args.Document)
            .OfCategory(BuiltInCategory.OST_Wire)
            .Where(el => el is Wire)
            .Select(el => el.LookupParameter(Names.Parameter.BreakerOptions))
            .First();

        if (wirePar is not null && _breakerDmu is not null)
        {
            UpdaterRegistry.AddTrigger(
                    _breakerDmu.GetUpdaterId(),
                    new ElementCategoryFilter(BuiltInCategory.OST_Wire),
                    Element.GetChangeTypeParameter(wirePar));
        }

        var circuitPar = new FilteredElementCollector(args.Document)
            .OfCategory(BuiltInCategory.OST_ElectricalCircuit)
            .Select(el => el.LookupParameter(Names.Parameter.BreakerOptions))
            .First();

        if (circuitPar is not null && _breakerDmu is not null)
        {
            UpdaterRegistry.AddTrigger(
                    _breakerDmu.GetUpdaterId(),
                    new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit),
                    Element.GetChangeTypeParameter(circuitPar));
        }
        */

    }
    private void App_Idling(object? sender, IdlingEventArgs args)
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

            /*
            var bDmu = new BreakerDmu();

            if (!UpdaterRegistry.IsUpdaterEnabled(bDmu.GetUpdaterId()))
            {
                UpdaterRegistry.EnableUpdater(bDmu.GetUpdaterId());
            }
            */

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
    public void App_ViewActivated(object? sender, EventArgs args)
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
        try
        {
            if (!doc.IsWorkshared || tx.Start() != TransactionStatus.Started) return;

            var wst = doc.GetWorksetTable();
            string viewSub = view.GetParameters(Names.Parameter.ViewSubdiscipline).Any() ?
                    view.GetParameters(Names.Parameter.ViewSubdiscipline).First().AsString() :
                    string.Empty;

            var isEnlarged = view.Name.ToLower().Contains("enlarged") || viewSub.ToLower().Contains("enlarged");
            var isSite = (view.Name.ToLower().Contains("site") || viewSub.ToLower().Contains("site")) &&
                          Settings?.DefWorkset[0] == 'E';
            if (isEnlarged && Settings is not null && Settings.SwitchEnlarged)
            {
                string ewName = Settings.DefWorkset[0] == 'M' ?
                    Names.Workset.MechEnlarged :
                    Names.Workset.ElecEnlarged;
                var enWs = new FilteredWorksetCollector(doc).FirstOrDefault(e => e.Name.Equals(ewName));

                if (enWs != null)
                    wst.SetActiveWorksetId(enWs.Id);

            }
            else if (isSite && Settings is not null && Settings.SwitchEnlarged)
            {
                var siteWs = new FilteredWorksetCollector(doc).FirstOrDefault(e => e.Name.Equals(Names.Workset.ElecSite));

                if (siteWs != null)
                    wst.SetActiveWorksetId(siteWs.Id);
            }
            else if (Settings is not null)
            {
                var defWs = new FilteredWorksetCollector(doc).FirstOrDefault(e => e.Name.Equals(Settings.DefWorkset));

                if (defWs is not null)
                    wst.SetActiveWorksetId(defWs.Id);
            }

            tx.Commit();
        }
        catch (Exception ex)
        {
            File.WriteAllText(
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop), "Proficient_error.txt"),
            ex.ToString());
            if (tx.GetStatus() == TransactionStatus.Started)
                tx.RollBack();
        }


    }
    public void App_DialogBoxShowing(object? sender, DialogBoxShowingEventArgs args)
    {
        if (args is not TaskDialogShowingEventArgs e) return;
            
        if (e.Message.StartsWith("This change will be applied to all elements of type") && Settings is not null && Settings.SuppressSchWarning)
            e.OverrideResult((int)TaskDialogCommonButtons.Ok);
        else if (e.Message.Contains("references Revit add-ins that are not installed"))
            e.OverrideResult((int)TaskDialogCommonButtons.Close);
        else if (e.Message.StartsWith("The following resources are not up to date"))
            e.OverrideResult(1001);

    }
    public void App_DocumentPrinting(object? sender, DocumentPrintingEventArgs args)
    {
        if (Settings is not null && !Settings.HideDesignNotes) return;
            
        var printViewIds = args.GetViewElementIds().ToList();
        DesignNotes.Hide(args.Document, printViewIds);
    }
    public void App_DocumentPrinted(object? sender, DocumentPrintedEventArgs args)
    {
        if (Settings is not null && Settings.HideDesignNotes)
            DesignNotes.Unhide(args.Document);
    }
    private static void App_DocumentClosing(object? sender, DocumentClosingEventArgs args)
    {
        if (args.Document.Application.Documents.Size == 1)
            args.Document.DocumentClosing -= App_DocumentClosing;
    }
    private static void App_ApplicationClosing(object? sender, ApplicationClosingEventArgs e)
    {
        if (!File.Exists(Names.File.ServerDll)) return;
            
        string? currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion;
        if (ProficientVersion != currentVersion)
            Process.Start(Names.File.ProficientInstaller);
    }
    private static void App_ApplicationInitialized(object? sender, ApplicationInitializedEventArgs args)
    {
        /*
        if (sender is not ASApp app || !ASApp.IsLoggedIn) return;
        ProficientVersion ??= string.Empty;

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
            var principal = Thread.CurrentPrincipal as WindowsPrincipal;
            using var pc = new PrincipalContext(ContextType.Domain);
            var up = UserPrincipal.FindByIdentity(pc, principal?.Identity.Name);
                
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
        var version = Convert.ToInt32(app.VersionNumber) > 2021 ?
            app.SubVersionNumber :
            w.Title.Split(' ')[2];

        MeiDbConn.SetUserRevitVersion(u.Id, Convert.ToInt32(app.VersionNumber), version);
        */
    }

    public void AddEventListeners()
    {
        if (App is null) return;

        App.ControlledApplication.DocumentOpened += App_DocumentOpened;
        App.Idling += App_Idling;
        App.ViewActivated += App_ViewActivated;
        App.DialogBoxShowing += App_DialogBoxShowing;
        App.ControlledApplication.DocumentPrinting += App_DocumentPrinting;
        App.ControlledApplication.DocumentPrinted += App_DocumentPrinted;
        App.ApplicationClosing += App_ApplicationClosing;
        App.ControlledApplication.ApplicationInitialized += App_ApplicationInitialized;
    }
    public void InitializeSettings()
    {
        string configPath = Names.File.UserSettings;
        Settings = new Settings();

        if (File.Exists(configPath) && new FileInfo(configPath).Length > 0)
        {
            string configTxt = File.ReadAllText(configPath);

            // System.Text.Json is case-sensitive by default. 
            // Use JsonSerializerOptions if your JSON keys don't match your C# property names exactly.
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Settings = JsonSerializer.Deserialize<Settings>(configTxt, options);
        }
        else
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonSettings = JsonSerializer.Serialize(Settings, options);
            File.WriteAllText(configPath, jsonSettings);
        }
    }
    public void AddCommandBindings()
    {
        if (App is null) return;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.TagByCategory)).BeforeExecuted += BeforeTag;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ElementKeynote)).BeforeExecuted += BeforeTag;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.UserKeynote)).BeforeExecuted += BeforeTag;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.RoomTag)).BeforeExecuted += BeforeTag;

        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SynchronizeAndModifySettings)).BeforeExecuted += BeforeSync;
        App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SynchronizeNow)).BeforeExecuted += BeforeSync;
    }
    public static void AddExternalService()
    {
        ExternalServiceRegistry
            .GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService)
            .AddServer(new Keynotes.ExternalResourceDBServer());
        ExternalServiceRegistry.
            GetService(ExternalServices.BuiltInExternalServices.ExternalResourceUIService)
            .AddServer(new Keynotes.ExternalResourceUIServer());
    }
    public static void CheckToolbarVersion()
    {
        if (!File.Exists(Names.File.ServerDll)) return;
            
        ProficientVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
        string? currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll)?.FileVersion;
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

        /*
        _breakerDmu = new BreakerDmu();
        UpdaterRegistry.RegisterUpdater(_breakerDmu);
        var fw = new ElementCategoryFilter(BuiltInCategory.OST_Wire);
        var fc = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);

        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fw, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM)));
        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fw, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_WIRE_CIRCUITS)));
        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fw, Element.GetChangeTypeElementAddition());
        UpdaterRegistry.AddTrigger(_breakerDmu.GetUpdaterId(), fc, Element.GetChangeTypeElementAddition());
        */

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
        App?.RegisterDockablePane(NotesPane.PaneId, "Proficient Notes", np);
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
        if (App is null) return;
        App.ControlledApplication.DocumentOpened -= App_DocumentOpened;
        App.ViewActivated -= App_ViewActivated;
        App.DialogBoxShowing -= App_DialogBoxShowing;
        App.ControlledApplication.DocumentPrinting -= App_DocumentPrinting;
        App.ControlledApplication.DocumentPrinted -= App_DocumentPrinted;
        App.Idling -= App_Idling;
    }
    private void BeforeTag(object? sender, BeforeExecutedEventArgs arg)
    {
        _listener?.UnHookKeyboard();
        _listener = new KeyListener();
        _listener.OnKeyPressed += OnKeyPressed;
        _listener.HookKeyboard();
    }
    private static void OnKeyPressed(object? sender, KeyPressedArgs e)
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
    private static void BeforeSync(object? sender, BeforeExecutedEventArgs arg)
    {
        try
        {
            UpdaterRegistry.DisableUpdater(new ElecLoadDmu().GetUpdaterId());
            //UpdaterRegistry.DisableUpdater(new BreakerDmu().GetUpdaterId());
            UpdaterRegistry.DisableUpdater(new DuctFittingDmu().GetUpdaterId());
        }
        catch(Exception ex)
        {
            TaskDialog.Show("Error Disabling Updaters", ex.ToString(), TaskDialogCommonButtons.Ok);
        }
    }

    private static Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        // Get the name of the assembly Revit is looking for
        var assemblyName = new AssemblyName(args.Name).Name;

        // Define the path to your add-in's folder
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyPath = Path.Combine(assemblyDir ?? string.Empty, assemblyName + ".dll");

        // If the DLL exists in your folder, load it explicitly
        if (File.Exists(assemblyPath))
        {
            return Assembly.LoadFrom(assemblyPath);
        }

        return null;
    }
}