using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

//
namespace XSNetwork.Acceptor
{
    public class AcceptorSync<T> : Acceptor<T>
        where T : Session.Session
    {
        protected int m_ThreadNum;
        private bool m_ThreadExitFlag;
        private Thread[] m_ThreadList;
        private ManualResetEvent[] m_ThreadEvent;
        private SocketAsyncEventArgs[] m_AsyncEventArgs;

        public AcceptorSync(Base.Desc desc)
            : base(desc)
        {
            m_ThreadNum = desc.m_AcceptThreadNum;

        }

        //
        public override void dispose()
        {
            // 循环等待线程结束
            m_ThreadExitFlag = true;
            WaitHandle.WaitAll(m_ThreadEvent, 3000);
            for (int i = 0; i < m_ThreadNum; i++)
            {
                switch (m_ThreadList[i].ThreadState)
                {
                    case ThreadState.Stopped:
                    case ThreadState.Aborted:
                        {
                            break;
                        }
                    default:
                        {
                            m_ThreadList[i].Abort();
                            break;
                        }
                }

                m_ThreadEvent[i].Close();
                m_ThreadEvent[i].Dispose();
                m_ThreadList[i] = null;
                m_ThreadEvent[i] = null;
            }

            m_ThreadEvent = null;
            m_ThreadList = null;

            base.dispose();
        }
        
        // 这将初始化并创建线程
        public override bool initialize()
        {
            if (!base.initialize())
            { return false; }

            //
            m_ThreadList = new Thread[m_ThreadNum];
            m_ThreadEvent = new ManualResetEvent[m_ThreadNum];
            m_AsyncEventArgs = new SocketAsyncEventArgs[m_ThreadNum];

            for (int i = 0; i < m_ThreadNum; i++)
            {
                m_AsyncEventArgs[i] = new SocketAsyncEventArgs();
                m_AsyncEventArgs[i].Completed += new EventHandler<SocketAsyncEventArgs>(Event_AsyncAccept);

                m_ThreadEvent[i] = new ManualResetEvent(false);

                m_ThreadList[i] = new Thread(new ParameterizedThreadStart(Thread_AcceptWork));
                m_ThreadList[i].Start(new Tuple<Int32, EventWaitHandle, object>(i, m_ThreadEvent[i], null));
            }

            //
            return true;
        }

        private void Thread_AcceptWork(object args)
        {
            Tuple<Int32, EventWaitHandle, object> param_list = (Tuple<Int32, EventWaitHandle, object>)(args);
            int thread_index = param_list.Item1;
            EventWaitHandle thread_event = param_list.Item2;
            object thread_token = param_list.Item3;

            Logout("[Acceptor] (thread[" + thread_index + "]) : Starting.");

            if (thread_index < m_ThreadNum)
            {
                while (!m_ThreadExitFlag)
                {
                    if (m_AsyncEventArgs[thread_index].AcceptSocket != null)
                    {
                        continue;
                    }

                    bool result = m_Socket.AcceptAsync(m_AsyncEventArgs[thread_index]);
                    if (!result)
                    {
                        Error(-1, "[Error] : " + m_AsyncEventArgs[thread_index].SocketError.ToString());
                        continue;
                    }
                }
            }

            Logout("[Acceptor] (thread[" + thread_index + "]) : Ending.");

            thread_event.Set();
        }

        private void Event_AsyncAccept(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                args.AcceptSocket.Close();
                args.AcceptSocket.Dispose();
                args.AcceptSocket = null;
                return;
            }

            Session.Session session = this.AllocSession();
            if (session == null)
            {
                args.AcceptSocket.Close();
                args.AcceptSocket.Dispose();

                Error(-1, "[Error] : alloc session is null.");
            }
            else
            {
                if (session.FreeSessionCaller == null ||
                    !session.FreeSessionCaller.Equals(new Base.FreeSessionMethodCaller(FreeSessionCaller)))
                {
                    session.FreeSessionCaller += new Base.FreeSessionMethodCaller(FreeSessionCaller);
                }

                if (!session.initialize(args.AcceptSocket, (IPEndPoint)m_Socket.LocalEndPoint, (IPEndPoint)args.AcceptSocket.RemoteEndPoint))
                {
                    this.FreeSession((T)session);

                    args.AcceptSocket.Close();
                    args.AcceptSocket.Dispose();

                    Error(-1, "[Error] : session(" + session.Index + ") : initialize fail.");
                }
                else
                {
                    if (!this.ProcessAccept(session))
                    {
                        session.close(true);

                        this.FreeSession((T)session);
                    }
                }
            }

            //C# 存在隐藏的指针,Are you kidding me?
            //如果定义成数组,这里是指针哦!
            args.AcceptSocket = null;
        }

        public void FreeSessionCaller(object caller)
        {
            this.FreeSession((Session.Session)caller);
        }

        protected virtual bool ProcessAccept(Session.Session session)
        {
            if (!session.ProcessAccept())
            {
                return false;
            }
            return true;
        }
    }
}
