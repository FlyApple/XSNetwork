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
    public class Acceptor<T> : Base.ReplyT<T>
        where T : Session.Session
    {
        public Acceptor(Base.Desc desc)
            : base(desc, Base.OBJECT_TYPE.TYPE_Acceptor)
        {
            m_LocalIPEndPoint = new IPEndPoint(IPAddress.Parse(desc.m_Address), desc.m_Port);
        }

        //
        public override void dispose()
        {
            base.dispose();
        }

        // 这将初始化
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
    }
}
