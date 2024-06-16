using System.Collections;
using System.Collections.Generic;
using System;

using QFramework;
using Cricket.Common;
using Proto;

namespace Cricket.Game
{
    public class MsgSenderPB : IMsgSender<byte[], byte[]>
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

        public void SendMessage(string name, byte[] body_data) {
            this.SendMessage(name, body_data, null);
        }

        public void SendMessage(string name, byte[] body_data, Action<bool, int> cb) {
            if (NetManager == null) {
                LogKit.E("[Network]NetManager is not set");
                return;
            }
            uint client_session_id = 0;
            if (cb != null) {
                m_SessionId++;
                client_session_id = m_SessionId;
                m_SessionMap[client_session_id] = cb;
            }

            ByteBuffer buff = new();
            buff.WriteNetworkStringUInt16(name);
            buff.WriteNetworkUInt32(client_session_id);
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
