﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using XSNetwork.Interface;

//
namespace XSNetwork.Creator
{
    class SessionCreator<Tx> : IPoolCreator<Tx>
        where Tx : class
    {
        private object m_Token;

        public SessionCreator(object token)
        {
            m_Token = token;
        }

        public IEnumerable<Tx> Create(int count)
        {
            return new SessionEnumerable<Tx>(count, m_Token);
        }

        class SessionEnumerable<Ty> : IEnumerable<Ty>
            where Ty : class
        {
            private int m_Count;
            private object m_Token;

            private Type m_ClassType;

            public SessionEnumerable(int count, object token)
            {
                m_Count = count;
                m_Token = token;

                m_ClassType = typeof(Ty);
                if (!m_ClassType.IsClass)
                { throw new Exception("Exception : " + m_ClassType.FullName + " not 'class' type."); }
            }

            public IEnumerator<Ty> GetEnumerator()
            {
                ConstructorInfo[] constructors = m_ClassType.GetConstructors();
                if(constructors.Length == 0){ yield return null; }

                Console.WriteLine("[Debug] class : '" + m_ClassType.FullName + "' constructor : '" + constructors[0].Name + "',");

                int count = m_Count;
                for (int i = 0; i < count; i++)
                {
                    object[] param_list = { i + 1, m_Token };
                    yield return (Ty)constructors[0].Invoke(param_list);
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
