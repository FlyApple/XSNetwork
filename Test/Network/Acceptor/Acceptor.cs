using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;


namespace XSNetwork.Acceptor
{
    //
    public class Acceptor : Base.Object
    {
        protected int m_SessionNum;
        protected Session.SessionPool m_SessionPool;

        public Acceptor(Base.Desc desc)
        {
            m_SessionNum = desc.m_AcceptSessionNum;
            m_LocalIPEndPoint = new IPEndPoint(IPAddress.Parse(desc.m_Address), desc.m_Port);

            m_SessionPool = new Session.SessionPool(Session.SessionType.Type_Acceptor, m_SessionNum, this);
        }

        //
        public override void dispose()
        {
            if (m_SessionPool != null)
            {
                m_SessionPool.dispose();
                m_SessionPool = null;
            }

            base.dispose();
        }

        // 这将初始化并创建线程
        public override bool initialize()
        {
            if (!base.initialize())
            { return false; }

            if (!CreateListen())
            { return false; }

            //
            return true;
        }

        private bool CreateListen()
        {
            try
            {
                m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_Socket.Bind(m_LocalIPEndPoint); m_LocalIPEndPoint = (IPEndPoint)m_Socket.LocalEndPoint;
                m_Socket.Listen(5);

                m_IsListening = true;

                Logout("[Acceptor] (listen) : " + this.LocalAddress + ":" + this.LocalPort);
            }
            catch (Exception e)
            {
                Error(e.HResult, "[Exception] : " + e.Message);
            }
            return true;
        }

        protected virtual Session.Session AllocSession()
        {
            if (m_SessionPool.IdleCount <= 0)
            { return null; }

            Session.Session session = m_SessionPool.Alloc();
            return session;
        }

        protected virtual void FreeSession(Session.Session session)
        {
            if (session != null)
            {
                if (session.IsInitialize)
                { session.dispose(); }

                m_SessionPool.Free(session);
                session = null;
            }
        }
    }
}
