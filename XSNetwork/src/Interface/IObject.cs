using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace XSNetwork.Interface
{
    interface IObject
    {
        String LocalAddress { get; }
        Int32 LocalPort { get; }
        IPEndPoint LocalIPEndPoint { get; }

        String RemoteAddress { get; }
        Int32 RemotePort { get; }
        IPEndPoint RemoteIPEndPoint { get; }
    }
}
