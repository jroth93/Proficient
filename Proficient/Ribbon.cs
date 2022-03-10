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
using System.Windows.Input;
using Proficient.Elec;
using Proficient.Utilities;

namespace Proficient
{
    static class Ribbon
    {
        internal static void Create()
        {
            UIControlledApplication app = Main.app;
            string al = Assembly.GetExecutingAssembly().Location;
            string ugUrl = "https://github.com/jroth93/Proficient/wiki/Proficient-User-Guide";
            app.CreateRibbonTab("Proficient");

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

//general panel
            var settingBtn = genRib.AddItem(
                new PushButtonData("EditSettings", "Edit\nSettings", al, "Proficient.EditSettings")
                ) as RibbonButton;
            settingBtn.LargeImage = Icons.ScaledIcon("settings", 32);
            settingBtn.ToolTip = "Change Proficient settings";
            settingBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#edit-settings"));

            var txtSplitBtn = genRib.AddItem(
                new SplitButtonData("TextTools", "Text Tools")
                ) as SplitButton;

            var cmbTxtBtn = txtSplitBtn.AddPushButton(
                new PushButtonData("CombineText", "Combine\nText", al, "Proficient.CombineText"));
            cmbTxtBtn.LargeImage = Icons.ScaledIcon("combine", 32);
            cmbTxtBtn.ToolTip = "Combine multiple text elements into one";
            cmbTxtBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#combine-text"));

            var txtLdrBtn = txtSplitBtn.AddPushButton(
                new PushButtonData("LeaderText", "Add Text\nWith Leader", al, "Proficient.TextLeader"));
            txtLdrBtn.LargeImage = Icons.ScaledIcon("leadertext", 32);
            txtLdrBtn.ToolTip = "Add text element with leader";
            txtLdrBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#add-text-with-leader"));

            var flatTxtBtn = txtSplitBtn.AddPushButton(
                new PushButtonData("FlattenText", "Flatten\nText", al, "Proficient.FlattenText"));
            flatTxtBtn.LargeImage = Icons.ScaledIcon("flattentext", 32);
            flatTxtBtn.ToolTip = "Remove all line breaks from text element";
            flatTxtBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#flatten-text"));

            var excelAssignBtn = genRib.AddItem(
                new PushButtonData("ExcelAssign", "Excel\nAssigner", al, "Proficient.ExcelAssign")
                ) as RibbonButton;
            excelAssignBtn.LargeImage = Icons.ScaledIcon("xl2rvt", 32);
            excelAssignBtn.ToolTip = "Import data from Excel file into Revit family parameters";
            excelAssignBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#excel-assigner"));

            var elemPlaceBtn = genRib.AddItem(
                new PushButtonData("ElementPlacer", "Element\nPlacer", al, "Proficient.ElementPlacer")
                ) as RibbonButton;
            elemPlaceBtn.LargeImage = Icons.ScaledIcon("elplc", 32);
            elemPlaceBtn.ToolTip = "Place elements along path";
            elemPlaceBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#element-placer"));

            var openSecBtn = genRib.AddItem(
                new PushButtonData("OpenSectionView", "Open\nSection", al, "Proficient.OpenSectionView")
                ) as RibbonButton;
            openSecBtn.LargeImage = Icons.ScaledIcon("section", 32);
            openSecBtn.ToolTip = "Open selected section view";
            openSecBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#open-section"));

            var calloutRefBtn = genRib.AddItem(
                new PushButtonData("ChangeCalloutReference", "Change Callout\nReference", al, "Proficient.ChangeCalloutRef")
                ) as RibbonButton;
            calloutRefBtn.LargeImage = Icons.ScaledIcon("callout", 32);
            calloutRefBtn.ToolTip = "Change referenced view of selected callouts";
            calloutRefBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#change-callout-reference"));

            var addLdrBtn = genRib.AddItem(
                new PushButtonData("AddLeader", "Add\nLeader", al, "Proficient.AddLeader")
                ) as RibbonButton;
            addLdrBtn.LargeImage = Icons.ScaledIcon("leader", 32);
            addLdrBtn.ToolTip = "Add leader to selected element";
            addLdrBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#add-leader"));

            IList<RibbonItem> scheds = genRib.AddStackedItems(
               new PushButtonData("schwidth", "Schedule Width", al, "Proficient.MatchSchWidth"),
               new PushButtonData("schalignleft", "Align Sched L", al, "Proficient.AlignScheduleL"),
               new PushButtonData("schalignright", "Align Sched R", al, "Proficient.AlignScheduleR"));

            (scheds[0] as PushButton).Image = Icons.ScaledIcon("width", 16);
            scheds[0].ToolTip = "Match width of one schedule to another";
            scheds[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-tools"));
            (scheds[1] as PushButton).Image = Icons.ScaledIcon("align", 16);
            scheds[1].ToolTip = "Align left side of one schedule to another";
            scheds[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-tools"));
            (scheds[2] as PushButton).Image = Icons.ScaledIcon("alignflip", 16);
            scheds[2].ToolTip = "Align right side of one schedule to another";
            scheds[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-tools"));

            IList<RibbonItem> flips = genRib.AddStackedItems(
                new PushButtonData("flipel", "Flip Element", al, "Proficient.FlipElements"),
                new PushButtonData("flipplane", "Flip Workplane", al, "Proficient.FlipWorkPlane"));

            (flips[0] as PushButton).Image = Icons.ScaledIcon("flipel", 16);
            flips[0].ToolTip = "Flip element in XY plane";
            flips[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#flip-tools"));
            (flips[1] as PushButton).Image = Icons.ScaledIcon("flipwp", 16);
            flips[1].ToolTip = "File element in Z direction";
            flips[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#flip-tools"));

//toggle panel
            IList<RibbonItem> toggles = togRib.AddStackedItems(
                new PushButtonData("toggleenlwkst", "Enlarged Workset", al, "Proficient.ToggleEnlWkst"),
                new PushButtonData("toggledesignnotes", "Design Annotations", al, "Proficient.ToggleDesignAnno"),
                new PushButtonData("toggleschwarning", "Schedule Warning", al, "Proficient.SuppressSchWarning"));

            (toggles[0] as PushButton).Image = Icons.ScaledIcon("enlwkst", 16);
            toggles[0].ToolTip = "Toggle enlarged workset visibility in current view";
            toggles[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#enlarged-workset"));
            (toggles[1] as PushButton).Image = Icons.ScaledIcon("notes", 16);
            toggles[1].ToolTip = "Toggle design annotation visibility in current view";
            toggles[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#design-annotations"));
            (toggles[2] as PushButton).Image = Icons.ScaledIcon("msg", 16);
            toggles[2].ToolTip = "Toggle type mark schedule warning";
            toggles[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-warning"));

//filter panel
            IList<RibbonItem> filters = filtrib.AddStackedItems(
                new PushButtonData("catfilt", "Category", al, "Proficient.CategoryFilter"),
                new PushButtonData("famfilt", "Family", al, "Proficient.FamilyFilter"),
                new PushButtonData("typefilt", "Type", al, "Proficient.TypeFilter"));

            (filters[0] as PushButton).Image = Icons.ScaledIcon("cat", 16);
            filters[0].ToolTip = "Filter selected elements by category";
            filters[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#filters"));
            (filters[1] as PushButton).Image = Icons.ScaledIcon("fam", 16);
            filters[1].ToolTip = "Filter selected elements by family";
            filters[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#filters"));
            (filters[2] as PushButton).Image = Icons.ScaledIcon("type", 16);
            filters[2].ToolTip = "Filter selected elements by type";
            filters[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#filters"));

//keynote panel
            var knReloadBtn = knRib.AddItem(
                new PushButtonData("ReloadKeynotes","Reload\nKeynotes",al,"Proficient.Keynotes.KNReload")
                ) as RibbonButton;
            knReloadBtn.LargeImage = Icons.ScaledIcon("reload", 32);
            knReloadBtn.ToolTip = "Reload keynotes from keynote Excel file";
            knReloadBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#reload-keynotes"));

            var knOpenBtn = knRib.AddItem(
                new PushButtonData("OpenKeynotes", "Open\nKeynotes", al, "Proficient.Keynotes.KNLauncher")
                ) as RibbonButton;
            knOpenBtn.LargeImage = Icons.ScaledIcon("knxl", 32);
            knOpenBtn.ToolTip = "Open keynote Excel file";
            knOpenBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#open-keynotes"));

            var knUtilBtn = knRib.AddItem(
                new PushButtonData("KeynotesUtility", "Keynotes\nUtility", al, "Proficient.Keynotes.KeynoteUtil")
                ) as RibbonButton;
            knUtilBtn.LargeImage = Icons.ScaledIcon("keynoteutil", 32);
            knUtilBtn.ToolTip = "View keynotes in project and the sheets containing them";
            knUtilBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#keynotes-utility"));

//mech panel
            IList<RibbonItem> mechTags = mechRib.AddStackedItems(
                new PushButtonData("tagduct", "Tag Ducts", al, "Proficient.Mech.DuctTag"),
                new PushButtonData("tagpipe", "Tag Pipes", al, "Proficient.Mech.PipeTag"),
                new PushButtonData("wipemark", "Wipe Mark", al, "Proficient.Mech.WipeMark"));

            (mechTags[0] as PushButton).Image = Icons.ScaledIcon("tagduct", 16);
            mechTags[0].ToolTip = "Tag ductwork in current view";
            mechTags[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#tag-ducts"));
            (mechTags[1] as PushButton).Image = Icons.ScaledIcon("tagpipe", 16);
            mechTags[1].ToolTip = "Tag pipes in current view";
            mechTags[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#tag-pipes"));
            (mechTags[2] as PushButton).Image = Icons.ScaledIcon("wipe", 16);
            mechTags[2].ToolTip = "Wipe type marks or marks for current selection";
            mechTags[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#wipe-mark"));

            IList<RibbonItem> mechTogs = mechRib.AddStackedItems(
                new PushButtonData("ducttoggle", "Duct Elbow Toggle", al, "Proficient.Mech.DuctElbowToggle"),
                new PushButtonData("dampertoggle", "Damper Toggle", al, "Proficient.Mech.DamperToggle"),
                new PushButtonData("togupdn", "Up/Dn Toggle", al, "Proficient.Mech.ToggleUpDn"));

            (mechTogs[0] as PushButton).Image = Icons.ScaledIcon("ducttoggle", 16);
            mechTogs[0].ToolTip = "Toggle duct elbow routing preference";
            mechTogs[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#duct-elbow-toggle"));
            (mechTogs[1] as PushButton).Image = Icons.ScaledIcon("damper", 16);
            mechTogs[1].ToolTip = "Toggle duct tap damper";
            mechTogs[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#damper-toggle"));
            (mechTogs[2] as PushButton).Image = Icons.ScaledIcon("togupdown", 16);
            mechTogs[2].ToolTip = "Toggle up/dn parameter on selected element";
            mechTogs[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#updn-toggle"));

            var pipeSpaceBtn = mechRib.AddItem(
                new PushButtonData("PipeSpacer", "Space\nPipes", al, "Proficient.Mech.PipeSpacer")
                ) as RibbonButton;
            pipeSpaceBtn.LargeImage = Icons.ScaledIcon("spacepipe", 32);
            pipeSpaceBtn.ToolTip = "Space pipes at preset distance";
            pipeSpaceBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#space-pipes"));

            var hardBreakBtn = mechRib.AddItem(
                new PushButtonData("HardBreak", "Hard\nBreak", al, "Proficient.Mech.HardBreak")
                ) as RibbonButton;
            hardBreakBtn.LargeImage = Icons.ScaledIcon("hardbreak", 32);
            hardBreakBtn.ToolTip = "Break pipes or duct without union at point specified";
            hardBreakBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#hard-break"));

            var setUpDnBtn = mechRib.AddItem(
                new PushButtonData("SetUpDn","Auto-Set\nUp/Dn", al, "Proficient.Mech.SetFittingUpDn")
                ) as RibbonButton;
            setUpDnBtn.LargeImage = Icons.ScaledIcon("updn", 32);
            setUpDnBtn.ToolTip = "Set up/dn parameter for all pipe and duct fittings in project";
            setUpDnBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#auto-set-updn"));

            PushButtonData ductulatorData = new PushButtonData("LaunchDuctulator", "Launch\nDuctulator", al, "Proficient.Mech.DuctLauncher");
            ductulatorData.AvailabilityClassName = "Proficient.CommandAvailability";
            var ductulatorBtn = mechRib.AddItem(ductulatorData) as RibbonButton;
            ductulatorBtn.LargeImage = Icons.ScaledIcon("duct",32);
            ductulatorBtn.ToolTip = "Launch ductulator tool";
            ductulatorBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#launch-ductulator"));

//elec panel
            var panelUtilBtn = elecRib.AddItem(
                new PushButtonData("PanelUtility","Panel\nUtility",al,"Proficient.Elec.PanelUtil")
                ) as RibbonButton;
            panelUtilBtn.LargeImage = Icons.ScaledIcon("elecpanel", 32);
            panelUtilBtn.ToolTip = "Show list of panels, panel schedules, and placed sheets";
            panelUtilBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#panel-utility"));

            var elecLoadBtn = elecRib.AddItem(
                new PushButtonData("LoadNameUpdate", "Sync Circuit\nNames", al, "Proficient.Elec.LoadNameUpdate")
                ) as RibbonButton;
            elecLoadBtn.LargeImage = Icons.ScaledIcon("motor", 32);
            elecLoadBtn.ToolTip = "Sync mechanical equipment tags to circuit load names";
            elecLoadBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#sync-circuit-names"));
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