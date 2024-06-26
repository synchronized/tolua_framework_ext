using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

using Cricket.Common.Net;

namespace Cricket.Common
{  
    public class NetManager : INetManager<byte[]>
    {
        #region 变量

        /// <summary>
        /// Socket
        /// </summary>
        private SocketClient m_SocketClient;

        /// <summary>
        /// 事件队列
        /// </summary>
        private static Queue<byte[]> m_EventQueue = new Queue<byte[]>();


        public IMsgDispatcher<byte[]> MsgDispatcher { get; set; }
        
        #endregion

        #region 接口函数

        public NetManager() {
            m_SocketClient = new SocketClient(this);
            m_SocketClient.OnRegister();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void DoUpdate()
        {
            UpdateEventQueue();
        }

        public void DoClose()
        {
            if (m_SocketClient == null)
                return;

            m_SocketClient.OnRemove();
        }

        /// <summary>
        /// 发送链接请求
        /// </summary>
        public void SendConnect(string address, int port)
        {
            m_SocketClient.Close();
            m_SocketClient.SendConnect(address, port);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void CloseSocket()
        {
            m_SocketClient.Close();
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (m_SocketClient == null)
                return false;

            return m_SocketClient.IsConnected();
        }

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        public void SendMessage(ByteBuffer buffer)
        {
            m_SocketClient.SendMessage(buffer);
        }

        public void SendMessage(byte[] buffer)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            byteBuffer.WriteBytes(buffer);
            SendMessage(byteBuffer);
        }

        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="bytearray"></param>
        public void AddEvent(byte[] bytearray)
        {
            lock (m_EventQueue)
            {
                m_EventQueue.Enqueue(bytearray);
            }
        }

        #endregion

        #region 函数

        /// <summary>
        /// 刷新事件队列
        /// </summary>
        private void UpdateEventQueue()
        {
            if (m_EventQueue.Count <= 0)
                return;

            while (m_EventQueue.Count > 0) 
            {
                byte[] bytearray = m_EventQueue.Dequeue();

                MsgDispatcher.Dispatcher(bytearray);
            }
        }

        #endregion
    }
}


