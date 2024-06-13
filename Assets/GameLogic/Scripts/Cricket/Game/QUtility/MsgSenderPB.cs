using System.Collections;
using System.Collections.Generic;
using System;

using Google.Protobuf;
using QFramework;
using Cricket.Common;
using Proto;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Cricket.Game
{
    public class MsgSenderPB : IMsgSender<byte[], IMessage>
    {

        private uint m_SessionId = 0;

        private Dictionary<uint, Action<bool, int>> m_SessionMap = new();

        public INetManager<byte[]> NetManager { get; set; } 

        private IMsgDispatcher<byte[]> m_MsgDispatcher;

        public IMsgDispatcher<byte[]> MsgDispatcher { 
            get {
                return m_MsgDispatcher;
            } 
            set {
                m_MsgDispatcher = value;
                m_MsgDispatcher.Register("res_msgresult", OnResMsgresult);
            }
        } 

        public void SendMessage(string name, IMessage protobuf) {
            this.SendMessage(name, protobuf, null);
        }

        public void SendMessage(string name, IMessage protobuf, Action<bool, int> cb) {
            if (NetManager == null) {
                LogKit.E("[Network]NetManager is not set");
                return;
            }
            req_msgheader req = new()
            {
                MsgName = name
            };
            if (cb != null) {
                m_SessionId++;
                req.Session = (int)m_SessionId;
                m_SessionMap[m_SessionId] = cb;
            }
            byte[] header_data = req.ToByteArray();
            byte[] body_data = protobuf.ToByteArray();

            ByteBuffer buff = new();
            buff.WriteNetworkUInt16((ushort)header_data.Length);
            buff.WriteBytes(header_data);
            buff.WriteNetworkUInt16((ushort)body_data.Length);
            buff.WriteBytes(body_data);

            LogKit.I("[Network]SendMessage msgname: {0}", name);
            NetManager.SendMessage(buff.ToBytes());
        }

        private void OnResMsgresult(byte[] bytearray) {
            res_msgresult resp = res_msgresult.Parser.ParseFrom(bytearray);
            OnCallback((uint)resp.Session, resp.Result, resp.ErrorCode);
        }

        public void OnCallback(uint sessionId, bool result, int errCode) {
            m_SessionMap[sessionId]?.Invoke(result, errCode);
        }
    }
}
