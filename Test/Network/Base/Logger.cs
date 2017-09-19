using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSNetwork.Base
{
    public class Logger
    {
        public EventLogoutHandler Event_LogoutHandler;
        public EventErrorHandler Event_ErrorHandler;

        public Logger()
        {
            Event_LogoutHandler = null;
            Event_ErrorHandler = null;
        }

        public void Logout(String text)
        {
            if (Event_LogoutHandler != null)
            { Event_LogoutHandler(text); }
        }

        public void Error(Int32 error, String text)
        {
            if (Event_ErrorHandler != null)
            { Event_ErrorHandler(error, text);  }
        }
    }
}
