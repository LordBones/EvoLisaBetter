using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenArt.Core.Classes.Misc
{
    public class ObjectRecyclerTS<T> where T : new()
    {
        private const int CONST_LOCK = 1;
        private const int CONST_UNLOCK = 0;

        private readonly int CONST_MaxElementForRecycle;

        private T [] _objects = new T[0];
        private int _objIndex = 0;
        SpinWait spinner = new SpinWait();

        int _isLock = CONST_UNLOCK;

        FastLock _fastLock = new FastLock();

        public ObjectRecyclerTS()
            :this(10)
        {

        }

        public ObjectRecyclerTS(int maxElementsForRecycle)
        {
            CONST_MaxElementForRecycle = maxElementsForRecycle;
            _objects = new T[CONST_MaxElementForRecycle];
        }

        public T GetNewOrRecycle()
        {
            //Helper_LockSection();

            //try
            {
                using (_fastLock.Lock())
                {
                    if (_objIndex > 0)
                    {
                        _objIndex--;
                        return _objects[_objIndex];
                    }
                }
                
            }
            //finally
            //{
            //    Helper_UnlockSection();
            //}

            return Instance.Invoke();// new T();
        }

        public static readonly Func<T> Instance =
     Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();

        public void PutForRecycle(T[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                PutForRecycle(objects[i]);
            }
        }

        public void PutForRecycle(T pobject)
        {
            //Helper_LockSection();

            //try
            //{
                using (_fastLock.Lock())
                {
                if (_objIndex < this.CONST_MaxElementForRecycle)
                {
                    _objects[_objIndex] =pobject;
                    _objIndex++;
                }
                }
            //}
            //finally
            //{
            //    Helper_UnlockSection();
            //}

        }

        public void Clear()
        {
            //Helper_LockSection();

            //try
            //{
                using (_fastLock.Lock())
                {
                    _objIndex = 0;
                }
            //}
            //finally
            //{
            //    Helper_UnlockSection();
            //}

        }

        private void Helper_LockSection()
        {
            if (Interlocked.CompareExchange(ref _isLock, CONST_LOCK, CONST_UNLOCK) != CONST_UNLOCK)
            {
                

                do
                {
                    spinner.SpinOnce();
                }
                while (Interlocked.CompareExchange(ref _isLock, CONST_LOCK, CONST_UNLOCK) != CONST_UNLOCK);
            }
        }

        private void Helper_UnlockSection()
        {
            //_isLock = CONST_UNLOCK;
            if (Interlocked.CompareExchange(ref _isLock, CONST_UNLOCK, CONST_LOCK) != CONST_LOCK)
            {
                throw new Exception("Toto nesmi nastat");

                var spinner = new SpinWait();

                do
                {
                    spinner.SpinOnce();
                }
                while (Interlocked.CompareExchange(ref _isLock, CONST_UNLOCK, CONST_LOCK) != CONST_LOCK);
            }
        }
    }

    /*public class ObjectRecyclerTS<T> where T : new()
    {
        private Stack<T> _objects = new Stack<T>();
        private object _Lock = new object();

        public T GetNewOrRecycle()
        {
            lock (_Lock)
            {
                if (_objects.Count > 0)
                    return _objects.Pop();
            }

            return new T();
        }

        public void PutForRecycle(T[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                PutForRecycle(objects[i]);
            }
        }

        public void PutForRecycle(T pobject)
        {
            lock (_Lock)
            {
                _objects.Push(pobject);
            }
        }

        public void Clear()
        {
            lock (_Lock)
            {
                _objects.Clear();
            }
        }
    }*/
}
