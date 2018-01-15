using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Virtualization
{
    [TemplatePart(Name = PartItemsControl, Type = typeof(ItemsControl))]
    public sealed class VirtualizingItemsControl : ItemsControl
    {
        #region Constants

        private const string PartItemsControl = nameof(PartItemsControl);
        private const string ItemWrapperName = "ItemWrapper";

        private readonly List<object> _removedItemsArgs;
        private readonly List<object> _addedItemsArgs;

        #endregion

        #region Private fields

        private ISelectable oldValueKeeper = null;
        private bool existSelectedItemCallback = false;
        private int fixedSelectedItemIndex = -1;
        private bool allItemsSelected = false;
        private ItemsControl _partItemsControl;

        #endregion



        #region Constructors

        public VirtualizingItemsControl()
        {
            _removedItemsArgs = new List<object>();
            _addedItemsArgs = new List<object>();

            DefaultStyleKey = typeof(VirtualizingItemsControl);

            Unloaded += VirtualizingItemsControl_Unloaded;
            Loaded += VirtualizingItemsControl_Loaded;
        }

        #endregion


        #region Override

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild(PartItemsControl) is ItemsControl itemsControl)
            {
                _partItemsControl = itemsControl;
                _partItemsControl.Tapped += Item_Tapped;
            }
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var border = UiExtensions.FindAscendant<Border>(e.OriginalSource as DependencyObject, ItemWrapperName);
            if (border?.DataContext != null)
            {
                ItemTapped(border.DataContext);
            }
        }

        #endregion



        #region Events

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<SelectionChangedEventArgs> Selected;
        public event EventHandler<SelectionChangedEventArgs> Unselected;

        #endregion



        #region Event Handlers

        private void VirtualizingItemsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_partItemsControl != null) _partItemsControl.Tapped -= Item_Tapped;

            SelectionChanged -= VirtualizingItemsControl_SelectionChanged;
            Selected -= VirtualizingItemsControl_Selected;
            Unselected -= VirtualizingItemsControl_Unselected;
        }

        private void VirtualizingItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            SelectionChanged += VirtualizingItemsControl_SelectionChanged;
            Selected += VirtualizingItemsControl_Selected;
            Unselected += VirtualizingItemsControl_Unselected;
        }

        private void VirtualizingItemsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: move this code to private method

            var children = _partItemsControl.ItemsPanelRoot.Children.ToArray();

            var items = children.Where(x => (x as FrameworkElement)?.DataContext is ISelectable).Select(x => ((FrameworkElement)x).DataContext as ISelectable).ToArray();

            var removedItems = e.RemovedItems.Where(x => x is ISelectable).Cast<ISelectable>().ToArray();
            var addedItems = e.AddedItems.Where(x => x is ISelectable).Cast<ISelectable>().ToArray();

            for (int i = 0; i < items.Length; ++i)
            {
                if (items[i] != null && removedItems.FirstOrDefault(x => x.Id == items[i].Id) is ISelectable removed)
                    items[i].IsSelected = removed.IsSelected;
            }

            for (int i = 0; i < items.Length; ++i)
            {
                if (items[i] != null && addedItems.FirstOrDefault(x => x.Id == items[i].Id) is ISelectable added)
                    items[i].IsSelected = added.IsSelected;
            }
        }

        private void VirtualizingItemsControl_Selected(object sender, SelectionChangedEventArgs e)
        {
        }

        private void VirtualizingItemsControl_Unselected(object sender, SelectionChangedEventArgs e)
        {
        }

        #endregion



        #region Dependency Properties

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(ISelectable),
            typeof(VirtualizingItemsControl),
            new PropertyMetadata(null, OnSelectedItemChanged));

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList<ISelectable>),
            typeof(VirtualizingItemsControl),
            new PropertyMetadata(new List<ISelectable>(), OnSelectedItemsChanged));

        public static readonly DependencyProperty SelectedItemTemplateProperty = DependencyProperty.Register(
            nameof(SelectedItemTemplate),
            typeof(DataTemplate),
            typeof(VirtualizingItemsControl),
            new PropertyMetadata(null, new PropertyChangedCallback(OnSelectedItemTemplateChanged)));

        public static readonly DependencyProperty UnselectedItemTemplateProperty = DependencyProperty.Register(
            nameof(UnselectedItemTemplate),
            typeof(DataTemplate),
            typeof(VirtualizingItemsControl),
            new PropertyMetadata(null, new PropertyChangedCallback(OnUnselectedItemTemplateChanged)));


        public IList<ISelectable> SelectedItems
        {
            get { return (IList<ISelectable>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public ISelectable SelectedItem
        {
            get { return (ISelectable)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public DataTemplate SelectedItemTemplate
        {
            get { return (DataTemplate)GetValue(SelectedItemTemplateProperty); }
            set { SetValue(SelectedItemTemplateProperty, value); }
        }

        public DataTemplate UnselectedItemTemplate
        {
            get { return (DataTemplate)GetValue(UnselectedItemTemplateProperty); }
            set { SetValue(UnselectedItemTemplateProperty, value); }
        }


        private void ItemTapped(object obj)
        {
            if (obj is ISelectable selectable)
            {
                //selectable.IsSelected = !selectable.IsSelected;


                // temp VERY BAD solution to ensure property changed callback call every time
                existSelectedItemCallback = true;
                oldValueKeeper = SelectedItem;
                SelectedItem = null;
                existSelectedItemCallback = false;

                SelectedItem = selectable;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VirtualizingItemsControl control)
            {
                if (control.existSelectedItemCallback) return;

                var coreWindow = CoreApplication.MainView.CoreWindow;
                var shiftState = coreWindow.GetKeyState(VirtualKey.Shift);
                var controlState = coreWindow.GetKeyState(VirtualKey.Control);


                if ((shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    if (e.NewValue is IVirtualizing newItem)
                    {
                        var controlUp = (controlState & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down;

                        if (controlUp && control.fixedSelectedItemIndex >= 0)
                        {
                            control.ClearSelectedItems();
                        }

                        if (control.fixedSelectedItemIndex >= 0 && newItem.Index >= 0)
                        {
                            control.SelectRange(control.fixedSelectedItemIndex, newItem.Index, deselectOld: controlUp);
                        }
                    }
                }
                else if ((controlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    if (e.NewValue is ISelectable newItem)
                    {
                        control.SelectNewItem(control.oldValueKeeper, newItem, deselectOld: false, addToSelectableCollection: false);

                        if (control.SelectedItems.Count == 0 && control.oldValueKeeper != null && control.oldValueKeeper.IsSelected)
                        {
                            if (control.oldValueKeeper.IsSelected) control.AddSelectedItem(control.oldValueKeeper);
                            else control.RemoveSelectedItem(control.SelectedItems.FirstOrDefault(x => x.Id == control.oldValueKeeper.Id));
                        }

                        if (newItem.IsSelected) control.AddSelectedItem(newItem);
                        else control.RemoveSelectedItem(control.SelectedItems.FirstOrDefault(x => x.Id == newItem.Id));

                    }
                }
                else
                {
                    control.ClearSelectedItems();

                    control.SelectNewItem(control.oldValueKeeper, e.NewValue as ISelectable, deselectOld: true, addToSelectableCollection: true);
                }
            }
        }

        private void AddSelectedItem(ISelectable item)
        {
            if (item == null) return;

            if (SelectedItems?.FirstOrDefault(x => x.Id == item.Id) == null)
            {
                if (item != null && !item.IsSelected) item.IsSelected = true;

                SelectedItems?.Add(item);
            }

            if (ItemsSource is ISelectableCollection collection && collection.SelectedItemsIds?.Contains(item.Id) != true)
            {
                collection.SelectedItemsIds.Add(item.Id);
            }
        }

        private void RemoveSelectedItem(ISelectable item)
        {
            if (item == null) return;

            item = SelectedItems?.FirstOrDefault(x => x.Id == item.Id);

            if (item != null)
            {
                if (item.IsSelected) item.IsSelected = false;

                SelectedItems?.Remove(item);
            }

            if (ItemsSource is ISelectableCollection collection)
            {
                collection.SelectedItemsIds?.Remove(item.Id);
            }
        }

        private void ClearSelectedItems()
        {
            SelectedItems?.Clear();

            (ItemsSource as ISelectableCollection)?.SelectedItemsIds?.Clear();
        }

        private void SelectNewItem(ISelectable oldValue, ISelectable newValue, bool deselectOld, bool addToSelectableCollection)
        {
            if (deselectOld && oldValue != null && oldValue != newValue) oldValue.IsSelected = false;
            if (newValue != null) newValue.IsSelected = !newValue.IsSelected;

            _removedItemsArgs.Clear();

            if (deselectOld)
            {
                var removed = _addedItemsArgs.ToArray();

                for (int i = 0; i < removed.Length; ++i)
                {
                    if (removed[i] is ISelectable item && item.Id != oldValue.Id) item.IsSelected = false;
                }

                _removedItemsArgs.AddRange(removed);

                _addedItemsArgs.Clear();
            }
            else
            {
                if (_addedItemsArgs.FirstOrDefault(x => (x as ISelectable)?.Id == newValue.Id) is ISelectable oldItem)
                    _addedItemsArgs.Remove(oldItem);
            }

            if (newValue != null)
                _addedItemsArgs.Add(newValue);

            var args = new SelectionChangedEventArgs(_removedItemsArgs.ToList(), _addedItemsArgs.ToList());

            if (newValue != null)
            {
                fixedSelectedItemIndex = newValue.IsSelected && newValue is IVirtualizing virtualizing ? virtualizing.Index : -1;

                if (newValue.IsSelected)
                {
                    OnSelected(args);

                    if (addToSelectableCollection && ItemsSource is ISelectableCollection collection && collection.SelectedItemsIds?.Contains(newValue.Id) != true)
                    {
                        collection.SelectedItemsIds.Add(newValue.Id);
                    }
                }

                else OnUnselected(args);
            }

            OnSelectionChanged(args);
        }

        private void SelectRange(int from, int to, bool deselectOld)
        {
            if (from < 0) throw new ArgumentOutOfRangeException(nameof(from));
            if (to < 0) throw new ArgumentOutOfRangeException(nameof(to));

            if (to < from)
            {
                var temp = from;
                from = to;
                to = temp;
            }

            _removedItemsArgs.Clear();

            if (deselectOld)
            {
                var removed = _addedItemsArgs.ToArray();

                for (int i = 0; i < removed.Length; ++i)
                {
                    if (removed[i] is ISelectable item) item.IsSelected = false;
                }

                _removedItemsArgs.AddRange(removed);

                _addedItemsArgs.Clear();
            }

            for (int i = from; i <= to && i < _partItemsControl.Items.Count; ++i)
            {
                if (_partItemsControl.Items[i] is ISelectable item)
                {
                    item.IsSelected = true;

                    AddSelectedItem(item);

                    if (_addedItemsArgs.FirstOrDefault(x => (x as ISelectable)?.Id == item.Id) is ISelectable oldItem)
                        _addedItemsArgs.Remove(oldItem);

                    _addedItemsArgs.Add(item);
                }
            }

            var args = new SelectionChangedEventArgs(_removedItemsArgs.ToList(), _addedItemsArgs.ToList());

            OnSelectionChanged(args);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VirtualizingItemsControl control)
            {
                var args = new SelectionChangedEventArgs(e.OldValue as IList<object>, e.NewValue as IList<object>);

                control.OnSelectionChanged(args);
            }
        }

        private static void OnSelectedItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void OnUnselectedItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion


        #region Private Methods

        private void OnSelectionChanged(SelectionChangedEventArgs args)
        {
            SelectionChanged?.Invoke(this, args);
        }

        private void OnSelected(SelectionChangedEventArgs args)
        {
            Selected?.Invoke(this, args);
        }

        private void OnUnselected(SelectionChangedEventArgs args)
        {
            Unselected?.Invoke(this, args);
        }

        #endregion




    }
}
