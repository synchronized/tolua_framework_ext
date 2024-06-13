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
            ushort header_size = buff.ReadNetworkUInt16();
            byte[] header_body = buff.ReadBytes(header_size);
            ushort body_size = buff.ReadNetworkUInt16();
            byte[] body_body = buff.ReadBytes(body_size);
            res_msgheader header = res_msgheader.Parser.ParseFrom(header_body);

            LogKit.I("[Network]Dispatcher msgname: {0}", header.MsgName);
            m_EventSystem.Send(header.MsgName, body_body);
        }

    }
}
