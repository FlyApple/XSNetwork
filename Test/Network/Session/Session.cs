using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

//
namespace XSNetwork.Session
{
    public enum SessionType
    {
        Type_Acceptor,
        Type_Connector,
        Type_Max
    }

    public class Session : Base.Object
    {
        private int m_Index;
        public int Index { get { return m_Index; } }

        protected bool m_IsInitialize;
        public bool IsInitialize { get { return m_IsInitialize; } }

        protected Base.Object m_Acceptor;

        protected Buffer.BufferManager m_SendBuffer;
        protected Buffer.BufferManager m_RecvBuffer;

        private SocketAsyncEventArgs[]      m_AsyncEvents;
        private ManualResetEvent            m_AsyncBlockCloseEvent;

        public Base.EventSession_InitHandler Event_Init;
        public Base.EventSession_FreeHandler Event_Free;
        public Base.EventSession_AcceptHandler Event_Accept;
        public Base.EventSession_CloseHandler Event_Close;
        public Base.EventSession_RecvHandler Event_Recv;

        public Session(SessionType type, int index, object token)
        {
            m_Index = index;
            m_Acceptor = type == SessionType.Type_Acceptor ? (Base.Object)token : null;

            m_IsInitialize = false;

            m_SendBuffer = new Buffer.BufferManager(8192);
            m_RecvBuffer = new Buffer.BufferManager(8192);

            m_AsyncEvents = new SocketAsyncEventArgs[(int)Base.ASYNC_TYPE.ASYNC_MAX];
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_CLOSE] = new SocketAsyncEventArgs();
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_CLOSE].Completed += new EventHandler<SocketAsyncEventArgs>(Event_AsyncIOCompleted);
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_CLOSE].UserToken = this;
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_SEND] = new SocketAsyncEventArgs();
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_SEND].SetBuffer(new byte[Buffer.BufferManager.ElementLength], 0, Buffer.BufferManager.ElementLength);
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_SEND].Completed += new EventHandler<SocketAsyncEventArgs>(Event_AsyncIOCompleted);
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_SEND].UserToken = this;
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_RECV] = new SocketAsyncEventArgs();
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_RECV].SetBuffer(new byte[Buffer.BufferManager.ElementLength], 0, Buffer.BufferManager.ElementLength);
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_RECV].Completed += new EventHandler<SocketAsyncEventArgs>(Event_AsyncIOCompleted);
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_RECV].UserToken = this;

            m_AsyncBlockCloseEvent = new ManualResetEvent(false);
        }

        public override void dispose()
        {
            base.dispose();

            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_SEND].SetBuffer(0, Buffer.BufferManager.ElementLength);
            m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_RECV].SetBuffer(0, Buffer.BufferManager.ElementLength);

            m_SendBuffer.ClearData();
            m_RecvBuffer.ClearData();

            if (Event_Free != null)
            { Event_Free(this); }

            m_IsInitialize = false;
        }

        public virtual bool initialize(Socket socket, IPEndPoint local, IPEndPoint remote)
        {
            if (!base.initialize())
            { return false; }

            m_Socket = socket;
            m_LocalIPEndPoint = local;
            m_RemoteIPEndPoint = remote;

            if (Event_Init != null && !Event_Init(this))
            { return false; }
            
            //
            m_AsyncBlockCloseEvent.Reset();

            //
            m_IsInitialize = true;
            return true;
        }

        public virtual void close(bool block = false)
        {
            //不调用父类关闭句柄
            //采用异步关闭
            if (!m_Socket.DisconnectAsync(m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_CLOSE]))
            { Event_AsyncIOCompleted(m_Socket, m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_CLOSE]); }

            //默认不需要阻塞,但是在清除回收的时候,需要阻塞
            if (block)
            { m_AsyncBlockCloseEvent.WaitOne(500); }
        }

        public virtual int send(byte[] data, int offset, int length)
        {
            if (data.Length + offset < length) { return -1; }


            return 0;
        }

        private void Event_AsyncIOCompleted(object sender, SocketAsyncEventArgs args)
        {
            switch (args.LastOperation)
            {
                case SocketAsyncOperation.Disconnect:
                    {
                        this.ProcessClose(false);
                        break;
                    }
                case SocketAsyncOperation.Receive:
                    {
                        if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
                        {
                            int length = m_RecvBuffer.PushData(args.Buffer, args.BytesTransferred);
                            if (length > 0)
                            {
                                args.SetBuffer(0, Buffer.BufferManager.ElementLength);
                            }

                            if ((length = this.ProcessParseData()) < 0)
                            {
                                Error(-1, "[Error] : Parse data error.");
                            }

                            if (length > 0)
                            {
                                m_RecvBuffer.PopData(length);
                            }

                            if (!this.ProcessReceive(args))
                            {
                                Error(-1, "[Error] : Start process recv error, Closing session.");

                                this.close();
                            }
                        }
                        else
                        {
                            this.ProcessClose(true);
                        }

                        break;
                    }
            }
        }

        public virtual bool ProcessAccept()
        {
            if (!this.OnAccept())
            { return false;  }

            if (this.Event_Accept != null && !this.Event_Accept(this))
            { return false; }

            //C# 存在隐藏的指针,Are you kidding me?
            //如果定义成非数组,这里是一个副本哦!
            if (!this.ProcessReceive(m_AsyncEvents[(int)Base.ASYNC_TYPE.ASYNC_RECV]))
            {
                return false;
            }
            return true;
        }

        public virtual bool ProcessClose(bool passive)
        {
            this.OnClose(passive);

            if (this.Event_Close != null)
            { this.Event_Close(passive); }

            //调用父类关闭句柄
            base.close();

            //
            m_AsyncBlockCloseEvent.Set();
            return true;
        }

        public virtual bool ProcessReceive(SocketAsyncEventArgs args)
        {
            try
            {
                //
                if (!m_Socket.ReceiveAsync(args))
                { Event_AsyncIOCompleted(this.m_Socket, args); }
            }
            catch (Exception e)
            {
                Error(-1, "[Exception] : " + e.Message);
                return false;
            }
            return true;
        }

        public int ProcessParseData()
        {
            int parse_length = 0;

            int length = 0;
            int offset = 0;
            byte[] buffer = new byte[Buffer.BufferManager.ElementLength];
            while ((length = m_RecvBuffer.GetData(offset, buffer, 0)) > 0)
            {
                this.OnRecv(buffer, length);

                if (Event_Recv != null) 
                { Event_Recv(buffer, length); }

                offset += length;
                parse_length += length;
            }

            return length < 0 ? -1 : parse_length;
        }

        protected virtual bool OnAccept() { return true; }
        protected virtual void OnClose(bool passive = false) { }
        protected virtual void OnRecv(byte[] buffer, int length) { }
    }
}
