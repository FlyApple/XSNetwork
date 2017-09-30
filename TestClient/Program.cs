using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace R2AuthServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
            try
            {
                socket.Connect(ipep);

                byte[] buffer = new byte[0x100];
                buffer[0] = 0x00;
                buffer[1] = 0x01;
                buffer[2] = 0x01;

                socket.Send(buffer);
                
                //Thread.Sleep(5000);

                buffer[2] = 0x02;
                socket.Send(buffer);
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return;
        }
    }
}
