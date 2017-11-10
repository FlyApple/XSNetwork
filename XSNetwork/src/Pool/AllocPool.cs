using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

using XSNetwork.Interface;

namespace XSNetwork.Pool
{
    public class AllocBasePool<T> : IPool<T>
    {
        private int m_TotalCount;
        public int TotalCount { get { return m_TotalCount; } }

        private int m_IdleCount;
        public int IdleCount { get { return m_IdleCount; } }

        private IPoolCreator<T> m_PoolCreator;
        private List<T> m_AllocList;
        private List<T> m_IdleList;
        //private ConcurrentStack<T>  m_IdleList;


        public AllocBasePool(int count, IPoolCreator<T> creator)
        {
            m_TotalCount = count;
            m_IdleCount = count;
            m_PoolCreator = creator;

            m_AllocList = new List<T>(count);
            foreach (var v in creator.Create(count))
            {
                m_AllocList.Add(v);
            }

            //m_IdleList = new ConcurrentStack<T>(m_AllocList);
            m_IdleList = new List<T>(m_AllocList);
        }

        public virtual void dispose()
        {
            if (m_IdleList != null)
            {
                m_IdleCount = 0;

                m_IdleList.Clear();
                m_IdleList = null;
            }

            if (m_AllocList != null)
            {
                m_TotalCount = 0;

                m_AllocList.Clear();
                m_AllocList = null;
            }

            m_PoolCreator = null;
        }

        public T Alloc()
        {
            T value = default(T);
            if (m_IdleList.Count > 0)
            {
                //if (m_IdleList.TryPop(out value))
                value = m_IdleList.First<T>();
                m_IdleList.RemoveAt(0);
                {
                    Interlocked.Decrement(ref m_IdleCount);
                }
            }
            return value;
        }

        public void Free(T value)
        {
            if (!m_IdleList.Contains<T>(value))
            {
                //m_IdleList.Push(value);
                m_IdleList.Add(value);

                Interlocked.Increment(ref m_IdleCount);
            }
        }
    }
}
