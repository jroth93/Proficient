using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using UIFramework;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock;

namespace Proficient.Utilities;

public static class DocumentTabEventUtils
{
    public static UIApplication? UIApp { get; private set; }

    public static bool IsUpdatingDocumentTabs { get; private set; }

    private static readonly object UpdateLock = new();

    public static DockingManager? GetDockingManager(UIApplication uiapp)
    {
        var wndRoot = (MainWindow?)UIAppEventUtils.GetWindowRoot(uiapp);
        return wndRoot != null ? MainWindow.FindFirstChild<DockingManager>(wndRoot) : null;
    }

    public static LayoutDocumentPaneGroupControl? GetDocumentTabGroup(UIApplication uiapp)
    {
        var wndRoot = (MainWindow?)UIAppEventUtils.GetWindowRoot(uiapp);
        return wndRoot != null ? MainWindow.FindFirstChild<LayoutDocumentPaneGroupControl>(wndRoot) : null;
    }

    public static IEnumerable<LayoutDocumentPaneControl> GetDocumentPanes(LayoutDocumentPaneGroupControl docTabGroup)
    {
        return docTabGroup != null ? docTabGroup.FindVisualChildren<LayoutDocumentPaneControl>() : [];
    }

    public static DocumentPaneTabPanel? GetDocumentTabsPane(LayoutDocumentPaneGroupControl docTabGroup)
    {
        return docTabGroup?.FindVisualChildren<DocumentPaneTabPanel>()?.FirstOrDefault();
    }

    public static IEnumerable<TabItem> GetDocumentTabs(LayoutDocumentPaneControl docPane)
    {
        return docPane != null ? docPane.FindVisualChildren<TabItem>() : [];
    }

    public static IEnumerable<TabItem> GetDocumentTabs(LayoutDocumentPaneGroupControl docTabGroup)
    {
        return docTabGroup != null ? docTabGroup.FindVisualChildren<TabItem>() : [];
    }

    public static void StartGroupingDocumentTabs(UIApplication uiapp)
    {
        lock (UpdateLock)
        {
            if (IsUpdatingDocumentTabs) return;

            UIApp = uiapp;
            IsUpdatingDocumentTabs = true;

            if(GetDockingManager(UIApp) is DockingManager docMgr)
                docMgr.LayoutUpdated += UpdateDockingManagerLayout;

        }
    }

    public static void StopGroupingDocumentTabs()
    {
        lock (UpdateLock)
        {
            if (!IsUpdatingDocumentTabs || UIApp is null) return;

            if (GetDockingManager(UIApp) is DockingManager docMgr)
                docMgr.LayoutUpdated -= UpdateDockingManagerLayout;

            ClearDocumentTabGroups();

            IsUpdatingDocumentTabs = false;
        }
    }

    static void UpdateDockingManagerLayout(object? sender, EventArgs e)
    {
        UpdateDocumentTabGroups();
    }

    static void ClearDocumentTabGroups()
    {
        lock (UpdateLock)
        {
            if(UIApp is null) return;

            if (GetDocumentTabGroup(UIApp) is LayoutDocumentPaneGroupControl docTabGroup)
            {
                // dont do anything if there are no tabs
                if (!GetDocumentTabs(docTabGroup).Any()) return;
                // reset tabs
            }
        }
    }

    static void UpdateDocumentTabGroups()
    {
        lock (UpdateLock)
        {
            if (!IsUpdatingDocumentTabs || UIApp is null) return;
            
            if (GetDocumentTabGroup(UIApp) is LayoutDocumentPaneGroupControl docTabGroup)
            {
                // dont do anything if there are no tabs
                if (!GetDocumentTabs(docTabGroup).Any()) return;
            }
        }
    }
}

public class UIAppEventUtils
{
    public static Visual? GetWindowRoot(UIApplication uiapp)
    {
        var wndHndle = IntPtr.Zero;
        try
        {
            wndHndle = uiapp.MainWindowHandle;
        }
        catch { }

        if (wndHndle != IntPtr.Zero)
        {
            var wndSource = HwndSource.FromHwnd(wndHndle);
            return wndSource.RootVisual;
        }
        return null;
    }
}