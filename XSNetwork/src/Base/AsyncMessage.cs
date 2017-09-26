using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

//
namespace XSNetwork.Base
{
    public class AsyncMessage
    {
        private long m_Value;
        private ManualResetEvent m_AsyncEvent;

        public AsyncMessage()
        {
            m_Value = 0;
            m_AsyncEvent = new ManualResetEvent(false);
        }

        public void dispose()
        {
            m_Value = 0;

            m_AsyncEvent.Close();
            m_AsyncEvent.Dispose();
            m_AsyncEvent = null;
        }

        public bool IsActive { get { return m_Value > 0; } }

        public void Reset()
        {
            lock (this)
            {
                m_Value = 0;
                m_AsyncEvent.Reset();
            }
        }

        public void ActiveAndWait(int timeout)
        {
            lock (this)
            {
                m_Value++;
                if (m_Value < 1)
                { m_AsyncEvent.WaitOne(timeout); }
            }
        }

        public void ResponseMessage()
        {
            lock (this)
            {
                if (m_Value > 0)
                { 
                    m_Value = m_Value <= 0 ? 0 : m_Value--;
                    m_AsyncEvent.Set();
                }
            }
        }
    }
}
