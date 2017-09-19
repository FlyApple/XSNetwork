using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using XSNetwork.Interface;

//
namespace XSNetwork.Creator
{
    class SessionCreator : IPoolCreator<Session.Session>
    {
        private Session.SessionType m_Type;
        private object m_Token;

        public SessionCreator(Session.SessionType type, object token)
        {
            m_Type = type;
            m_Token = token;
        }

        public IEnumerable<Session.Session> Create(int count)
        {
            return new SessionEnumerable(m_Type, count, m_Token);
        }

        class SessionEnumerable : IEnumerable<Session.Session>
        {
            private Session.SessionType m_Type;
            private int m_Count;
            private object m_Token;

            public SessionEnumerable(Session.SessionType type, int count, object token)
            {
                m_Type = type;
                m_Count = count;
                m_Token = token;
            }

            public IEnumerator<Session.Session> GetEnumerator()
            {
                int count = m_Count;
                for (int i = 0; i < count; i++)
                {
                    Session.Session session = new Session.Session(m_Type, i + 1, m_Token);
                    yield return session;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
