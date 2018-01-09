using System;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Virtualization
{
    public static class FrameworkElementExtensions
    {
        public static FrameworkElement FindVisualParent(this FrameworkElement element, Type type)
        {
            var parent = element;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                {
                    return parent;
                }
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }
            return null;
        }
    }

    public class RelativeSourceBinding
    {
        public static readonly DependencyProperty AncestorTypeProperty = DependencyProperty
            .RegisterAttached("AncestorType", typeof(Type), typeof(RelativeSourceBinding), new PropertyMetadata(default(Type), OnAncestorTypeChanged));

        public static readonly DependencyProperty AncestorProperty = DependencyProperty
            .RegisterAttached("Ancestor", typeof(FrameworkElement), typeof(RelativeSourceBinding), new PropertyMetadata(default(FrameworkElement)));

        public static void SetAncestorType(DependencyObject element, Type value)
        {
            element.SetValue(AncestorTypeProperty, value);
        }

        public static Type GetAncestorType(DependencyObject element)
        {
            return (Type)element.GetValue(AncestorTypeProperty);
        }

        private static void OnAncestorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Loaded -= OnFrameworkElementLoaded;

                if (e.NewValue != null)
                {
                    element.Loaded += OnFrameworkElementLoaded;
                    OnFrameworkElementLoaded(element, null);
                }
            }
        }

        private static void OnFrameworkElementLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var ancestorType = GetAncestorType(element);
                if (ancestorType != null)
                {
                    var findAncestor = (element).FindVisualParent(ancestorType);
                    SetAncestor(element, findAncestor);
                }
                else
                {
                    SetAncestor(element, null);
                }
            }
        }

        public static void SetAncestor(DependencyObject element, FrameworkElement value)
        {
            element.SetValue(AncestorProperty, value);
        }

        public static FrameworkElement GetAncestor(DependencyObject element)
        {
            return (FrameworkElement)element.GetValue(AncestorProperty);
        }
    }
}
