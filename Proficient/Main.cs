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

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Main : IExternalApplication
    {
        public static Main Instance { get; set; }
        public static Settings Settings { get; set; }
        public RibbonButton suppressSchWarning;
        public Result OnStartup(UIControlledApplication app)
        {
            Instance = this;
            InitializeSettings();

            AddEventListeners(app);
            CreateRibbon(app);

            //add external resource servers for keynotes
            ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.ExternalResourceService).AddServer(new ExternalResourceDBServer());
            ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.ExternalResourceUIService).AddServer(new ExternalResourceUIServer());

            //check toolbar version
            string thisVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string currentVersion = FileVersionInfo.GetVersionInfo(Names.File.ServerDll).FileVersion.ToString();
            if (thisVersion != currentVersion)
            {
                Util.BalloonTip("Proficient", "New version of Proficient available.\nClick here, close Revit, and run the installer.", "Proficient Out Of Date", Names.File.ServerAddinFolder);
            }

            return Result.Succeeded;
        }

        public void AddEventListeners(UIControlledApplication app)
        {
            app.ControlledApplication.DocumentOpened += new EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(Application_DocumentOpened);
            app.ViewActivated += Application_ViewActivated;
            app.DialogBoxShowing += new EventHandler<DialogBoxShowingEventArgs>(Application_DialogBoxShowing);
            app.ControlledApplication.DocumentPrinting += new EventHandler<Autodesk.Revit.DB.Events.DocumentPrintingEventArgs>(Application_DocumentPrinting);
            app.ControlledApplication.DocumentPrinted += new EventHandler<Autodesk.Revit.DB.Events.DocumentPrintedEventArgs>(Application_DocumentPrinted);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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

        public void CreateRibbon(UIControlledApplication application)
        {
            string thisAssyPath = Assembly.GetExecutingAssembly().Location;
            application.CreateRibbonTab("Proficient");

            #region add panels
            RibbonPanel genrib = application.CreateRibbonPanel("Proficient", "General");
            RibbonPanel togrib = application.CreateRibbonPanel("Proficient", "Toggles");
            RibbonPanel filtrib = application.CreateRibbonPanel("Proficient", "Filters");
            RibbonPanel knrib = application.CreateRibbonPanel("Proficient", "Keynotes");
            RibbonPanel mechrib = application.CreateRibbonPanel("Proficient", "Mechanical");
            RibbonPanel elecrib = application.CreateRibbonPanel("Proficient", "Electrical");
            genrib.Title = "General";
            togrib.Title = "Toggles";
            knrib.Title = "Keynotes";
            mechrib.Title = "Mechanical";
            elecrib.Title = "Electrical";
            #endregion

            #region add buttons
            RibbonButton settings = genrib.AddItem(
                new PushButtonData("settings", "Edit\nSettings", thisAssyPath, "Proficient.EditSettings")) as RibbonButton;
            SplitButton txtSplit = genrib.AddItem(
                new SplitButtonData("txttools", "Text Tools")) as SplitButton;
            RibbonButton xl2Rvt = genrib.AddItem(
                new PushButtonData("excelassign", "Excel\nAssigner", thisAssyPath, "Proficient.ExcelAssign")) as RibbonButton;
            RibbonButton elPlacer = genrib.AddItem(
                new PushButtonData("elplacer", "Element\nPlacer", thisAssyPath, "Proficient.ElementPlacer")) as RibbonButton;
            RibbonButton wipeMark = genrib.AddItem(
                new PushButtonData("wipemark", "Wipe\nMark", thisAssyPath, "Proficient.WipeMark")) as RibbonButton;
            RibbonButton calloutRef = genrib.AddItem(
                new PushButtonData("calloutref", "Change Callout\nReference", thisAssyPath, "Proficient.ChangeCalloutRef")) as RibbonButton;
            IList<RibbonItem> fliptools = genrib.AddStackedItems(
                new PushButtonData("flipel", "Flip Element", thisAssyPath, "Proficient.FlipElements"),
                new PushButtonData("flipplane", "Flip Workplane", thisAssyPath, "Proficient.FlipWorkPlane"));

            IList<RibbonItem> toggletools = togrib.AddStackedItems(
                new PushButtonData("toggleenlwkst", "Enlarged Workset", thisAssyPath, "Proficient.ToggleEnlWkst"),
                new PushButtonData("toggledesignnotes", "Design Annotations", thisAssyPath, "Proficient.ToggleDesignAnno"),
                new PushButtonData("toggleschwarning", "Schedule Warning", thisAssyPath, "Proficient.SuppressSchWarning"));

            IList<RibbonItem> filttools = filtrib.AddStackedItems(
                new PushButtonData("catfilt", "Category", thisAssyPath, "Proficient.CategoryFilter"),
                new PushButtonData("famfilt", "Family", thisAssyPath, "Proficient.FamilyFilter"),
                new PushButtonData("typefilt", "Type", thisAssyPath, "Proficient.TypeFilter"));

            RibbonButton reloadKn = knrib.AddItem(
                new PushButtonData("reloadkn", "Reload\nKeynotes", thisAssyPath, "Proficient.KNReload")) as RibbonButton;
            RibbonButton openKn = knrib.AddItem(
                new PushButtonData("launchkn", "Open\nKeynotes", thisAssyPath, "Proficient.KNLauncher")) as RibbonButton;
            RibbonButton knUtil = knrib.AddItem(
                new PushButtonData("knutil", "Keynote\nUtility", thisAssyPath, "Proficient.KeynoteUtil")) as RibbonButton;

            RibbonButton ductTag = mechrib.AddItem(
                new PushButtonData("ducttag", "Tag\nDucts", thisAssyPath, "Proficient.DuctTag")) as RibbonButton;
            RibbonButton pipeTag = mechrib.AddItem(
                new PushButtonData("pipetag", "Tag\nPipes", thisAssyPath, "Proficient.PipeTag")) as RibbonButton;
            RibbonButton pipeSpacer = mechrib.AddItem(
                new PushButtonData("pipespacer", "Space\nPipes", thisAssyPath, "Proficient.PipeSpacer")) as RibbonButton;
            RibbonButton setFitUpDn = mechrib.AddItem(
                new PushButtonData("setfitupdn", "Set Fitting\nUp/Down", thisAssyPath, "Proficient.SetFittingUpDn")) as RibbonButton;
            RibbonButton damperToggle = mechrib.AddItem(
                new PushButtonData("dampertoggle", "Damper\nToggle", thisAssyPath, "Proficient.DamperToggle")) as RibbonButton;
            RibbonButton elbowToggle = mechrib.AddItem(
                new PushButtonData("elbowtoggle", "Duct Elbow\nToggle", thisAssyPath, "Proficient.DuctElbowToggle")) as RibbonButton;
            RibbonButton ductulator = mechrib.AddItem(
                new PushButtonData("ductulator", "Launch\nDuctulator", thisAssyPath, "Proficient.DuctLauncher")) as RibbonButton;

            RibbonButton elpanelbtn = elecrib.AddItem(
                new PushButtonData("panelcheck", "Panel\nChecker", thisAssyPath, "Proficient.PanelUtil")) as RibbonButton;

            PushButton cmbtxt = txtSplit.AddPushButton(
                new PushButtonData("combinetxt", "Combine\nText", thisAssyPath, "Proficient.CombineText"));
            PushButton txtldr = txtSplit.AddPushButton(
                new PushButtonData("textleader", "Add Text\nWith Leader", thisAssyPath, "Proficient.TextLeader"));
            PushButton addldr = txtSplit.AddPushButton(
                new PushButtonData("addleader", "Add\nLeader", thisAssyPath, "Proficient.AddLeader"));
            PushButton flattxt = txtSplit.AddPushButton(
                new PushButtonData("flattentext", "Flatten\nText", thisAssyPath, "Proficient.FlattenText"));
            #endregion

            #region add images
            settings.LargeImage = NewBitmapImage("wkst");
            cmbtxt.LargeImage = NewBitmapImage("combine");
            txtldr.LargeImage = NewBitmapImage("leadertext");
            addldr.LargeImage = NewBitmapImage("addleader");
            flattxt.LargeImage = NewBitmapImage("flattentext");
            xl2Rvt.LargeImage = NewBitmapImage("xl2rvt");
            elPlacer.LargeImage = NewBitmapImage("elplc");
            wipeMark.LargeImage = NewBitmapImage("wipe");
            calloutRef.LargeImage = NewBitmapImage("callout");
            (fliptools[0] as PushButton).Image = NewBitmapImage("flipel");
            (fliptools[1] as PushButton).Image = NewBitmapImage("flipwp");

            (toggletools[0] as PushButton).Image = NewBitmapImage("enlwkst");
            (toggletools[1] as PushButton).Image = NewBitmapImage("notes");
            (toggletools[2] as PushButton).Image = NewBitmapImage("msg");

            (filttools[0] as PushButton).Image = NewBitmapImage("cat");
            (filttools[1] as PushButton).Image = NewBitmapImage("fam");
            (filttools[2] as PushButton).Image = NewBitmapImage("type");

            reloadKn.LargeImage = NewBitmapImage("reload");
            openKn.LargeImage = NewBitmapImage("knxl");
            knUtil.LargeImage = NewBitmapImage("keynoteutil");

            ductTag.LargeImage = NewBitmapImage("tagduct");
            pipeTag.LargeImage = NewBitmapImage("tagpipe");
            pipeSpacer.LargeImage = NewBitmapImage("spacepipe");
            setFitUpDn.LargeImage = NewBitmapImage("updown");
            damperToggle.LargeImage = NewBitmapImage("damper");
            elbowToggle.LargeImage = NewBitmapImage("ducttoggle");
            ductulator.LargeImage = NewBitmapImage("duct");

            elpanelbtn.LargeImage = NewBitmapImage("elecpanel");
            #endregion

            #region add tooltips

            settings.ToolTip = "Change Proficient settings";
            cmbtxt.ToolTip = "Combine multiple text objects into one";
            txtldr.ToolTip = "Add text object with leader";
            addldr.ToolTip = "Add leader to existing text object";
            flattxt.ToolTip = "Remove all carriage returns from text object";
            xl2Rvt.ToolTip = "Assign parameters to family types or instances from Excel document";

            #endregion
        }

        public static BitmapImage NewBitmapImage(string imageName)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Main).Namespace + ".images." + imageName + ".png");
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
