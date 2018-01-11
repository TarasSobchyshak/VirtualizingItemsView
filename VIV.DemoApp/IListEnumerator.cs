using System;
using System.Collections;
using System.Collections.Generic;

namespace VIV.DemoApp
{
    public struct IListEnumerator<T> : IEnumerator<T>, IEnumerator
    {
        private IList list;
        private int index;
        private T current;

        internal IListEnumerator(IList list)
        {
            this.list = list;
            index = 0;
            current = default(T);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {

            IList localList = list;

            if ( ((uint)index < (uint)localList.Count))
            {
                current = (T)localList[index];
                index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            index = list.Count + 1;
            current = default(T);
            return false;
        }

        public T Current
        {
            get
            {
                return current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                if (index == 0 || index == list.Count + 1)
                {
                    throw new InvalidOperationException("ExceptionResource.InvalidOperation_EnumOpCantHappen");
                    //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                }
                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            index = 0;
            current = default(T);
        }

    }
}
