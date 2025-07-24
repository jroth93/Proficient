using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

using UIFramework;

using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Controls;

namespace Proficient.Utilities;

public static class DocumentTabEventUtils
{
    public static UIApplication UIApp { get; private set; }

    public static bool IsUpdatingDocumentTabs { get; private set; }

    private static object UpdateLock = new object();

    public static Xceed.Wpf.AvalonDock.DockingManager GetDockingManager(UIApplication uiapp)
    {
        var wndRoot = (MainWindow) UIAppEventUtils.GetWindowRoot(uiapp);
        if (wndRoot != null)
        {
            return MainWindow.FindFirstChild<Xceed.Wpf.AvalonDock.DockingManager>(wndRoot);
        }

        return null;
    }

    public static LayoutDocumentPaneGroupControl GetDocumentTabGroup(UIApplication uiapp)
    {
        var wndRoot = (MainWindow)UIAppEventUtils.GetWindowRoot(uiapp);
        if (wndRoot != null)
        {
            return MainWindow.FindFirstChild<LayoutDocumentPaneGroupControl>(wndRoot);
        }

        return null;
    }

    public static IEnumerable<LayoutDocumentPaneControl> GetDocumentPanes(
        LayoutDocumentPaneGroupControl docTabGroup)
    {
        if (docTabGroup != null)
        {
            return docTabGroup.FindVisualChildren<LayoutDocumentPaneControl>();
        }

        return new List<LayoutDocumentPaneControl>();
    }

    public static DocumentPaneTabPanel GetDocumentTabsPane(LayoutDocumentPaneGroupControl docTabGroup)
    {
        return docTabGroup?.FindVisualChildren<DocumentPaneTabPanel>()?.FirstOrDefault();
    }

    public static IEnumerable<TabItem> GetDocumentTabs(LayoutDocumentPaneControl docPane)
    {
        if (docPane != null)
        {
            return docPane.FindVisualChildren<TabItem>();
        }

        return new List<TabItem>();
    }

    public static IEnumerable<TabItem> GetDocumentTabs(LayoutDocumentPaneGroupControl docTabGroup)
    {
        if (docTabGroup != null)
        {
            return docTabGroup.FindVisualChildren<TabItem>();
        }

        return new List<TabItem>();
    }

    public static void StartGroupingDocumentTabs(UIApplication uiapp)
    {
        lock (UpdateLock)
        {
            if (!IsUpdatingDocumentTabs)
            {
                UIApp = uiapp;
                IsUpdatingDocumentTabs = true;

                var docMgr = GetDockingManager(UIApp);
                docMgr.LayoutUpdated += UpdateDockingManagerLayout;
            }
        }
    }

    public static void StopGroupingDocumentTabs()
    {
        lock (UpdateLock)
        {
            if (IsUpdatingDocumentTabs)
            {
                var docMgr = GetDockingManager(UIApp);
                docMgr.LayoutUpdated -= UpdateDockingManagerLayout;

                ClearDocumentTabGroups();

                IsUpdatingDocumentTabs = false;
            }
        }
    }

    static void UpdateDockingManagerLayout(object sender, EventArgs e)
    {
        UpdateDocumentTabGroups();
    }

    static void ClearDocumentTabGroups()
    {
        lock (UpdateLock)
        {
            var docTabGroup = GetDocumentTabGroup(UIApp);
            if (docTabGroup != null)
            {
                var docTabs = GetDocumentTabs(docTabGroup);
                // dont do anything if there are no tabs
                if (docTabs.Count() == 0)
                    return;

                // reset tabs
            }
        }
    }

    static void UpdateDocumentTabGroups()
    {
        lock (UpdateLock)
        {
            if (IsUpdatingDocumentTabs)
            {
                // get the ui tabs
                var docTabGroup = GetDocumentTabGroup(UIApp);
                if (docTabGroup != null)
                {
                    var docTabs = GetDocumentTabs(docTabGroup);

                    // dont do anything if there are no tabs
                    if (docTabs.Count() == 0)
                        return;


                }
            }
        }
    }
}

public class UIAppEventUtils
{
    public static Visual GetWindowRoot(UIApplication uiapp)
    {
        IntPtr wndHndle = IntPtr.Zero;
        try
        {
            wndHndle = Autodesk.Windows.ComponentManager.ApplicationWindow;
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