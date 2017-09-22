using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

//
namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Application application = new Application();
            if (application.initialize())
            {
                application.running();
            }
            application.dispose();
            return;
        }
    }

    class Application
    {
        private Thread m_ThreadMain;
        private bool m_ThreadMainExitFlag;
        private ManualResetEvent m_ThreadMainEvent;

        private XSNetwork.Acceptor.AcceptorSync<TestSession> m_Acceptor;

        public Application()
        {

        }

        public virtual bool initialize()
        {
            if (!CreateMainThread())
            { return false; }

            return true;
        }

        public virtual void dispose()
        {
            DestoryMainThread();
        }

        public virtual void running()
        {
            while (true)
            {
                Thread.Sleep(10);

                String command = Console.ReadLine();
                switch (command.ToLower())
                {
                    case "quit":
                        {
                            goto exit_flag;
                        }
                }
            }
        exit_flag:
            return;
        }

        protected bool CreateMainThread()
        {
            try
            {
                m_ThreadMainEvent = new ManualResetEvent(false);
                m_ThreadMain = new Thread(new ParameterizedThreadStart(ThreadMainwork));
                m_ThreadMain.Start(null);
                m_ThreadMain.Priority = ThreadPriority.AboveNormal;
            }
            catch (Exception e)
            {
                Console.WriteLine("[Exception] : " + e.Message);
                return false;
            }
            return true;
        }

        protected void DestoryMainThread()
        {
            //
            m_ThreadMainExitFlag = true;

            if (!m_ThreadMainEvent.WaitOne(3000))
            { m_ThreadMain.Abort(); }

            m_ThreadMainEvent.Close();
            m_ThreadMainEvent.Dispose();
            m_ThreadMainEvent = null;
            m_ThreadMain = null;
        }

        private void ThreadMainwork(object args)
        {
            Console.WriteLine("[Application] : Main thread starting ...");
            try
            {
                if (this.OnInitialize())
                {
                    while (!m_ThreadMainExitFlag)
                    {
                        Thread.Sleep(10);
                    }
                }
                this.OnRelease();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Exception] : " + e.Message);
            }

            Console.WriteLine("[Application] : Main thread ending ...");
            m_ThreadMainEvent.Set();
        }

        protected virtual void OnRelease()
        {
            m_Acceptor.dispose();
            m_Acceptor = null;
        }

        protected virtual bool OnInitialize()
        {
            XSNetwork.Base.Desc desc = new XSNetwork.Base.Desc();
            desc.m_Port = 7777;
            desc.m_AcceptSessionNum = 10;
            desc.m_AcceptThreadNum = 2;

            m_Acceptor = new XSNetwork.Acceptor.AcceptorSync<TestSession>(desc);
            if (!m_Acceptor.initialize())
            { return false; }

            return true;
        }
    }

    class TestSession : XSNetwork.Session.Session
    {
        public TestSession(XSNetwork.Session.SessionType type, int index, object token)
            : base(type, index, token)
        {
            this.Event_Init += new XSNetwork.Base.EventSession_InitHandler(OnInit);
            this.Event_Free += new XSNetwork.Base.EventSession_FreeHandler(OnFree);
        }
        
        public virtual void OnFree(XSNetwork.Session.Session session)
        {
        }

        public virtual bool OnInit(XSNetwork.Session.Session session)
        {
            return true;
        }

        protected override bool OnAccept()
        {
            if (!base.OnAccept()) { return false; }

            Console.WriteLine("[" + this.Index +"] (Accept) " + this.RemoteAddress + ":" + this.RemotePort);

            this.close(true);
            return true;
        }
        protected override void OnClose(bool passive = false)
        {
            Console.WriteLine("[" + this.Index + "] (Close) " + (!passive ? "" : "(passive) ") + this.RemoteAddress + ":" + this.RemotePort);
        }

        protected override void OnRecv(byte[] buffer, int length)
        {
            String temp = "";
            for (int i = 0; i < length; i++)
            {
                if (temp.Length > 0) { temp += ","; }
                temp += String.Format("{0:X2}", buffer[i]);
            }
            Console.WriteLine(temp);
        }
    }
}
