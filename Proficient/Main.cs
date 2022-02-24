using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Proficient.Elec;
using Proficient.Utilities;


namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Main : IExternalApplication
    {
        public static Main Instance { get; set; }
        public static Settings Settings { get; set; }
        public static UIControlledApplication app;
        public static readonly Guid appId = new Guid("339af853-36e4-461f-9171-c5dceda4e721");
        private static ElecLoadDMU elecLoadDMU;

        private Dictionary<View, ICollection<ElementId>> DesignNoteViews = new Dictionary<View, ICollection<ElementId>>();
        private KeyListener _listener;
        public static Forms.ProficientPane Pane;
        public static DockablePaneId PaneId { get; set; }

        public Result OnStartup(UIControlledApplication uicApp)
        {
            Instance = this;
            app = uicApp;
            InitializeSettings();
            AddCommandBindings();
            AddEventListeners();
            CreateRibbon();
            AddExternalService();
            CheckToolbarVersion();
            if(app.ControlledApplication.VersionNumber != "2022")
            {
                RegisterElecLoadDMU();
            }
            //RegisterDockablePane();


            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication uicApp)
        {
            app.ControlledApplication.DocumentOpened -= App_DocumentOpened;
            app.ViewActivated -= App_ViewActivated;
            app.DialogBoxShowing -= App_DialogBoxShowing;
            app.ControlledApplication.DocumentPrinting -= App_DocumentPrinting;
            app.ControlledApplication.DocumentPrinted -= App_DocumentPrinted;
            app.Idling -= App_Idling;
            return Result.Succeeded;
        }

        public void AddEventListeners()
        {
            app.ControlledApplication.DocumentOpened += App_DocumentOpened;
            app.ViewActivated += App_ViewActivated;
            app.DialogBoxShowing += App_DialogBoxShowing;
            app.ControlledApplication.DocumentPrinting += App_DocumentPrinting;
            app.ControlledApplication.DocumentPrinted += App_DocumentPrinted;
            app.ApplicationClosing += App_ApplicationClosing;

            app.Idling += App_Idling;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private void App_ApplicationClosing(object sender, ApplicationClosingEventArgs e)
        {
            string thisVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion;
            if (thisVersion != currentVersion)
            {
                Process.Start(Names.File.SilentUpdateExe);
            }
        }
        private void App_Idling(object sender, IdlingEventArgs e)
        {
            _listener?.UnHookKeyboard();
            UIApplication uiapp = sender as UIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            uiapp.GetRibbonPanels("Proficient").Where(pnl => pnl.Name == "Electrical").First().Visible = app.IsElectricalEnabled;
            uiapp.GetRibbonPanels("Proficient").Where(pnl => pnl.Name == "Mechanical").First().Visible = app.IsMechanicalEnabled;

        }
        private void App_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
        {
            if (args.Document.IsWorkshared)
            {
                Document doc = args.Document;
                doc.DocumentClosing += App_DocumentClosing;
                WorksetTable wst = doc.GetWorksetTable();

                FilteredWorksetCollector wscol = new FilteredWorksetCollector(doc);
                Workset workset = wscol.FirstOrDefault(e => e.Name.Equals(Settings.defWorkset));

                Transaction transaction = new Transaction(doc, "Change Workset");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    wst.SetActiveWorksetId(workset.Id);
                    transaction.Commit();
                }
            }

            var mec = new FilteredElementCollector(args.Document).OfCategory(BuiltInCategory.OST_MechanicalEquipment).Where(e => e is FamilyInstance);
            ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_MechanicalEquipment);

            foreach (Element el in mec)
            {
                Parameter par = el.LookupParameter(Names.Parameter.DisplaySeparation);
                
                if (par != null)
                {
                    UpdaterRegistry.AddTrigger(elecLoadDMU.GetUpdaterId(), f, Element.GetChangeTypeParameter(par));
                }
            }
        }
        private void App_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {
            if (e.Document.Application.Documents.Size == 1)
            {
                e.Document.DocumentClosing -= App_DocumentClosing;
                Pane.WebView.Dispose();
            }
            throw new NotImplementedException();
        }
        public void App_ViewActivated(object sender, EventArgs args)
        {
            UIApplication uiapp = sender as UIApplication;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;

            WorksetTable wst = doc.GetWorksetTable();
            FilteredWorksetCollector wscol = new FilteredWorksetCollector(doc);
            string viewname = view.Name;
            String viewsub = view.GetParameters(Names.Parameter.ViewSubdiscipline).Count() > 0 ? view.GetParameters(Names.Parameter.ViewSubdiscipline)[0].AsString() : string.Empty;

            if (viewname.ToLower().Contains("enlarged") || viewsub.ToLower().Contains("enlarged"))
            {
                if (doc.IsWorkshared && Settings.switchEnlarged)
                {
                    string enlWkst = Settings.defWorkset[0] == 'M' ? Names.Workset.MechEnlarged : Names.Workset.ElecEnlarged;
                    Workset enlWorkset = wscol.FirstOrDefault(e => e.Name.Equals(enlWkst));

                    Transaction transaction = new Transaction(doc, "Change Workset");
                    if (transaction.Start() == TransactionStatus.Started)
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
                    if (transaction.Start() == TransactionStatus.Started)
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
                if (transaction.Start() == TransactionStatus.Started)
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
        }
        public void App_DocumentPrinting(object sender, Autodesk.Revit.DB.Events.DocumentPrintingEventArgs args)
        {
            if (Settings.hideDesignNotes)
            {
                Document doc = args.Document;
                List<ElementId> printViews = args.GetViewElementIds().ToList();
                HideDesignNotes(doc, printViews);
            }
        }
        public void App_DocumentPrinted(object sender, Autodesk.Revit.DB.Events.DocumentPrintedEventArgs args)
        {
            if (Settings.hideDesignNotes)
            {
                Document doc = args.Document;
                UnhideDesignNotes(doc);
            }
        }


        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string directoryDLLs = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string pathAssembly = Path.Combine(directoryDLLs, args.Name);
            if (File.Exists(pathAssembly))
            {
                return Assembly.LoadFrom(pathAssembly);
            }

            // assembly cannot be resolved
            return null;
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
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.TagByCategory)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ElementKeynote)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.UserKeynote)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.RoomTag)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
        }
        

        public void CreateRibbon()
        {
            string asLoc = Assembly.GetExecutingAssembly().Location;
            app.CreateRibbonTab("Proficient");

            #region add panels
            RibbonPanel genRib = app.CreateRibbonPanel("Proficient", "General");
            RibbonPanel togRib = app.CreateRibbonPanel("Proficient", "Toggles");
            RibbonPanel filtrib = app.CreateRibbonPanel("Proficient", "Filters");
            RibbonPanel knRib = app.CreateRibbonPanel("Proficient", "Keynotes");
            RibbonPanel mechRib = app.CreateRibbonPanel("Proficient", "Mechanical");
            RibbonPanel elecRib = app.CreateRibbonPanel("Proficient", "Electrical");
            genRib.Title = "General";
            togRib.Title = "Toggles";
            knRib.Title = "Keynotes";
            mechRib.Title = "Mechanical";
            elecRib.Title = "Electrical";
            #endregion

            #region add buttons
            AddRibbonButton(genRib, "EditSettings", "Edit\nSettings", "settings", "Change Proficient settings");
            SplitButton txtSplit = genRib.AddItem(new SplitButtonData("txttools", "Text Tools")) as SplitButton;
            AddRibbonButton(genRib, "ExcelAssign", "Excel\nAssigner", "xl2rvt", "Assign parameters to family types or instances from Excel document");
            AddRibbonButton(genRib, "ElementPlacer", "Element\nPlacer", "elplc", "");
            AddRibbonButton(genRib, "OpenSectionView", "Open\nSection", "section", "");
            AddRibbonButton(genRib, "ChangeCalloutRef", "Change Callout\nReference", "callout", "");
            AddRibbonButton(genRib, "AddLeader", "Add\nLeader", "leader", "");
            IList<RibbonItem> scheds = genRib.AddStackedItems(
               new PushButtonData("schwidth", "Schedule Width", asLoc, "Proficient.MatchSchWidth"),
               new PushButtonData("schalignleft", "Align Sched L", asLoc, "Proficient.AlignScheduleL"),
               new PushButtonData("schalignright", "Align Sched R", asLoc, "Proficient.AlignScheduleR"));
            IList<RibbonItem> flips = genRib.AddStackedItems(
                new PushButtonData("flipel", "Flip Element", asLoc, "Proficient.FlipElements"),
                new PushButtonData("flipplane", "Flip Workplane", asLoc, "Proficient.FlipWorkPlane"));
            IList<RibbonItem> toggles = togRib.AddStackedItems(
                new PushButtonData("toggleenlwkst", "Enlarged Workset", asLoc, "Proficient.ToggleEnlWkst"),
                new PushButtonData("toggledesignnotes", "Design Annotations", asLoc, "Proficient.ToggleDesignAnno"),
                new PushButtonData("toggleschwarning", "Schedule Warning", asLoc, "Proficient.SuppressSchWarning"));
            IList<RibbonItem> filters = filtrib.AddStackedItems(
                new PushButtonData("catfilt", "Category", asLoc, "Proficient.CategoryFilter"),
                new PushButtonData("famfilt", "Family", asLoc, "Proficient.FamilyFilter"),
                new PushButtonData("typefilt", "Type", asLoc, "Proficient.TypeFilter"));
            //AddRibbonButton(genRib, "ShowPane", "\nShow\nPane", "", "");

            AddRibbonButton(knRib, "Keynotes.KNReload", "Reload\nKeynotes", "reload", "");
            AddRibbonButton(knRib, "Keynotes.KNLauncher", "Open\nKeynotes", "knxl", "");
            AddRibbonButton(knRib, "Keynotes.KeynoteUtil", "Keynotes\nUtility", "keynoteutil", "");

            IList<RibbonItem> mechTags = mechRib.AddStackedItems(
                new PushButtonData("tagduct", "Tag Ducts", asLoc, "Proficient.Mech.DuctTag"),
                new PushButtonData("tagpipe", "Tag Pipes", asLoc, "Proficient.Mech.PipeTag"),
                new PushButtonData("wipemark", "Wipe Mark", asLoc, "Proficient.Mech.WipeMark"));

            IList<RibbonItem> mechTogs = mechRib.AddStackedItems(
                new PushButtonData("ducttoggle", "Duct Elbow Toggle", asLoc, "Proficient.Mech.DuctElbowToggle"),
                new PushButtonData("dampertoggle", "Damper Toggle", asLoc, "Proficient.Mech.DamperToggle"),
                new PushButtonData("togupdn", "Up/Dn Toggle", asLoc, "Proficient.Mech.ToggleUpDn"));

            AddRibbonButton(mechRib, "Mech.PipeSpacer", "Space\nPipes", "spacepipe", "");
            AddRibbonButton(mechRib, "Mech.HardBreak", "Hard\nBreak", "hardbreak", "");
            AddRibbonButton(mechRib, "Mech.SetFittingUpDn", "Auto-Set\nUp/Dn", "updn", "");
            AddRibbonButton(mechRib, "Mech.DuctLauncher", "Launch\nDuctulator", "duct", "", true);

            AddRibbonButton(elecRib, "Elec.PanelUtil", "Panel\nUtility", "elecpanel", "");

            PushButton cmbtxt = txtSplit.AddPushButton(
                new PushButtonData("combinetxt", "Combine\nText", asLoc, "Proficient.CombineText"));
            PushButton txtldr = txtSplit.AddPushButton(
                new PushButtonData("textleader", "Add Text\nWith Leader", asLoc, "Proficient.TextLeader"));
            PushButton flattxt = txtSplit.AddPushButton(
                new PushButtonData("flattentext", "Flatten\nText", asLoc, "Proficient.FlattenText"));
            #endregion

            #region add images
            cmbtxt.LargeImage = Icons.ScaledIcon("combine",32,32);
            txtldr.LargeImage = Icons.ScaledIcon("leadertext", 32, 32);
            flattxt.LargeImage = Icons.ScaledIcon("flattentext", 32, 32);

            (scheds[0] as PushButton).Image = Icons.ScaledIcon("width", 16, 16);
            (scheds[1] as PushButton).Image = Icons.ScaledIcon("align", 16, 16);
            (scheds[2] as PushButton).Image = Icons.ScaledIcon("alignflip", 16, 16);

            (flips[0] as PushButton).Image = Icons.ScaledIcon("flipel", 16, 16);
            (flips[1] as PushButton).Image = Icons.ScaledIcon("flipwp", 16, 16);

            (toggles[0] as PushButton).Image = Icons.ScaledIcon("enlwkst", 16, 16);
            (toggles[1] as PushButton).Image = Icons.ScaledIcon("notes", 16, 16);
            (toggles[2] as PushButton).Image = Icons.ScaledIcon("msg", 16, 16);

            (filters[0] as PushButton).Image = Icons.ScaledIcon("cat", 16, 16);
            (filters[1] as PushButton).Image = Icons.ScaledIcon("fam", 16, 16);
            (filters[2] as PushButton).Image = Icons.ScaledIcon("type", 16, 16);

            (mechTags[0] as PushButton).Image = Icons.ScaledIcon("tagduct", 16, 16);
            (mechTags[1] as PushButton).Image = Icons.ScaledIcon("tagpipe", 16, 16);
            (mechTags[2] as PushButton).Image = Icons.ScaledIcon("wipe", 16, 16);

            (mechTogs[0] as PushButton).Image = Icons.ScaledIcon("ducttoggle", 16, 16);
            (mechTogs[1] as PushButton).Image = Icons.ScaledIcon("damper", 16, 16);
            (mechTogs[2] as PushButton).Image = Icons.ScaledIcon("togupdown", 16, 16);

            #endregion

            #region add tooltips

            cmbtxt.ToolTip = "Combine multiple text objects into one";
            txtldr.ToolTip = "Add text object with leader";
            flattxt.ToolTip = "Remove all carriage returns from text object";

            #endregion
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
            string thisVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion.ToString();
            if (thisVersion != currentVersion)
            {
                Util.BalloonTip("Proficient", "New version of Proficient available.\nVersion will be updated on Revit close", "Proficient Out Of Date");
            }
        }
        private void RegisterDockablePane()
        {
            PaneId = new DockablePaneId(new Guid("F984D829-98D6-46F7-A35A-B3B8C0B6A55A"));
            Pane = new Forms.ProficientPane();
            app.RegisterDockablePane(PaneId, "Proficient", Pane);
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

        public static void AddRibbonButton(RibbonPanel panel, string className, string title, string imgName, string tooltip)
        {
            string cmdName = title.Replace("\n", " ");
            string aPath = Assembly.GetExecutingAssembly().Location;
            string fullName = $"Proficient.{className}";
            RibbonButton rb = panel.AddItem(new PushButtonData(cmdName, title, aPath, fullName)) as RibbonButton;
            if(imgName != String.Empty)
            {
                rb.LargeImage = Icons.ScaledIcon(imgName, 32, 32);
            }
            rb.ToolTip = tooltip;
        }
        public static void AddRibbonButton(RibbonPanel panel, string className, string title, string imgName, string tooltip, bool zeroBtn)
        {
            string cmdName = title.Replace("\n", " ");
            string aPath = Assembly.GetExecutingAssembly().Location;
            string fullName = $"Proficient.{className}";
            PushButtonData d = new PushButtonData(cmdName, title, aPath, fullName);
            if (zeroBtn)
            {
                d.AvailabilityClassName = "Proficient.CommandAvailability";
            }
            RibbonButton rb = panel.AddItem(d) as RibbonButton;
            if (imgName != String.Empty)
            {
                rb.LargeImage = Icons.ScaledIcon(imgName, 32, 32);
            }
            rb.ToolTip = tooltip;
        }

        private void HideDesignNotes(Document doc, List<ElementId> printViews)
        {
            var textEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_TextNotes)
                .Where(x => x.Name.ToLower().Contains("design"));

            var lineEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Lines)
                .Where(x => (x as CurveElement).LineStyle.Name.ToLower().Contains("design"));

            var dimEl = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Dimensions)
                .Where(x => x.Name.ToLower().Contains("design"));

            List<Element> designEl = new List<Element>();

            designEl.AddRange(textEl);
            designEl.AddRange(lineEl);
            designEl.AddRange(dimEl);

            List<ElementId> printViewsSub = new List<ElementId>();
            printViewsSub.AddRange(printViews);

            foreach (ElementId id in printViews)
            {
                if (doc.GetElement(id) is ViewSheet vs)
                {
                    foreach (ElementId vid in vs.GetAllPlacedViews())
                    {
                        printViewsSub.Add(vid);
                        ElementId pvid = (doc.GetElement(vid) as View).GetPrimaryViewId();
                        if (pvid == ElementId.InvalidElementId) printViewsSub.Add(pvid);
                    }
                }
                else
                {
                    ElementId pvid = (doc.GetElement(id) as View).GetPrimaryViewId();
                    if (pvid != ElementId.InvalidElementId) printViewsSub.Add(pvid);
                }
            }

            var noteViews = designEl
                .Select(x => x.OwnerViewId)
                .Distinct()
                .ToList();
            var noteDependentViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Where(x => noteViews.Contains((x as View).GetPrimaryViewId()))
                .Select(x => x.Id);
            noteViews.AddRange(noteDependentViews);
            var views = printViewsSub.Intersect(noteViews);

            DesignNoteViews.Clear();

            using (Transaction tx = new Transaction(doc, "Hide Design Notes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (ElementId viewId in views)
                    {
                        View curView = doc.GetElement(viewId) as View;
                        ICollection<ElementId> viewDesignEl = designEl
                            .Where(x => x.OwnerViewId == viewId || x.OwnerViewId == curView.GetPrimaryViewId())
                            .Select(x => x.Id)
                            .ToList();
                        DesignNoteViews.Add(curView, viewDesignEl);
                        curView.HideElements(viewDesignEl);
                    }
                }
                tx.Commit();
            }
        }
        private void UnhideDesignNotes(Document doc)
        {
            using (Transaction tx = new Transaction(doc, "Unhide Design Notes"))
            {
                if (tx.Start() == TransactionStatus.Started)
                {
                    foreach (View curView in DesignNoteViews.Keys)
                    {
                        curView.UnhideElements(DesignNoteViews[curView]);
                    }
                }
                tx.Commit();
            }
        }

    }

    public class CommandAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication app, CategorySet cs)
        {
            return true;
        }
    }


}
