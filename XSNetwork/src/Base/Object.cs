using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using XSNetwork.Interface;

namespace XSNetwork.Base
{
    public enum ASYNC_TYPE
    {
        ASYNC_ACCEPT = 0,
        ASYNC_CONNECT,
        ASYNC_CLOSE,
        ASYNC_RECV,
        ASYNC_SEND,
        ASYNC_MAX
    };

    public enum OBJECT_TYPE
    {
        TYPE_None,
        TYPE_Acceptor,
        TYPE_Connector,
        TYPE_Session,
        TYPE_Max
    };

    public enum OBJECT_STATUS
    {
        STATUS_None,
        STATUS_StartListen,
        STATUS_Listening,
        STATUS_StartConnect,
        STATUS_Connecting,
        STATUS_Closing,
        STATUS_Closed,
        STATUS_StartUse,
        STATUS_Using,    //only session
        STATUS_Freed,   //all free
    };

    public class Object : BaseLogEvent, IObject
    {
        protected OBJECT_TYPE m_Type;
        public OBJECT_TYPE ObjectType { get { return m_Type; } }

        protected OBJECT_STATUS m_Status;
        public OBJECT_STATUS ObjectStatus
        {
            get { return m_Status; }
            set { lock (this) { m_Status = value; } }
        }

        public bool IsListening
        {
            get
            {
                return this.ObjectType == OBJECT_TYPE.TYPE_Acceptor &&
                (m_Status == OBJECT_STATUS.STATUS_StartListen || m_Status == OBJECT_STATUS.STATUS_Listening);
            }
        }
        public bool IsConnecting
        {
            get
            {
                return this.ObjectType == OBJECT_TYPE.TYPE_Connector &&
                (m_Status == OBJECT_STATUS.STATUS_StartConnect || m_Status == OBJECT_STATUS.STATUS_Connecting);
            }
        }
        public bool IsUsing
        {
            get
            {
                return IsConnecting || IsListening ||
                (m_Status == OBJECT_STATUS.STATUS_StartUse || m_Status == OBJECT_STATUS.STATUS_Using);
            }
        }


        protected Socket m_Socket;
        protected IPEndPoint m_LocalIPEndPoint;
        protected IPEndPoint m_RemoteIPEndPoint;

        public String LocalAddress { get { return m_LocalIPEndPoint == null ? "" : m_LocalIPEndPoint.Address.ToString(); } }
        public Int32 LocalPort { get { return m_LocalIPEndPoint == null ? 0 : m_LocalIPEndPoint.Port; } }
        public IPEndPoint LocalIPEndPoint { get { return m_LocalIPEndPoint; } }

        public String RemoteAddress { get { return m_RemoteIPEndPoint == null ? "" : m_RemoteIPEndPoint.Address.ToString(); } }
        public Int32 RemotePort { get { return m_RemoteIPEndPoint == null ? 0 : m_RemoteIPEndPoint.Port; } }
        public IPEndPoint RemoteIPEndPoint { get { return m_RemoteIPEndPoint; } }

        public Object(OBJECT_TYPE type = OBJECT_TYPE.TYPE_None)
        {
            m_Type = type;
            m_Status = OBJECT_STATUS.STATUS_None;
        }

        public virtual void dispose()
        {
            // 关闭套接字
            if (m_Socket != null)
            {
                this.close();

                if (m_Status != OBJECT_STATUS.STATUS_Freed)
                {
                    m_Socket.Dispose();
                    m_Socket = null;
                }
            }

            m_LocalIPEndPoint = null;
            m_RemoteIPEndPoint = null;
            m_Status = OBJECT_STATUS.STATUS_Freed;
        }

        public virtual bool initialize()
        {
            m_Status = OBJECT_STATUS.STATUS_None;

            return true;
        }

        public virtual void close()
        {
            //
            if (this.IsConnecting ||
                this.IsListening ||
                m_Status == OBJECT_STATUS.STATUS_StartUse || m_Status == OBJECT_STATUS.STATUS_Using)
            { this.ObjectStatus = OBJECT_STATUS.STATUS_Closing; }

            //
            if (m_Status == OBJECT_STATUS.STATUS_Closing)
            {
                lock (this)
                {
                    if (m_Socket != null)
                    {
                        //m_Socket.Shutdown(SocketShutdown.Both);
                        m_Socket.Close();
                    }
                    this.ObjectStatus = OBJECT_STATUS.STATUS_Closed;
                }
            }
        }
    }
}
