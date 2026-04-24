using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Proficient.Utilities;

public class VisualTreeHelpers
{
    /// <summary>
    /// Returns the first ancester of specified type
    /// </summary>
    public static T? FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
    {
        current = VisualTreeHelper.GetParent(current);

        while (current != null)
        {
            if (current is T t)
            {
                return t;
            }
            current = VisualTreeHelper.GetParent(current);
        };
        return null;
    }

    /// <summary>
    /// Returns a specific ancester of an object
    /// </summary>
    public static T? FindAncestor<T>(DependencyObject current, T lookupItem)
        where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T t && current == lookupItem)
            {
                return t;
            }
            current = VisualTreeHelper.GetParent(current);
        };
        return null;
    }

    /// <summary>
    /// Finds an ancestor object by name and type
    /// </summary>
    public static T? FindAncestor<T>(DependencyObject current, string parentName)
        where T : DependencyObject
    {
        while (current != null)
        {
            if (!string.IsNullOrEmpty(parentName))
            {
                if (current is T t && current is FrameworkElement frameworkElement && frameworkElement.Name == parentName)
                {
                    return t;
                }
            }
            else if (current is T t)
            {
                return t;
            }
            current = VisualTreeHelper.GetParent(current);
        };

        return null;

    }

    /// <summary>
    /// Looks for a child control within a parent by name
    /// </summary>
    public static T? FindChild<T>(DependencyObject parent, string childName)
        where T : DependencyObject
    {
        // Confirm parent and childName are valid.
        if (parent == null) return null;

        T? foundChild = null;

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            if (child is not T)
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child.
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                // If the child's name is set for search
                if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                {
                    // if the child's name is of the request name
                    foundChild = (T)child;
                    break;
                }
                else
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
            }
            else
            {
                // child element found.
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }

    /// <summary>
    /// Looks for a child control within a parent by type
    /// </summary>
    public static T? FindChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        // Confirm parent is valid.
        if (parent == null) return null;

        T? foundChild = null;

        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            if (child is not T)
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child);

                // If the child is found, break so we do not overwrite the found child.
                if (foundChild != null) break;
            }
            else
            {
                // child element found.
                foundChild = (T)child;
                break;
            }
        }
        return foundChild;
    }
        
}