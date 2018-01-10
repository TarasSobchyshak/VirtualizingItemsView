using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Virtualization
{
    public static class UiExtensions
    {
        public static T FindAscendant<T>(this DependencyObject element, string parentName = null)
            where T : DependencyObject
        {
            if (element is T && (string.IsNullOrWhiteSpace(parentName) || element is FrameworkElement item && item.Name == parentName)) return (T)element;

            return VisualTreeHelper.GetParent(element)?.FindAscendant<T>(parentName);
        }


        public static T FindChild<T>(this DependencyObject parent, string childName = null) where T : DependencyObject
        {
            if (parent == null)
                return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; foundChild == null && i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var childType = child as T;
                if (childType == null)
                {
                    foundChild = FindChild<T>(child, childName);
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                    }
                    else
                    {
                        foundChild = FindChild<T>(child, childName);
                    }
                }
                else
                {
                    foundChild = (T)child;
                }
            }

            return foundChild;
        }
    }
}
