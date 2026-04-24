using Proficient.General;
using System.Reflection;
using Proficient.Electrical;
using Proficient.Filters;
using Proficient.Keynotes;
using Proficient.Mechanical;
using Proficient.Toggles;
using SelectionFilterType = Proficient.Filters.SelectionFilterType;

namespace Proficient.Utilities;

internal static class Ribbon
{
    internal static void Create()
    {
        var app = Main.App;
        var al = Assembly.GetExecutingAssembly().Location;
        const string ugUrl = "https://github.com/jroth93/Proficient/wiki/Proficient-User-Guide";

        if (app == null) return;
        app.CreateRibbonTab("Proficient");

        var genRib = app.CreateRibbonPanel("Proficient", "General");
        var togRib = app.CreateRibbonPanel("Proficient", "Toggles");
        var filtRib = app.CreateRibbonPanel("Proficient", "Filters");
        var knRib = app.CreateRibbonPanel("Proficient", "Keynotes");
        var mechRib = app.CreateRibbonPanel("Proficient", "Mechanical");
        var elecRib = app.CreateRibbonPanel("Proficient", "Electrical");

        genRib.Title = "General";
        togRib.Title = "Toggles";
        knRib.Title = "Keynotes";
        mechRib.Title = "Mechanical";
        elecRib.Title = "Electrical";

//general panel
        var settingBtn = (RibbonButton)genRib.AddItem(
            new PushButtonData("EditSettings", "Edit\nSettings", al, typeof(EditSettings).FullName));
        settingBtn.LargeImage = Icons.ScaledIcon("settings", 32);
        settingBtn.ToolTip = "Change Proficient settings";
        settingBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#edit-settings"));

        var txtSplitBtn = (SplitButton)genRib.AddItem(
            new SplitButtonData("TextTools", "Text Tools"));

        var cmbTxtBtn = txtSplitBtn.AddPushButton(
            new PushButtonData("CombineText", "Combine\nText", al, typeof(CombineText).FullName));
        cmbTxtBtn.LargeImage = Icons.ScaledIcon("combine", 32);
        cmbTxtBtn.ToolTip = "Combine multiple text elements into one";
        cmbTxtBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#combine-text"));

        var txtLdrBtn = txtSplitBtn.AddPushButton(
            new PushButtonData("LeaderText", "Add Text\nWith Leader", al, typeof(TextLeader).FullName));
        txtLdrBtn.LargeImage = Icons.ScaledIcon("leadertext", 32);
        txtLdrBtn.ToolTip = "Add text element with leader";
        txtLdrBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#add-text-with-leader"));

        var flatTxtBtn = txtSplitBtn.AddPushButton(
            new PushButtonData("FlattenText", "Flatten\nText", al, typeof(FlattenText).FullName));
        flatTxtBtn.LargeImage = Icons.ScaledIcon("flattentext", 32);
        flatTxtBtn.ToolTip = "Remove all line breaks from text element";
        flatTxtBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#flatten-text"));

        var excelAssignBtn = (RibbonButton)genRib.AddItem(
            new PushButtonData("ExcelAssign", "Excel\nAssigner", al, typeof(ExcelAssign).FullName));
        excelAssignBtn.LargeImage = Icons.ScaledIcon("xl2rvt", 32);
        excelAssignBtn.ToolTip = "Import data from Excel file into Revit family parameters";
        excelAssignBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#excel-assigner"));

        var elemPlaceBtn = (RibbonButton)genRib.AddItem(
            new PushButtonData("ElementPlacer", "Element\nPlacer", al, typeof(ElementPlacer).FullName));
        elemPlaceBtn.LargeImage = Icons.ScaledIcon("elplc", 32);
        elemPlaceBtn.ToolTip = "Place elements along path";
        elemPlaceBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#element-placer"));

        var openSecBtn = (RibbonButton)genRib.AddItem(
            new PushButtonData("OpenSectionView", "Open\nSection", al, typeof(OpenSectionView).FullName));
        openSecBtn.LargeImage = Icons.ScaledIcon("section", 32);
        openSecBtn.ToolTip = "Open selected section view";
        openSecBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#open-section"));

        var calloutRefBtn = (RibbonButton)genRib.AddItem(
            new PushButtonData("ChangeCalloutReference", "Change Callout\nReference", al, typeof(ChangeCalloutRef).FullName));
        calloutRefBtn.LargeImage = Icons.ScaledIcon("callout", 32);
        calloutRefBtn.ToolTip = "Change referenced view of selected callouts";
        calloutRefBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#change-callout-reference"));

        var addLdrBtn = (RibbonButton)genRib.AddItem(
            new PushButtonData("AddLeader", "Add\nLeader", al, typeof(AddLeader).FullName));
        addLdrBtn.LargeImage = Icons.ScaledIcon("leader", 32);
        addLdrBtn.ToolTip = "Add leader to selected element";
        addLdrBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#add-leader"));

        var reloadFamBtn = (RibbonButton) genRib.AddItem(
            new PushButtonData("ReloadFamily", "Reload\nFamily", al, typeof(ReloadFamily).FullName));
        reloadFamBtn.LargeImage = Icons.ScaledIcon("reloadfam", 32);
        reloadFamBtn.ToolTip = "Reload family for selected element";
        reloadFamBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#reload-family"));

        /*
        var notesPaneBtn = (RibbonButton)genRib.AddItem(
            new PushButtonData("ShowNotesPane", "Show Notes\nPane", al, typeof(ShowNotesPane).FullName));
        notesPaneBtn.LargeImage = Icons.ScaledIcon("notespane", 32);
        notesPaneBtn.ToolTip = "Show notes pane";
        notesPaneBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#show-notes-pane"));
        */

        var scheds = genRib.AddStackedItems(
                new PushButtonData("schwidth", "Schedule Width", al, typeof(MatchSchWidth).FullName),
                new PushButtonData("schalignleft", "Align Sched L", al, typeof(AlignScheduleL).FullName),
                new PushButtonData("schalignright", "Align Sched R", al, typeof(AlignScheduleR).FullName))
            .Cast<PushButton>().ToList();

        scheds[0].Image = Icons.ScaledIcon("width", 16);
        scheds[0].ToolTip = "Match width of one schedule to another";
        scheds[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-tools"));

        scheds[1].Image = Icons.ScaledIcon("align", 16);
        scheds[1].ToolTip = "Align left side of one schedule to another";
        scheds[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-tools"));

        scheds[2].Image = Icons.ScaledIcon("alignflip", 16);
        scheds[2].ToolTip = "Align right side of one schedule to another";
        scheds[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-tools"));

        var flips = genRib.AddStackedItems(
                new PushButtonData("flipel", "Flip Element", al, typeof(FlipElements).FullName),
                new PushButtonData("flipplane", "Flip Workplane", al, typeof(FlipWorkPlane).FullName))
            .Cast<PushButton>().ToList();

        flips[0].Image = Icons.ScaledIcon("flipel", 16);
        flips[0].ToolTip = "Flip element in XY plane";
        flips[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#flip-tools"));

        flips[1].Image = Icons.ScaledIcon("flipwp", 16);
        flips[1].ToolTip = "File element in Z direction";
        flips[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#flip-tools"));

//toggle panel
        var toggles = togRib.AddStackedItems(
                new PushButtonData("toggleenlwkst", "Enlarged Workset", al, typeof(ToggleEnlargedWorkset).FullName),
                new PushButtonData("toggledesignnotes", "Design Annotations", al, typeof(ToggleDesignAnnotations).FullName),
                new PushButtonData("toggleschwarning", "Schedule Warning", al, typeof(ScheduleWarningToggle).FullName))
            .Cast<PushButton>().ToList();

        toggles[0].Image = Icons.ScaledIcon("enlwkst", 16);
        toggles[0].ToolTip = "Toggle enlarged workset visibility in current view";
        toggles[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#enlarged-workset"));
            
        toggles[1].Image = Icons.ScaledIcon("notes", 16);
        toggles[1].ToolTip = "Toggle design annotation visibility in current view";
        toggles[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#design-annotations"));
            
        toggles[2].Image = Icons.ScaledIcon("msg", 16);
        toggles[2].ToolTip = "Toggle type mark schedule warning";
        toggles[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#schedule-warning"));

//filter panel
        var filters = filtRib.AddStackedItems(
                new PushButtonData("catfilt", "Category", al, typeof(SelectionFilterCategory).FullName),
                new PushButtonData("famfilt", "Family", al, typeof(SelectionFilterFamily).FullName),
                new PushButtonData("typefilt", "Type", al, typeof(SelectionFilterType).FullName))
            .Cast<PushButton>().ToList();

        filters[0].Image = Icons.ScaledIcon("cat", 16);
        filters[0].ToolTip = "Filter selected elements by category";
        filters[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#filters"));
            
        filters[1].Image = Icons.ScaledIcon("fam", 16);
        filters[1].ToolTip = "Filter selected elements by family";
        filters[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#filters"));
            
        filters[2].Image = Icons.ScaledIcon("type", 16);
        filters[2].ToolTip = "Filter selected elements by type";
        filters[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#filters"));

//keynote panel
        var knReloadBtn = (RibbonButton)knRib.AddItem(
            new PushButtonData("ReloadKeynotes","Reload\nKeynotes",al,typeof(KnReload).FullName)
        );
        knReloadBtn.LargeImage = Icons.ScaledIcon("reload", 32);
        knReloadBtn.ToolTip = "Reload keynotes from keynote Excel file";
        knReloadBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#reload-keynotes"));

        var knOpenBtn = (RibbonButton)knRib.AddItem(
            new PushButtonData("OpenKeynotes", "Open\nKeynotes", al, typeof(KnLauncher).FullName)
        );
        knOpenBtn.LargeImage = Icons.ScaledIcon("knxl", 32);
        knOpenBtn.ToolTip = "Open keynote Excel file";
        knOpenBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#open-keynotes"));

        var knUtilBtn = (RibbonButton)knRib.AddItem(
            new PushButtonData("KeynotesUtility", "Keynotes\nUtility", al, typeof(KeynoteUtil).FullName)
        );
        knUtilBtn.LargeImage = Icons.ScaledIcon("keynoteutil", 32);
        knUtilBtn.ToolTip = "View keynotes in project and the sheets containing them";
        knUtilBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#keynotes-utility"));

//mech panel
        var mechTags = mechRib.AddStackedItems(
                new PushButtonData("tagduct", "Tag Ducts", al, typeof(DuctTag).FullName),
                new PushButtonData("tagpipe", "Tag Pipes", al, typeof(PipeTag).FullName),
                new PushButtonData("wipemark", "Wipe Mark", al, typeof(WipeMark).FullName))
            .Cast<PushButton>().ToList();

        mechTags[0].Image = Icons.ScaledIcon("tagduct", 16);
        mechTags[0].ToolTip = "Tag ducts in current view";
        mechTags[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#tag-ducts"));

        mechTags[1].Image = Icons.ScaledIcon("tagpipe", 16);
        mechTags[1].ToolTip = "Tag pipes in current view";
        mechTags[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#tag-pipes"));

        mechTags[2].Image = Icons.ScaledIcon("wipe", 16);
        mechTags[2].ToolTip = "Wipe marks for current selection";
        mechTags[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#wipe-mark"));

        var mechTogs = mechRib.AddStackedItems(
                new PushButtonData("ducttoggle", "Duct Elbow Toggle", al, typeof(DuctElbowToggle).FullName),
                new PushButtonData("dampertoggle", "Damper Toggle", al, typeof(DamperToggle).FullName),
                new PushButtonData("togupdn", "Up/Dn Toggle", al, typeof(ToggleUpDn).FullName))
            .Cast<PushButton>().ToList();

        mechTogs[0].Image = Icons.ScaledIcon("ducttoggle", 16);
        mechTogs[0].ToolTip = "Toggle duct elbow type";
        mechTogs[0].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#duct-elbow-toggle"));

        mechTogs[1].Image = Icons.ScaledIcon("damper", 16);
        mechTogs[1].ToolTip = "Toggle duct tap damper";
        mechTogs[1].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#damper-toggle"));

        mechTogs[2].Image = Icons.ScaledIcon("togupdown", 16);
        mechTogs[2].ToolTip = "Toggle up/dn parameter on selected element";
        mechTogs[2].SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#updn-toggle"));

        var pipeSpaceBtn = (RibbonButton)mechRib.AddItem(
            new PushButtonData("PipeSpacer", "Space\nPipes", al, typeof(PipeSpacer).FullName)
        );
        pipeSpaceBtn.LargeImage = Icons.ScaledIcon("spacepipe", 32);
        pipeSpaceBtn.ToolTip = "Space pipes at preset distance";
        pipeSpaceBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#space-pipes"));

        var hardBreakBtn = (RibbonButton)mechRib.AddItem(
            new PushButtonData("HardBreak", "Hard\nBreak", al, typeof(HardBreak).FullName)
        );
        hardBreakBtn.LargeImage = Icons.ScaledIcon("hardbreak", 32);
        hardBreakBtn.ToolTip = "Break pipes or duct without union at point specified";
        hardBreakBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#hard-break"));

        var setUpDnBtn = (RibbonButton)mechRib.AddItem(
            new PushButtonData("SetUpDn","Auto-Set\nUp/Dn", al, typeof(SetFittingUpDn).FullName)
        );
        setUpDnBtn.LargeImage = Icons.ScaledIcon("updn", 32);
        setUpDnBtn.ToolTip = "Set up/dn parameter for pipe/duct fittings in view or selected pipe/duct fittings";
        setUpDnBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#auto-set-updn"));

        var dlData = new PushButtonData("LaunchDuctulator", "Launch\nDuctulator", al, typeof(DuctLauncher).FullName)
        {
            AvailabilityClassName = typeof(CommandAvailability).FullName
        };
        var dlBtn = (RibbonButton)mechRib.AddItem(dlData);
        dlBtn.LargeImage = Icons.ScaledIcon("duct",32);
        dlBtn.ToolTip = "Launch ductulator tool";
        dlBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#launch-ductulator"));

//elec panel
        var panelUtilBtn = (RibbonButton)elecRib.AddItem(
            new PushButtonData("PanelUtility","Panel\nUtility",al,typeof(PanelUtil).FullName)
        );
        panelUtilBtn.LargeImage = Icons.ScaledIcon("elecpanel", 32);
        panelUtilBtn.ToolTip = "Show list of panels, panel schedules, and placed sheets";
        panelUtilBtn.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, $"{ugUrl}#panel-utility"));

        var elecLoadBtn = (RibbonButton)elecRib.AddItem(
            new PushButtonData("LoadNameUpdate", "Sync Circuit\nNames", al, typeof(LoadNameUpdate).FullName)
        );
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