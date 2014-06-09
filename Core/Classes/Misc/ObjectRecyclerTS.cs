using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenArt.Core.Classes.Misc
{
    public class ObjectRecyclerTS<T> where T : new()
    {
        private const int CONST_LOCK = 1;
        private const int CONST_UNLOCK = 0;

        private Stack<T> _objects = new Stack<T>();
        SpinWait spinner = new SpinWait();

        int _isLock = CONST_UNLOCK;

        FastLock _fastLock = new FastLock();

        public T GetNewOrRecycle()
        {
            //Helper_LockSection();

            //try
            {
                using (_fastLock.Lock())
                {
                    if (_objects.Count > 0)
                        return _objects.Pop();
                }
                
            }
            //finally
            //{
            //    Helper_UnlockSection();
            //}

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
            //Helper_LockSection();

            //try
            //{
                using (_fastLock.Lock())
                {
                    if (_objects.Count < 1500)
                        _objects.Push(pobject);
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
                    _objects.Clear();
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
