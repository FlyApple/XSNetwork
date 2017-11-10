using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSNetwork.Base
{
    public delegate void EventLogoutHandler(String text);
    public delegate void EventErrorHandler(Int32 error, String text);
    
    //
    public delegate void FreeSessionMethodCaller(object caller);

    //
    public delegate bool EventSession_InitHandler(Session.Session session);
    public delegate void EventSession_FreeHandler(Session.Session session);
    public delegate bool EventSession_AcceptHandler(Session.Session session);
    public delegate void EventSession_CloseHandler(bool passive);
    public delegate void EventSession_RecvHandler(byte[] buffer, int length);
    public delegate void EventSession_SendHandler(byte[] buffer, int offset, int length);

    public delegate Int32 EventParsePacketHeader(object sender, byte[] buffer, int offset);
}
