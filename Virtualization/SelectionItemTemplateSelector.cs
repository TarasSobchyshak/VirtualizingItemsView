using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Virtualization
{
    public class SelectionItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedItemTemplate { get; set; }
        public DataTemplate UnselectedItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ISelectable selectable)
            {
                if (selectable.IsSelected && SelectedItemTemplate != null) return SelectedItemTemplate;

                if (!selectable.IsSelected && UnselectedItemTemplate != null) return UnselectedItemTemplate;
            }
            return base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ISelectable selectable)
            {
                if (selectable.IsSelected && SelectedItemTemplate != null) return SelectedItemTemplate;

                if (!selectable.IsSelected && UnselectedItemTemplate != null) return UnselectedItemTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
