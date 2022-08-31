using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.ExtensibleStorage;
using ASApp = Autodesk.Revit.ApplicationServices.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Data.Entity;
using Proficient.Elec;
using Proficient.Utilities;
using Proficient.Forms;
using MySql.Data.EntityFramework;
using System.DirectoryServices.AccountManagement;
using System.Threading;
using System.Security.Principal;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Main : IExternalApplication
    {
        public static Main Instance { get; set; }
        public static Settings Settings { get; set; }
        public static UIControlledApplication App { get; set; }
        public static string ProficientVersion { get; set; }
        public static string CurrentUser { get; set; }


        public static readonly Guid appId = new Guid("339af853-36e4-461f-9171-c5dceda4e721");

        private static ElecLoadDMU elecLoadDMU;
        private static BreakerDMU breakerDMU;
        private static DuctFittingDMU ductFittingDMU;
        private KeyListener _listener;

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

            RegisterElecLoadDMU();
            RegisterBreakerDMU();
            RegisterDuctFittingDMU();
            RegisterPanes();
            BuildSchemas();

            DbConfiguration.SetConfiguration(new MySqlEFConfiguration());

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication uicApp)
        {
            RemoveEventListeners();
            return Result.Succeeded;
        }

        private void App_DocumentOpened(object sender, DocumentOpenedEventArgs args)
        {
            Document doc = args.Document;
            NotesModel.NM.CurrentDoc = doc;
            if(doc != null)
            {
                //change workset to user setting
                if (doc.IsWorkshared)
                {
                    doc.DocumentClosing += App_DocumentClosing;
                    WorksetTable wst = doc.GetWorksetTable();

                    FilteredWorksetCollector wscol = new FilteredWorksetCollector(doc);
                    Workset workset = wscol.FirstOrDefault(e => e.Name.Equals(Settings.defWorkset));

                    Transaction transaction = new Transaction(doc, "Change Workset");
                    if (transaction.Start() == TransactionStatus.Started && workset != null)
                    {
                        wst.SetActiveWorksetId(workset.Id);
                        transaction.Commit();
                    }
                }



                //add display separation parameter listeners for Elec Load DMU
                IEnumerable<Element> mec = new FilteredElementCollector(args.Document)
                    .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                    .Where(e => e is FamilyInstance);

                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);

                foreach (Element el in mec)
                {
                    Parameter par = el.LookupParameter(Names.Parameter.DisplaySeparation);

                    if (par != null)
                    {
                        UpdaterRegistry.AddTrigger(elecLoadDMU.GetUpdaterId(), f, Element.GetChangeTypeParameter(par));
                    }
                }

                //add listeners for Breaker DMU
                var wec = new FilteredElementCollector(args.Document)
                    .OfCategory(BuiltInCategory.OST_Wire)
                    .Where(el => el is Wire);

                ElementCategoryFilter fw = new ElementCategoryFilter(BuiltInCategory.OST_Wire);

                foreach (Element el in wec)
                {
                    Parameter par = el.LookupParameter(Names.Parameter.BreakerOptions);

                    if (par != null)
                    {
                        UpdaterRegistry.AddTrigger(breakerDMU.GetUpdaterId(), fw, Element.GetChangeTypeParameter(par));
                    }
                }

                var cec = new FilteredElementCollector(args.Document)
                    .OfCategory(BuiltInCategory.OST_ElectricalCircuit);

                ElementCategoryFilter fc = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);

                foreach (Element el in wec)
                {
                    Parameter par = el.LookupParameter(Names.Parameter.BreakerOptions);

                    if (par != null)
                    {
                        UpdaterRegistry.AddTrigger(breakerDMU.GetUpdaterId(), fc, Element.GetChangeTypeParameter(par));
                    }
                }
            }
            
        }
        private void App_Idling(object sender, IdlingEventArgs args)
        {
            _listener?.UnHookKeyboard();
            UIApplication uiapp = sender as UIApplication;
            ASApp app = uiapp.Application;
            uiapp.GetRibbonPanels("Proficient").Where(pnl => pnl.Name == "Electrical").First().Visible = app.IsElectricalEnabled;
            uiapp.GetRibbonPanels("Proficient").Where(pnl => pnl.Name == "Mechanical").First().Visible = app.IsMechanicalEnabled;

            if(ASApp.IsLoggedIn && CurrentUser == null)
            {
                CurrentUser = app.Username;
            }
            
            try
            {
                ElecLoadDMU eldmu = new ElecLoadDMU();
                if (!UpdaterRegistry.IsUpdaterEnabled(eldmu.GetUpdaterId()))
                {
                    UpdaterRegistry.EnableUpdater(eldmu.GetUpdaterId());
                }

                BreakerDMU bdmu = new BreakerDMU();

                if (!UpdaterRegistry.IsUpdaterEnabled(bdmu.GetUpdaterId()))
                {
                    UpdaterRegistry.EnableUpdater(bdmu.GetUpdaterId());
                }

                DuctFittingDMU dfdmu = new DuctFittingDMU();
                if (!UpdaterRegistry.IsUpdaterEnabled(dfdmu.GetUpdaterId()))
                {
                    UpdaterRegistry.EnableUpdater(dfdmu.GetUpdaterId());
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error Enabling Updaters", ex.ToString(), TaskDialogCommonButtons.Ok);
            }
        }
        public void App_ViewActivated(object sender, EventArgs args)
        {
            UIApplication uiapp = sender as UIApplication;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;


            NotesModel.NM.ViewChange(view);
            
            if(NotesModel.NM.CurrentDoc != doc)
            {
                NotesModel.NM.ProjectChange(doc);
            }

            WorksetTable wst = doc.GetWorksetTable();
            FilteredWorksetCollector wscol = new FilteredWorksetCollector(doc);
            string viewname = view.Name;
            string viewsub = 
                view.GetParameters(Names.Parameter.ViewSubdiscipline).Count() > 0 ? 
                view.GetParameters(Names.Parameter.ViewSubdiscipline)[0].AsString() : 
                string.Empty;

            if (viewname.ToLower().Contains("enlarged") || viewsub.ToLower().Contains("enlarged"))
            {
                if (doc.IsWorkshared && Settings.switchEnlarged)
                {
                    string enlWkst = Settings.defWorkset[0] == 'M' ? Names.Workset.MechEnlarged : Names.Workset.ElecEnlarged;
                    Workset enlWorkset = wscol.FirstOrDefault(e => e.Name.Equals(enlWkst));

                    Transaction transaction = new Transaction(doc, "Change Workset");
                    if (transaction.Start() == TransactionStatus.Started && enlWkst != null)
                    {
                        wst.SetActiveWorksetId(enlWorkset.Id);
                        transaction.Commit();
                    }
                }
            }
            else if ((viewname.ToLower().Contains("site") || viewsub.ToLower().Contains("site")) && Settings.defWorkset[0] == 'E')
            {
                if (doc.IsWorkshared && Settings.switchEnlarged)
                {
                    Workset siteWorkset = wscol.FirstOrDefault(e => e.Name.Equals(Names.Workset.ElecSite));

                    Transaction transaction = new Transaction(doc, "Change Workset");
                    if (transaction.Start() == TransactionStatus.Started && siteWorkset != null)
                    {
                        wst.SetActiveWorksetId(siteWorkset.Id);
                        transaction.Commit();
                    }
                }
            }
            else
            {
                Workset workset = wscol.FirstOrDefault(e => e.Name.Equals(Settings.defWorkset));

                Transaction transaction = new Transaction(doc, "Change Workset");
                if (transaction.Start() == TransactionStatus.Started && workset != null)
                {
                    wst.SetActiveWorksetId(workset.Id);
                    transaction.Commit();
                }
            }


        }
        public void App_DialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            if (!(args is TaskDialogShowingEventArgs e))
            {
                return;
            }

            if (e.Message.StartsWith("This change will be applied to all elements of type") && Settings.suppressSchWarning)
            {
                e.OverrideResult((int)TaskDialogCommonButtons.Ok);
            }
            if (e.Message.Contains("references Revit add-ins that are not installed"))
            {
                e.OverrideResult((int)TaskDialogCommonButtons.Close);
            }

            if (e.Message.StartsWith("The following resources are not up to date"))
            {
                e.OverrideResult(1001);
            }

            /*if (e.Message.StartsWith("The file being loaded is causing a conflict with existing data in the model"))
            {
                e.OverrideResult((int)TaskDialogCommonButtons.Ok);
            }*/
        }
        public void App_DocumentPrinting(object sender, DocumentPrintingEventArgs args)
        {
            if (Settings.hideDesignNotes)
            {
                Document doc = args.Document;
                List<ElementId> printViews = args.GetViewElementIds().ToList();
                DesignNotes.Hide(doc, printViews);
            }
        }
        public void App_DocumentPrinted(object sender, DocumentPrintedEventArgs args)
        {
            if (Settings.hideDesignNotes)
            {
                Document doc = args.Document;
                DesignNotes.Unhide(doc);
            }
        }
        private void App_DocumentClosing(object sender, DocumentClosingEventArgs args)
        {
            if (args.Document.Application.Documents.Size == 1)
            {
                args.Document.DocumentClosing -= App_DocumentClosing;
            }
            throw new NotImplementedException();
        }
        private void App_ApplicationClosing(object sender, ApplicationClosingEventArgs e)
        {
            
            if (File.Exists(Names.File.ServerDll))
            {
                string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion;
                if (ProficientVersion != currentVersion)
                {
                    Process.Start(Names.File.SilentUpdateExe);
                }
            }
        }
        private void App_ApplicationInitialized(object sender, ApplicationInitializedEventArgs args)
        {
            ASApp app = sender as ASApp;
            if (ASApp.IsLoggedIn)
            {
                CurrentUser = app.Username;
                User u = MEIDBConn.GetUserByAdId(CurrentUser);
                if (u != null)
                {
                    if(u.ProficientVersion != ProficientVersion)
                    {
                        u.ProficientVersion = ProficientVersion;
                        MEIDBConn.SetUserProficientVersion(u);
                    }

                    HwndSource hwndSource = HwndSource.FromHwnd(App.MainWindowHandle);
                    string version = Convert.ToInt32(app.VersionNumber) > 2021 ?
                        app.SubVersionNumber :                        
                        (hwndSource.RootVisual as Window).Title.Split(' ')[2];

                    MEIDBConn.SetUserRevitVersion(u.Id, Convert.ToInt32(app.VersionNumber), version);
                }
                else
                {
                    Thread.GetDomain().SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                    WindowsPrincipal principal = (WindowsPrincipal)Thread.CurrentPrincipal;
                    UserPrincipal up;
                    using (PrincipalContext pc = new PrincipalContext(ContextType.Domain))
                    {
                        up = UserPrincipal.FindByIdentity(pc, principal.Identity.Name);
                    }

                    if(up != null)
                    {
                        TextInfo ti = new CultureInfo("en-US", false).TextInfo;
                        u = MEIDBConn.CreateUser(CurrentUser, ProficientVersion, ti.ToTitleCase(up.GivenName), ti.ToTitleCase(up.Surname));
                    }
                    else
                    {
                        u = MEIDBConn.CreateUser(CurrentUser, ProficientVersion); 
                    }

                    HwndSource hwndSource = HwndSource.FromHwnd((sender as UIApplication).MainWindowHandle);
                    string version = (hwndSource.RootVisual as Window).Title.Split(' ')[2];

                    MEIDBConn.SetUserRevitVersion(u.Id, Convert.ToInt32(app.VersionNumber), version);


                }
            }
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
            string configPath = Names.File.UserSettings;
            string configTxt;
            Settings = new Settings();

            if (File.Exists(configPath) && File.ReadAllText(configPath) != string.Empty)
            {
                configTxt = File.ReadAllText(configPath);
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
            App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.TagByCategory)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ElementKeynote)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.UserKeynote)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.RoomTag)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);

            App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SynchronizeAndModifySettings)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeSync);
            App.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.SynchronizeNow)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeSync);
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
            if (File.Exists(Names.File.ServerDll))
            {
                ProficientVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion.ToString();
                if (ProficientVersion != currentVersion)
                {
                    Util.BalloonTip("Proficient", "New version of Proficient available.\nVersion will be updated on Revit close", "Proficient Out Of Date");
                }
            }
        }
        private void RegisterElecLoadDMU()
        {

            elecLoadDMU = new ElecLoadDMU();
            UpdaterRegistry.RegisterUpdater(elecLoadDMU);
            ElementCategoryFilter fme = new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);
            ElementCategoryFilter fec = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);

            UpdaterRegistry.AddTrigger(elecLoadDMU.GetUpdaterId(), fme, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ALL_MODEL_MARK)));
            UpdaterRegistry.AddTrigger(elecLoadDMU.GetUpdaterId(), fme, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ALL_MODEL_TYPE_MARK)));
            UpdaterRegistry.AddTrigger(elecLoadDMU.GetUpdaterId(), fme, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ELEM_TYPE_PARAM)));
            UpdaterRegistry.AddTrigger(elecLoadDMU.GetUpdaterId(), fme, Element.GetChangeTypeElementAddition());
            UpdaterRegistry.AddTrigger(elecLoadDMU.GetUpdaterId(), fec, Element.GetChangeTypeElementAddition());

        }
        private void RegisterBreakerDMU()
        {
            breakerDMU = new BreakerDMU();
            UpdaterRegistry.RegisterUpdater(breakerDMU);
            ElementCategoryFilter fw = new ElementCategoryFilter(BuiltInCategory.OST_Wire);
            ElementCategoryFilter fc = new ElementCategoryFilter(BuiltInCategory.OST_ElectricalCircuit);

            UpdaterRegistry.AddTrigger(breakerDMU.GetUpdaterId(), fw, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_CIRCUIT_PANEL_PARAM)));
            UpdaterRegistry.AddTrigger(breakerDMU.GetUpdaterId(), fw, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.RBS_ELEC_WIRE_CIRCUITS)));
            UpdaterRegistry.AddTrigger(breakerDMU.GetUpdaterId(), fw, Element.GetChangeTypeElementAddition());
            UpdaterRegistry.AddTrigger(breakerDMU.GetUpdaterId(), fc, Element.GetChangeTypeElementAddition());


        }
        private void RegisterDuctFittingDMU()
        {
            ductFittingDMU = new DuctFittingDMU();
            UpdaterRegistry.RegisterUpdater(ductFittingDMU);
            ElementCategoryFilter fdf = new ElementCategoryFilter(BuiltInCategory.OST_DuctFitting);

            UpdaterRegistry.AddTrigger(ductFittingDMU.GetUpdaterId(), fdf, Element.GetChangeTypeParameter(new ElementId(BuiltInParameter.ELEM_TYPE_PARAM)));
            UpdaterRegistry.AddTrigger(ductFittingDMU.GetUpdaterId(), fdf, Element.GetChangeTypeElementAddition());

        }
        private void RegisterPanes()
        {
            NotesHandler handler = new NotesHandler();
            ExternalEvent exEvent = ExternalEvent.Create(handler);
            
            NotesPane np = new NotesPane(exEvent, handler);
            App.RegisterDockablePane(NotesPane.PaneId, "Proficient Notes", np);
        }
        private void BuildSchemas()
        {
            SchemaBuilder sb = new SchemaBuilder(Names.Guids.ProficientSchema)
                .SetReadAccessLevel(AccessLevel.Public)
                .SetWriteAccessLevel(AccessLevel.Public)
                .SetSchemaName("ProficientSchema");

            sb.AddMapField(ESKeys.BoolDict, typeof(string), typeof(bool));
            sb.AddMapField(ESKeys.IntDict, typeof(string), typeof(int));
            sb.AddMapField(ESKeys.DoubleDict, typeof(string), typeof(double))
#if FORGE
                .SetSpec(SpecTypeId.Custom);
#else
                .SetUnitType(UnitType.UT_Custom);
#endif
            sb.AddMapField(ESKeys.StringDict, typeof(string), typeof(string));

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
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();
        }
        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed == Key.LeftShift || e.KeyPressed == Key.RightShift)
            {
                try
                {
                    Util.ToggleLeader();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Exception", ex.Message + "\n" + ex.StackTrace);
                }
            }

            if (e.KeyPressed == Key.E)
            {
                try
                {
                    Util.ToggleLeaderEnd();
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("nonenabled element"))
                    {
                        TaskDialog.Show("Exception", ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }

            if (e.KeyPressed == Key.D)
            {
                try
                {
                    Util.CycleLeaderDistance();
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("nonenabled element"))
                    {
                        TaskDialog.Show("Exception", ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }


        }

        private void BeforeSync(object sender, BeforeExecutedEventArgs arg)
        {
            try
            {
                ElecLoadDMU eldmu = new ElecLoadDMU();
                UpdaterRegistry.DisableUpdater(eldmu.GetUpdaterId());

                BreakerDMU bdmu = new BreakerDMU();
                UpdaterRegistry.DisableUpdater(bdmu.GetUpdaterId());

                DuctFittingDMU dfdmu = new DuctFittingDMU();
                UpdaterRegistry.DisableUpdater(dfdmu.GetUpdaterId());
            }
            catch(Exception ex)
            {
                TaskDialog.Show("Error Disabling Updaters", ex.ToString(), TaskDialogCommonButtons.Ok);
            }
        }


    }


}
