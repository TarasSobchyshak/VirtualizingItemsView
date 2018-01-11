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
                    // TODO: improve this to support data virtualizing ItemsSource

                    //var items = ((IEnumerable<object>)control._partItemsControl.Items).Cast<ISelectable>().ToList();
                    //var newItemIndex = items.Select(x => x.Id).ToList().IndexOf((e.NewValue as ISelectable)?.Id);

                    var newItemIndex = -1;
                    if (e.NewValue is ISelectable newItem)
                    {
                        for (int i = 0; i < control._partItemsControl.Items.Count; ++i)
                        {
                            if (control._partItemsControl.Items[i] is ISelectable item && item.Id == newItem.Id)
                            {
                                //item.IsSelected = true;
                                newItemIndex = i;
                                break;
                            }
                        }

                        if ((controlState & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down && control.fixedSelectedItemIndex >= 0)
                        {
                            ClearSelectedItems(control);
                        }

                        if (control.fixedSelectedItemIndex >= 0 && newItemIndex >= 0)
                        {
                            for (int i = Math.Min(control.fixedSelectedItemIndex, newItemIndex); i <= Math.Max(control.fixedSelectedItemIndex, newItemIndex); ++i)
                            {
                                if (control._partItemsControl.Items[i] is ISelectable item)
                                {
                                    item.IsSelected = true;

                                    if (!control.SelectedItems.Contains(item)) control.AddSelectedItem(item);
                                }
                            }
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
                    ClearSelectedItems(control);

                    control.SelectNewItem(control.oldValueKeeper, e.NewValue as ISelectable, deselectOld: true, addToSelectableCollection: true);
                }
            }
        }

        private void AddSelectedItem(ISelectable item)
        {
            if (item == null) return;

            if (SelectedItems?.Contains(item) != true)
            {
                SelectedItems.Add(item);
            }

            if (ItemsSource is ISelectableCollection collection && collection.SelectedItemsIds?.Contains(item.Id) != true)
            {
                collection.SelectedItemsIds.Add(item.Id);
            }
        }

        private void RemoveSelectedItem(ISelectable item)
        {
            if (item == null) return;

            SelectedItems?.Remove(item);

            if (ItemsSource is ISelectableCollection collection)
            {
                collection.SelectedItemsIds?.Remove(item.Id);
            }
        }

        private static void ClearSelectedItems(VirtualizingItemsControl control)
        {
            if (control == null) return;

            for (int i = 0; i < control.SelectedItems.Count; ++i)
            {
                if (control.SelectedItems[i] is ISelectable item)
                {
                    item.IsSelected = false;
                }
            }

            if (control.ItemsSource is ISelectableCollection collection)
            {
                collection.SelectedItemsIds.Clear();
            }

            control.SelectedItems.Clear();
        }

        private void SelectNewItem(ISelectable oldValue, ISelectable newValue, bool deselectOld, bool addToSelectableCollection)
        {
            if (oldValue != null && deselectOld && oldValue != newValue) oldValue.IsSelected = false;
            if (newValue != null) newValue.IsSelected = !newValue.IsSelected;

            var args = new SelectionChangedEventArgs(new List<object>() { oldValue }, new List<object>() { newValue });

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
