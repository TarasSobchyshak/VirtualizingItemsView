using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Virtualization;

namespace VIV.DemoApp
{
    public class VirtualizingCollection<T> : IList, INotifyCollectionChanged, INotifyPropertyChanged, IEnumerable<T>, ISelectableCollection
        where T : ISelectable, IVirtualizing
    {
        #region Constructors

        public VirtualizingCollection(IItemsProvider<T> itemsProvider, int pageSize, int count = -1)
        {
            _itemsProvider = itemsProvider ?? throw new ArgumentNullException(nameof(itemsProvider));
            _pageSize = pageSize > 0 ? pageSize : throw new ArgumentOutOfRangeException(nameof(pageSize));

            _items = new List<T>();
            _count = count;
            _currentPageIndex = -1;
            _updateOffset = _pageSize / 2;

            SelectedItemsIds = new List<string>();
        }

        #endregion


        #region Fields

        private Object _syncRoot;
        private SimpleMonitor _monitor = new SimpleMonitor();
        private IItemsProvider<T> _itemsProvider;

        private IList<T> _items;


        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        private int _count;
        private readonly int _pageSize;
        private readonly int _updateOffset;
        private int _currentPageIndex;
        private bool _isUpdating;

        #endregion


        #region Protected Fields and Properties

        protected IList<T> Items => _items;

        #endregion


        #region Properties

        public IList<string> SelectedItemsIds { get; set; }
        public bool AllItemsSelected { get; set; }

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public int Count
        {
            get
            {
                if (_count < 0)
                {
                    _count = 0;
                    LoadCount();
                }
                return _count;
            }
        }

        public bool IsSynchronized => false;

        public object SyncRoot => _syncRoot;

        public T this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }

        #endregion


        #region Data Virtualization Methods

        protected T GetItem(int index)
        {
            if (!_isUpdating && CurrentPageChanged(index))
            {
                UpdateItemsAsync(index);
            }

            var item = default(T);
            if (Items.Count == 0) return item;

            var modIndex = index % _updateOffset;

            item = Items[index < _pageSize ? index : modIndex + _updateOffset];

            item.Index = index;
            item.IsSelected = SelectedItemsIds?.Contains(item.Id) == true;
            if (item.IsSelected)
            {

            }

            return item;
        }

        protected void SetItem(int index, T item)
        {
            if (Items.IsReadOnly)
            {
                throw new NotSupportedException("Operation not supported on read-only collection");
            }

            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            throw new NotSupportedException($"'{nameof(SetItem)}' method is not supported.");
            //CheckReentrancy();
            //T originalItem = this[index];
            //base.SetItem(index, item);

            //OnPropertyChanged(IndexerName);
            //OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        private async void UpdateItemsAsync(int index)
        {
            _isUpdating = true;
            _items = new List<T>();

            var divIndex = index / _updateOffset;
            try
            {
                if (index < _updateOffset)
                {
                    _items = await LoadRangeAsync(0, _pageSize);
                    //OnCollectionChanged((IList)_itemsProvider.GetLoadedItems(), 0);

                    return;
                }

                _items = await LoadRangeAsync(_updateOffset * (divIndex - 1), _pageSize);
                //OnCollectionChanged((IList)_itemsProvider.GetLoadedItems(), _updateOffset * (divIndex - 1));
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private bool CurrentPageChanged(int index)
        {
            var newPageIndex = index / _updateOffset;
            var changed = _currentPageIndex != newPageIndex;

            if (changed)
            {
                _currentPageIndex = newPageIndex;
            }

            return changed;
        }

        #endregion


        #region ItemsProvider methods

        private async Task<IList<T>> LoadRangeAsync(int startIndex, int length)
        {
            var range = await _itemsProvider.FetchRangeAsync(startIndex, length);

            return range;
        }

        private async void LoadCount()
        {
            _count = await _itemsProvider.FetchCountAsync();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(CountString));
        }

        #endregion


        #region Private Methods

        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCollectionChanged(IList newItems, int startingIndex)
        {
            if (startingIndex < 0) throw new ArgumentOutOfRangeException(nameof(startingIndex));

            if (startingIndex < _updateOffset)
            {
                //var list = new T[_pageSize];
                for (int i = 0; i < _pageSize && i < newItems.Count; ++i)
                {
                    //list[i] = default(T);
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(
                            action: NotifyCollectionChangedAction.Replace,
                            newItem: newItems[i],
                            oldItem: default(T),
                            index: startingIndex + i
                            ));
                }
                //CollectionChanged?.Invoke(this,
                //       new NotifyCollectionChangedEventArgs(
                //           action: NotifyCollectionChangedAction.Replace,
                //           newItems: newItems,
                //           oldItems: list,
                //           startingIndex: startingIndex
                //           ));
            }
            else
            {
                for (int i = _updateOffset; i < newItems.Count && i + startingIndex < _count; ++i)
                {
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(
                            action: NotifyCollectionChangedAction.Replace,
                            newItem: newItems[i],
                            oldItem: default(T),
                            index: startingIndex + i
                            ));
                }
            }
        }

        #endregion


        #region Private Static Methods

        private static bool IsCompatibleObject(object value)
        {
            // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            return ((value is T) || (value == null && default(T) == null));
        }

        #endregion


        #region Protected Methods

        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                // we can allow changes if there's only one listener - the problem
                // only arises if reentrant changes make the original event args
                // invalid for later listeners.  This keeps existing code working
                // (e.g. Selector.SelectedItems).
                if (CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("ObservableCollectionReentrancyNotAllowed");
                }
            }
        }

        #endregion


        #region IList implementation


        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            return new IListEnumerator<T>(this);
        }


        #endregion


        #region INotifyCollectionChanged implementation

        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add
            {
                CollectionChanged += value;
            }

            remove
            {
                CollectionChanged -= value;
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }
        }

        #endregion INotifyPropertyChanged implementation


        #region INotifyPropertyChanged implementation

        protected virtual event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }
            remove
            {
                PropertyChanged -= value;
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        #endregion INotifyPropertyChanged implementation


        #region Implicit Interfaces Implementations

        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (IEnumerator<T>)GetEnumerator();
        }

        int ICollection.Count => Count;

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    if (Items is ICollection c)
                    {
                        _syncRoot = c.SyncRoot;
                    }
                    else
                    {
                        System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);
                    }
                }

                return _syncRoot;
            }
        }

        #endregion


        #region Private types

        private class SimpleMonitor : IDisposable
        {
            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }

            public bool Busy { get { return _busyCount > 0; } }

            int _busyCount;
        }

        #endregion
    }
}
