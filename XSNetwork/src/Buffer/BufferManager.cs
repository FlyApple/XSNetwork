using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

//
namespace XSNetwork.Buffer
{
    public class BufferManager
    {
        private static int m_ElementLength = 2048;
        public static int ElementLength { get { return m_ElementLength; } }

        public static Base.EventParsePacketHeader Event_ParsePacketHeader;

        private int m_BufferLength;
        private int m_BufferOffset;
        private byte[] m_BufferData;

        public BufferManager(int LengthMax)
        {
            m_BufferLength = 0;
            m_BufferOffset = 0;
            m_BufferData = new byte[LengthMax];
        }

        public void dispose()
        {
            m_BufferData = null;
            m_BufferLength = 0;
            m_BufferOffset = 0;
        }

        public int Length { get { return m_BufferLength; } }
        public int LengthMax { get { return m_BufferData.Length; } }
        public byte[] Buffer { get { return m_BufferData; } }

        public void ClearData()
        {
            m_BufferLength = 0;
            m_BufferOffset = 0;
        }

        public int PushData(byte[] buffer, int length)
        {
            return this.PushData(buffer, 0, length);
        }

        public int PushData(byte[] buffer, int offset, int length)
        {
            if (length > ElementLength) { return 0; }
            if (m_BufferOffset + m_BufferLength + length > m_BufferData.Length &&
                m_BufferOffset < length)
            { return 0; }

            if (m_BufferOffset + m_BufferLength + length <= m_BufferData.Length)
            { Array.Copy(buffer, offset, m_BufferData, m_BufferOffset + m_BufferLength, length); }
            else if (m_BufferOffset >= length)
            {
                Array.Copy(m_BufferData, m_BufferOffset, m_BufferData, 0, m_BufferLength);
        
                m_BufferOffset = 0;

                Array.Copy(buffer, offset, m_BufferData, m_BufferOffset + m_BufferLength, length);
            }
            else
            {
                return 0;
            }

            m_BufferLength += length;
            return length;
        }

        public bool PopData(int length)
        {
            if (m_BufferOffset + length > m_BufferData.Length ||
                length > m_BufferLength)
            { return false; }

            m_BufferOffset += length;
            m_BufferLength -= length;
            return true;
        }

        public int GetData(int src_index, byte[] dst_data, int dst_index)
        {
            if (dst_data == null) { return -1; }
            
            if (m_BufferOffset + src_index + 4 > m_BufferLength) { return 0; }

            int packet_header1 = BitConverter.ToUInt16(m_BufferData, m_BufferOffset + src_index);
            int packet_header2 = BitConverter.ToUInt16(m_BufferData, m_BufferOffset + src_index + 2);

            int length = packet_header1;
            if (Event_ParsePacketHeader != null)
            {
                length = Event_ParsePacketHeader(this, m_BufferData, m_BufferOffset + src_index);
            }

            if (length > 2048 || length < 4) { return -1; }
            if (length > m_BufferLength) { return 0; }

            Array.Copy(m_BufferData, m_BufferOffset + src_index, dst_data, dst_index, length);
            return length;
        }


    }
}
