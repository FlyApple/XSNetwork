using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSNetwork.Pool;

namespace XSNetwork.Session
{
    public class SessionPool : AllocBasePool<Session>
    {
        public SessionPool(SessionType type, int count, Base.Object token)
           : base(count, new Creator.SessionCreator(type, token))
        { 
        }
    }
}
