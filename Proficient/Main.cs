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


namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Main : IExternalApplication
    {
        public static Main Instance { get; set; }
        public static Settings Settings { get; set; }
        public RibbonButton suppressSchWarning;
        public static bool Leader = true;
        public Result OnStartup(UIControlledApplication app)
        {
            Instance = this;
            InitializeSettings();
            
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.TagByCategory)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ElementKeynote)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.UserKeynote)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);
            app.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.RoomTag)).BeforeExecuted +=
                new EventHandler<BeforeExecutedEventArgs>(BeforeTag);

            AddEventListeners(app);
            CreateRibbon(app);

            //add external resource servers for keynotes
            ExternalServiceRegistry
                .GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService)
                .AddServer(new Keynotes.ExternalResourceDBServer());
            ExternalServiceRegistry.
                GetService(ExternalServices.BuiltInExternalServices.ExternalResourceUIService)
                .AddServer(new Keynotes.ExternalResourceUIServer());

            //check toolbar version
            string thisVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion.ToString();
            if (thisVersion != currentVersion)
            {
                Util.BalloonTip("Proficient", "New version of Proficient available.\nClick here, close Revit, and run the installer.", "Proficient Out Of Date", Names.File.ServerAddinFolder);
            }

            return Result.Succeeded;
        }

        private void BeforeTag(object sender, BeforeExecutedEventArgs arg)
        {
            _listener?.UnHookKeyboard();
            _listener = new KeyListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.HookKeyboard();
        }

        public bool TagLeader { get; set; }
        public bool AttachedLeader { get; set; }

        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (e.KeyPressed == Key.LeftShift || e.KeyPressed == Key.RightShift)
            {
                try
                {
                    Util.ToggleLeader();
                }
                catch(Exception ex)
                {
                    TaskDialog.Show("Exception", ex.Message + "\n" + ex.StackTrace);
                }
            }
            
            if(e.KeyPressed == Key.E)
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

        private KeyListener _listener;

        public void AddEventListeners(UIControlledApplication app)
        {
            app.ControlledApplication.DocumentOpened += new EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(Application_DocumentOpened);
            app.ViewActivated += Application_ViewActivated;
            app.DialogBoxShowing += new EventHandler<DialogBoxShowingEventArgs>(Application_DialogBoxShowing);
            app.ControlledApplication.DocumentPrinting += new EventHandler<Autodesk.Revit.DB.Events.DocumentPrintingEventArgs>(Application_DocumentPrinting);
            app.ControlledApplication.DocumentPrinted += new EventHandler<Autodesk.Revit.DB.Events.DocumentPrintedEventArgs>(Application_DocumentPrinted);
            app.Idling += App_Idling;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private void App_Idling(object sender, IdlingEventArgs e)
        {
            _listener?.UnHookKeyboard();
            UIApplication uiapp = sender as UIApplication;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            uiapp.GetRibbonPanels("Proficient").Where(pnl => pnl.Name == "Electrical").First().Visible = app.IsElectricalEnabled;
            uiapp.GetRibbonPanels("Proficient").Where(pnl => pnl.Name == "Mechanical").First().Visible = app.IsMechanicalEnabled;

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

        public void CreateRibbon(UIControlledApplication app)
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
            AddRibbonButton(genRib, "EditSettings", "Edit\nSettings", "wkst", "Change Proficient settings");
            SplitButton txtSplit = genRib.AddItem(new SplitButtonData("txttools", "Text Tools")) as SplitButton;
            AddRibbonButton(genRib, "ExcelAssign", "Excel\nAssigner", "xl2rvt", "Assign parameters to family types or instances from Excel document");
            AddRibbonButton(genRib, "ElementPlacer", "Element\nPlacer", "elplc", "");
            AddRibbonButton(genRib, "OpenSectionView", "Open\nSection", "section", "");
            AddRibbonButton(genRib, "ChangeCalloutRef", "Change Callout\nReference", "callout", "");
            AddRibbonButton(genRib, "AddLeader", "Add\nLeader", "leader", "");
            IList<RibbonItem> scheds = genRib.AddStackedItems(
               new PushButtonData("schwidth", "Schedule Width", asLoc, "Proficient.MatchSchWidth"),
               new PushButtonData("schalign", "Align Schedules", asLoc, "Proficient.AlignSchedule"));
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
            AddRibbonButton(mechRib, "Mech.DuctLauncher", "Launch\nDuctulator", "duct", "");

            AddRibbonButton(elecRib, "Elec.PanelUtil", "Panel\nUtility", "elecpanel", "");

            PushButton cmbtxt = txtSplit.AddPushButton(
                new PushButtonData("combinetxt", "Combine\nText", asLoc, "Proficient.CombineText"));
            PushButton txtldr = txtSplit.AddPushButton(
                new PushButtonData("textleader", "Add Text\nWith Leader", asLoc, "Proficient.TextLeader"));
            PushButton flattxt = txtSplit.AddPushButton(
                new PushButtonData("flattentext", "Flatten\nText", asLoc, "Proficient.FlattenText"));
            #endregion

            #region add images
            cmbtxt.LargeImage = NewBitmapImage("combine");
            txtldr.LargeImage = NewBitmapImage("leadertext");
            flattxt.LargeImage = NewBitmapImage("flattentext");

            (scheds[0] as PushButton).Image = NewBitmapImage("width");
            (scheds[1] as PushButton).Image = NewBitmapImage("align");

            (flips[0] as PushButton).Image = NewBitmapImage("flipel");
            (flips[1] as PushButton).Image = NewBitmapImage("flipwp");

            (toggles[0] as PushButton).Image = NewBitmapImage("enlwkst");
            (toggles[1] as PushButton).Image = NewBitmapImage("notes");
            (toggles[2] as PushButton).Image = NewBitmapImage("msg");

            (filters[0] as PushButton).Image = NewBitmapImage("cat");
            (filters[1] as PushButton).Image = NewBitmapImage("fam");
            (filters[2] as PushButton).Image = NewBitmapImage("type");

            (mechTags[0] as PushButton).Image = NewBitmapImage("tagduct");
            (mechTags[1] as PushButton).Image = NewBitmapImage("tagpipe");
            (mechTags[2] as PushButton).Image = NewBitmapImage("wipe");

            (mechTogs[0] as PushButton).Image = NewBitmapImage("ducttoggle");
            (mechTogs[1] as PushButton).Image = NewBitmapImage("damper");
            (mechTogs[2] as PushButton).Image = NewBitmapImage("togupdown");

            #endregion

            #region add tooltips

            cmbtxt.ToolTip = "Combine multiple text objects into one";
            txtldr.ToolTip = "Add text object with leader";
            flattxt.ToolTip = "Remove all carriage returns from text object";

            #endregion
        }

        public static void AddRibbonButton(RibbonPanel panel, string className, string title, string imgName, string tooltip)
        {
            string cmdName = title.Replace("\n", " ");
            string aPath = Assembly.GetExecutingAssembly().Location;
            string fullName = "Proficient." + className;
            RibbonButton rb = panel.AddItem(new PushButtonData(cmdName, title, aPath, fullName)) as RibbonButton;
            if(imgName != String.Empty)
            {
                rb.LargeImage = NewBitmapImage(imgName);
            }
            rb.ToolTip = tooltip;
        }

        public static BitmapImage NewBitmapImage(string imgName)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Main).Namespace + ".images." + imgName + ".png");
            BitmapImage img = new BitmapImage();

            img.BeginInit();
            img.StreamSource = s;
            img.EndInit();

            return img;
        }

        public Result OnShutdown(UIControlledApplication app)
        {
            app.ControlledApplication.DocumentOpened -= Application_DocumentOpened;
            app.ViewActivated -= Application_ViewActivated;
            app.DialogBoxShowing -= Application_DialogBoxShowing;
            app.ControlledApplication.DocumentPrinting -= Application_DocumentPrinting;
            app.ControlledApplication.DocumentPrinted -= Application_DocumentPrinted;
            app.Idling -= App_Idling;
            return Result.Succeeded;
        }

        private void Application_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
        {
            if (args.Document.IsWorkshared)
            {
                Document doc = args.Document;
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
        }

        public void Application_ViewActivated(object sender, EventArgs args)
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

        public void Application_DialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
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


        public void Application_DocumentPrinting(object sender, Autodesk.Revit.DB.Events.DocumentPrintingEventArgs args)
        {
            if (Settings.hideDesignNotes)
            {
                Document doc = args.Document;
                List<ElementId> printViews = args.GetViewElementIds().ToList();
                HideDesignNotes(doc, printViews);
            }
        }

        public void Application_DocumentPrinted(object sender, Autodesk.Revit.DB.Events.DocumentPrintedEventArgs args)
        {
            if (Settings.hideDesignNotes)
            {
                Document doc = args.Document;
                UnhideDesignNotes(doc);
            }
        }

        public void InitializeSettings()
        {
            string configPath = Names.File.UserSettings;
            string configTxt;
            Settings = new Settings();

            if (File.Exists(configPath))
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

        private Dictionary<View, ICollection<ElementId>> DesignNoteViews = new Dictionary<View, ICollection<ElementId>>();
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


}
