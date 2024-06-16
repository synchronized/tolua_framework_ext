using System.Collections;
using System.Collections.Generic;
using System;

using QFramework;
using Cricket.Common;
using Proto;

namespace Cricket.Game
{
    public class MsgDispatcherPB : IMsgDispatcher<byte[]>
    {

        private StringEventSystem m_EventSystem = new StringEventSystem();

        public void Register(string key, Action<byte[]> onEvent)
        {
            m_EventSystem.Register<byte[]>(key, onEvent);
        }

        public void UnRegister(string key, Action<byte[]> onEvent)
        {
            m_EventSystem.UnRegister<byte[]>(key, onEvent); 
        }

        public void Dispatcher(byte[] bytearray) {
            ByteBuffer buff = new ByteBuffer(bytearray);
            string msgName = buff.ReadNetworkStringUInt16();
            ushort body_size = buff.ReadNetworkUInt16();
            byte[] body_body = buff.ReadBytes(body_size);

            LogKit.I("[Network]Dispatcher msgname: {0}", msgName);
            m_EventSystem.Send(msgName, body_body);
        }

    }
}
