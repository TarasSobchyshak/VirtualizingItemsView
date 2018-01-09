using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VIV.DemoApp.MVVM
{
    public abstract class BindableObject : INotifyPropertyChanged, IDisposable
    {
        #region Private members

        private Dictionary<string, object> _properties = new Dictionary<string, object>();

        #endregion

        #region Protected virtual

        protected virtual T GetProperty<T>([CallerMemberName] string propertyName = null)
        {
            if (_properties.TryGetValue(propertyName, out object output))
            {
                return (T)output;
            }

            return default(T);
        }

        protected virtual bool SetProperty<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(GetProperty<T>(propertyName), value))
            {
                return false;
            }

            _properties[propertyName] = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion


        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #region IDisposable implementation

        protected bool Disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {

                }

            }

            Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
