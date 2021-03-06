﻿using System;
using System.Reflection;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Collections.Generic;

namespace Proficient
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class AddPanel : IExternalApplication
    {
        static string _namespace_prefix = typeof(AddPanel).Namespace + ".";
        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.DocumentOpened
                += new EventHandler<Autodesk.Revit.DB.Events.DocumentOpenedEventArgs>(Application_DocumentOpened);
            application.ViewActivated += Application_ViewActivated;

            application.CreateRibbonTab("Proficient");
            RibbonPanel genrib = application.CreateRibbonPanel("Proficient","General");
            RibbonPanel knrib = application.CreateRibbonPanel("Proficient", "Keynotes");
            RibbonPanel mechrib = application.CreateRibbonPanel("Proficient", "Mechanical");
            RibbonPanel elecrib = application.CreateRibbonPanel("Proficient", "Electrical");
            genrib.Title = "General";
            knrib.Title = "Keynotes";
            mechrib.Title = "Mechanical";
            elecrib.Title = "Electrical";

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            
            //initialize button data
            PushButtonData button1data = new PushButtonData("cmdflip","Flip Element", thisAssemblyPath, "Proficient.FlipElements");
            PushButtonData button2data = new PushButtonData("cmdpipespace", "Space\nPipes", thisAssemblyPath, "Proficient.PipeSpacer");
            PushButtonData button3data = new PushButtonData("cmdreloadkn", "Reload\nKeynotes", thisAssemblyPath, "Proficient.KeynoteReload");
            PushButtonData button4data = new PushButtonData("cmdflipplane", "Flip Workplane", thisAssemblyPath, "Proficient.FlipWorkPlane");
            PushButtonData button5data = new PushButtonData("cmdchangecallout", "Change Callout\nReference", thisAssemblyPath, "Proficient.ChangeCalloutRef");
            PushButtonData button6data = new PushButtonData("cmdcombinetext", "Combine\nText", thisAssemblyPath, "Proficient.CombineText");
            PushButtonData button7data = new PushButtonData("cmdlaunchduct", "Launch\nDuctulator", thisAssemblyPath, "Proficient.DuctLauncher");
            PushButtonData button8data = new PushButtonData("cmdlaunchkn", "Open\nKeynotes", thisAssemblyPath, "Proficient.KNXLLauncher");
            PushButtonData button9data = new PushButtonData("cmdstg", "Edit\nSettings", thisAssemblyPath, "Proficient.EditSettings");
            PushButtonData button10data = new PushButtonData("cmdelplc", "Element\nPlacer", thisAssemblyPath, "Proficient.ElementPlacer");
            PushButtonData button11data = new PushButtonData("cmdtextleader", "Add Text\nWith Leader", thisAssemblyPath, "Proficient.TextLeader");
            PushButtonData button12data = new PushButtonData("cmdaddleader", "Add\nLeader", thisAssemblyPath, "Proficient.AddLeader");
            PushButtonData button13data = new PushButtonData("cmdflattenText", "Flatten\nText", thisAssemblyPath, "Proficient.FlattenText");
            PushButtonData button14data = new PushButtonData("cmdducttag", "Tag\nDucts", thisAssemblyPath, "Proficient.DuctTag");
            PushButtonData button15data = new PushButtonData("cmdpanelcheck", "Panel\nChecker", thisAssemblyPath, "Proficient.PanelUtil");
            PushButtonData button16data = new PushButtonData("cmdexcelassign", "Excel Assigner\n(beta)", thisAssemblyPath, "Proficient.ExcelAssign");
            PushButtonData button17data = new PushButtonData("cmdknutil", "Keynote\nUtility", thisAssemblyPath, "Proficient.KeynoteUtil");
            PushButtonData button18data = new PushButtonData("cmddampertoggle", "Damper\nToggle", thisAssemblyPath, "Proficient.DamperToggle");

            SplitButtonData sbdata = new SplitButtonData("splttxttools", "Text Tools");

            button1data.Image = NewBitmapImage("flipel.png");
            button4data.Image = NewBitmapImage("flipwp.png");

            //create ribbon
            RibbonButton settingsbtn = genrib.AddItem(button9data) as RibbonButton;
            SplitButton txtsplit = genrib.AddItem(sbdata) as SplitButton;
            RibbonButton calloutbutton = genrib.AddItem(button5data) as RibbonButton;
            RibbonButton elplcbutton = genrib.AddItem(button10data) as RibbonButton;
            IList<RibbonItem> stackedGroup1 = genrib.AddStackedItems(button1data, button4data);
            RibbonButton xl2Revitbutton = genrib.AddItem(button16data) as RibbonButton;
            RibbonButton knbutton = knrib.AddItem(button3data) as RibbonButton;
            RibbonButton xlbutton = knrib.AddItem(button8data) as RibbonButton;
            RibbonButton knutilbtn = knrib.AddItem(button17data) as RibbonButton;
            RibbonButton pipebutton = mechrib.AddItem(button2data) as RibbonButton;
            RibbonButton ducttagbtn = mechrib.AddItem(button14data) as RibbonButton;
            RibbonButton ductbutton = mechrib.AddItem(button7data) as RibbonButton;
            RibbonButton damperbtn = mechrib.AddItem(button18data) as RibbonButton;
            RibbonButton elpanelbtn = elecrib.AddItem(button15data) as RibbonButton;

            PushButton cmbtxtbutton = txtsplit.AddPushButton(button6data);
            PushButton txtldrbtn = txtsplit.AddPushButton(button11data);
            PushButton addldrbtn = txtsplit.AddPushButton(button12data);
            PushButton flattxtbtn = txtsplit.AddPushButton(button13data);

            //add images
            knbutton.LargeImage = NewBitmapImage("reload.png");
            pipebutton.LargeImage = NewBitmapImage("spacepipe.png");
            calloutbutton.LargeImage = NewBitmapImage("callout.png");
            cmbtxtbutton.LargeImage = NewBitmapImage("combine.png");
            ductbutton.LargeImage = NewBitmapImage("duct.png");
            xlbutton.LargeImage = NewBitmapImage("knxl.png");
            settingsbtn.LargeImage = NewBitmapImage("wkst.png");
            elplcbutton.LargeImage = NewBitmapImage("elplc.png");
            txtldrbtn.LargeImage = NewBitmapImage("leadertext.png");
            addldrbtn.LargeImage = NewBitmapImage("addleader.png");
            flattxtbtn.LargeImage = NewBitmapImage("flattentext.png");
            ducttagbtn.LargeImage = NewBitmapImage("tagduct.png");
            elpanelbtn.LargeImage = NewBitmapImage( "elecpanel.png");
            xl2Revitbutton.LargeImage = NewBitmapImage("xl2rvt.png");
            knutilbtn.LargeImage = NewBitmapImage("keynoteutil.png");
            damperbtn.LargeImage = NewBitmapImage("damper.png");

            return Result.Succeeded;
        }

        BitmapImage NewBitmapImage(string imageName)
        {
            Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(_namespace_prefix + "images." + imageName);
            BitmapImage img = new BitmapImage();

            img.BeginInit();
            img.StreamSource = s;
            img.EndInit();

            return img;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // nothing to clean up in this simple case
            application.ControlledApplication.DocumentOpened -= Application_DocumentOpened;
            application.ViewActivated -= Application_ViewActivated;
            return Result.Succeeded;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private void Application_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs args)
        {
            if (args.Document.IsWorkshared)
            {
                Document doc = args.Document;
                WorksetTable wst = doc.GetWorksetTable();

                FilteredWorksetCollector wscol = new FilteredWorksetCollector(doc);
                Workset workset = wscol.FirstOrDefault<Workset>(e => e.Name.Equals(Properties.Settings.Default.workset)) as Workset;

                Transaction transaction = new Transaction(doc, "Change Workset");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    wst.SetActiveWorksetId(workset.Id);
                    transaction.Commit();
                }
            }
        }

        public static void Application_ViewActivated(object sender, EventArgs args)
        {
            UIApplication uiapp = sender as UIApplication;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view = doc.ActiveView;
            
            WorksetTable wst = doc.GetWorksetTable();
            FilteredWorksetCollector wscol = new FilteredWorksetCollector(doc);
            string viewname = view.Name;
            String viewsub = view.GetParameters("MEI Discipline-Sub").Count() > 0 ? view.GetParameters("MEI Discipline-Sub")[0].AsString() : "";

            if (viewname.ToLower().Contains("enlarged") || viewsub.ToLower().Contains("enlarged"))
            {
                if (doc.IsWorkshared && Properties.Settings.Default.switchenlarged)
                {
                    string enlWkst = Properties.Settings.Default.workset[0] == 'M' ? "M-Enlarged Plans" : "E-Enlarged Plans";
                    Workset enlWorkset = wscol.FirstOrDefault<Workset>(e => e.Name.Equals(enlWkst)) as Workset;

                    Transaction transaction = new Transaction(doc, "Change Workset");
                    if (transaction.Start() == TransactionStatus.Started)
                    {
                        wst.SetActiveWorksetId(enlWorkset.Id);
                        transaction.Commit();
                    }
                }
            }
            else if ((viewname.ToLower().Contains("site") || viewsub.ToLower().Contains("site")) && Properties.Settings.Default.workset[0] == 'E')
            {
                if (doc.IsWorkshared && Properties.Settings.Default.switchenlarged)
                {
                    string siteWkst = "E-Site";
                    Workset siteWorkset = wscol.FirstOrDefault<Workset>(e => e.Name.Equals(siteWkst)) as Workset;

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
                Workset workset = wscol.FirstOrDefault<Workset>(e => e.Name.Equals(Properties.Settings.Default.workset)) as Workset;

                Transaction transaction = new Transaction(doc, "Change Workset");
                if (transaction.Start() == TransactionStatus.Started)
                {
                    wst.SetActiveWorksetId(workset.Id);
                    transaction.Commit();
                }
            }


        }
    }


}
