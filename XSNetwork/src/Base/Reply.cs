using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSNetwork.Base
{
    public class ReplyT<T> : Base.Object, Interface.ISessionAlloc
        where T : Session.Session
    {
        protected int m_SessionNum;
        protected Session.SessionPool<T> m_SessionPool;

        public ReplyT(Base.Desc desc, OBJECT_TYPE type)
            : base(type)
        {
            m_SessionNum = desc.m_AcceptSessionNum;
            m_SessionPool = new Session.SessionPool<T>(m_SessionNum, this);
        }

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

            //
            return true;
        }

        public Session.Session AllocSession()
        {
            if (m_SessionPool.IdleCount <= 0)
            { return null; }

            Session.Session session = m_SessionPool.Alloc();
            return session;
        }

        public void FreeSession(Session.Session session)
        {
            if (session != null)
            {
                if (session.IsInitialize)
                { session.dispose(); }

                m_SessionPool.Free((T)session);
                session = null;
            }
        }
    }
}
