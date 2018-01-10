using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Virtualization
{
    public sealed class VirtualizingItemsControl : ItemsControl
    {
        #region Constructors

        public VirtualizingItemsControl()
        {
            DefaultStyleKey = typeof(VirtualizingItemsControl);

            ItemTappedCommand = new RelayCommand<object>(ItemTapped);

            Unloaded += VirtualizingItemsControl_Unloaded;
            Loaded += VirtualizingItemsControl_Loaded;
        }

        #endregion


        #region Override

        //protected override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //}

        #endregion



        #region Events

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<SelectionChangedEventArgs> Selected;
        public event EventHandler<SelectionChangedEventArgs> Unselected;

        #endregion



        #region Event Handlers

        private void VirtualizingItemsControl_Unloaded(object sender, RoutedEventArgs e)
        {
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

        public static readonly DependencyProperty ItemTappedCommandProperty = DependencyProperty.Register(
            nameof(ItemTappedCommand),
            typeof(ICommand),
            typeof(VirtualizingItemsControl),
            new PropertyMetadata(null));


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

        public ICommand ItemTappedCommand
        {
            get { return (ICommand)GetValue(ItemTappedCommandProperty); }
            set { SetValue(ItemTappedCommandProperty, value); }
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

        private ISelectable oldValueKeeper = null;
        private bool existSelectedItemCallback = false;
        private int fixedSelectedItemIndex = -1;

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
                    var items = ((IEnumerable<object>)control.ItemsSource).Cast<ISelectable>().ToList();
                    var newItemIndex = items.IndexOf(e.NewValue as ISelectable);

                    if ((controlState & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down)
                    {
                        ClearSelectedItems(control);

                        //for (int i = 0; i < items.Count; ++i)
                        //    items[i].IsSelected = false;

                        //control.SelectedItems.Clear();
                    }

                    if (control.fixedSelectedItemIndex >= 0 && newItemIndex >= 0)
                    {
                        for (int i = Math.Min(control.fixedSelectedItemIndex, newItemIndex); i <= Math.Max(control.fixedSelectedItemIndex, newItemIndex); ++i)
                        {
                            items[i].IsSelected = true;

                            if (!control.SelectedItems.Contains(items[i]))
                            {
                                control.SelectedItems.Add(items[i]);
                            }
                        }
                    }
                    //else
                    //{
                    //    if (e.NewValue is ISelectable newItem) newItem.IsSelected = false;
                    //}
                }
                else if ((controlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    if (e.NewValue is ISelectable newItem)
                    {
                        SelectNewItem(control, control.oldValueKeeper, newItem, deselectOld: false);

                        if (control.SelectedItems.Count == 0 && control.oldValueKeeper != null && control.oldValueKeeper.IsSelected)
                        {
                            if (control.oldValueKeeper.IsSelected) control.SelectedItems.Add(control.oldValueKeeper);
                            else control.SelectedItems.Remove(control.SelectedItems.FirstOrDefault(x => x.Id == control.oldValueKeeper.Id));
                        }

                        if (newItem.IsSelected) control.SelectedItems.Add(newItem);
                        else control.SelectedItems.Remove(control.SelectedItems.FirstOrDefault(x => x.Id == newItem.Id));

                    }
                }
                else
                {
                    ClearSelectedItems(control);

                    SelectNewItem(control, control.oldValueKeeper, e.NewValue as ISelectable, deselectOld: true);
                }
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
            control.SelectedItems.Clear();
        }

        private static void SelectNewItem(VirtualizingItemsControl control, ISelectable oldValue, ISelectable newValue, bool deselectOld)
        {
            if (oldValue != null && deselectOld && oldValue != newValue) oldValue.IsSelected = false;
            if (newValue != null) newValue.IsSelected = !newValue.IsSelected;

            var args = new SelectionChangedEventArgs(new List<object>() { oldValue }, new List<object>() { newValue });

            if (newValue != null)
            {
                var items = ((IEnumerable<object>)control.ItemsSource).Cast<ISelectable>().ToList();

                if (newValue.IsSelected) control.fixedSelectedItemIndex = items.IndexOf(newValue);
                else control.fixedSelectedItemIndex = -1;

                if (newValue.IsSelected) control.OnSelected(args);
                else control.OnUnselected(args);
            }

            control.OnSelectionChanged(args);
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

        private static void OnItemTappedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
