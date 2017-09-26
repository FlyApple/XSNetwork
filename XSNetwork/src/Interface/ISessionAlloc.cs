using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSNetwork.Interface
{
    public interface ISessionAlloc
    {
        Session.Session AllocSession();
        void FreeSession(Session.Session session);
    }
}
