using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

//
namespace XSNetwork
{
    //
    class SocketIOControl
    {
        public const UInt32 IOCPARM_MASK = 0x7F;      /* no parameters */
        public const UInt32 IOC_VOID = 0x20000000;      /* no parameters */
        public const UInt32 IOC_OUT = 0x40000000;      /* copy out parameters */
        public const UInt32 IOC_IN = 0x80000000;      /* copy in parameters */
        public const UInt32 IOC_INOUT = (IOC_IN | IOC_OUT);

        private static UInt32 _IO(UInt32 x, UInt32 y) { return (IOC_VOID | ((x) << 8) | (y)); }
        private static UInt32 _IOR(UInt32 x, UInt32 y) { return (IOC_OUT | ((0x04 & IOCPARM_MASK) << 16) | ((x) << 8) | (y)); }
        private static UInt32 _IOW(UInt32 x, UInt32 y) { return (IOC_IN | ((0x04 & IOCPARM_MASK) << 16) | ((x) << 8) | (y)); }

        public static UInt32 FIONWRITE { get { return _IOW('f', 128); } }
        public static UInt32 FIONREAD { get { return _IOR('f', 127); } }
        public static UInt32 FIONBIO { get { return _IOW('f', 126); } }
        public static UInt32 FIOASYNC { get { return _IOW('f', 125); } }
        public static UInt32 FIOSETOWN { get { return _IOW('f', 124); } }
        public static UInt32 FIOGETOWN { get { return _IOR('f', 123); } }
        public static UInt32 FIODTYPE { get { return _IOR('f', 122); } }

        public static UInt32 NRead(Socket socket)
        {
            byte[] bytes = BitConverter.GetBytes((UInt32)0);
            socket.IOControl((int)SocketIOControl.FIONREAD, null, bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        //not support
        public static void NWrite(Socket socket, UInt32 value)
        {
            byte[] bytes = BitConverter.GetBytes((UInt32)value);
            socket.IOControl((int)SocketIOControl.FIONWRITE, bytes, null);
            return;
        }
    }
}
