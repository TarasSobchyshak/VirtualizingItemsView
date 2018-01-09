using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
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
            //ItemTemplateSelector.SelectTemplate(e.AddedItems.FirstOrDefault());
        }

        private void VirtualizingItemsControl_Unselected(object sender, SelectionChangedEventArgs e)
        {
            //ItemTemplateSelector.SelectTemplate(e.RemovedItems.FirstOrDefault());
        }

        #endregion



        #region Dependency Properties

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(VirtualizingItemsControl),
            new PropertyMetadata(null, OnSelectedItemChanged));

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList<object>),
            typeof(VirtualizingItemsControl),
            new PropertyMetadata(new List<object>(), OnSelectedItemsChanged));

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


        public IList<object> SelectedItems
        {
            get { return (IList<object>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
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
                selectable.IsSelected = !selectable.IsSelected;
                SelectedItem = selectable;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VirtualizingItemsControl control)
            {
                if (e.OldValue is ISelectable oldValue) oldValue.IsSelected = false;
                if (e.NewValue is ISelectable newValue) newValue.IsSelected = true;

                var args = new SelectionChangedEventArgs(new List<object>() { e.OldValue }, new List<object>() { e.NewValue });

                if (e.NewValue is ISelectable selectable)
                {
                    if (selectable.IsSelected) control.OnSelected(args);
                    else control.OnUnselected(args);
                }

                control.OnSelectionChanged(args);
            }
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
