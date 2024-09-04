using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using GameFramework.Network;
using LuaInterface;
using ToLuaGameFramework;

namespace GameClient.Network
{

    public sealed class GameServerPacketHeader : IPacketHeader {

        public byte[] data = new byte[2];
        public int packageLength = 0;

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        public int PacketLength
        {
            get { return packageLength; }
        }
    }

    public sealed class GameServerPacketReq : Packet {

        public string name;
        public byte[] data;

        public override int Id { get { return 0; } }

        public GameServerPacketReq(string _name, byte[] _data) {
            name = _name;
            data = _data;
        }

        /// <summary>
        /// 清理引用。
        /// </summary>
        public override void Clear() {}
    }

    public sealed class GameServerPacket : Packet {

        public string name;
        public byte[] data;

        public override int Id { get { return 0; } }

        public GameServerPacket(int Length) {
            data = new byte[Length];
        }

        public GameServerPacket(string _name, byte[] _data) {
            name = _name;
            data = _data;
        }

        /// <summary>
        /// 清理引用。
        /// </summary>
        public override void Clear() {}
    }

    public sealed class GameServerNetworkChannelHelper : INetworkChannelHelper
    {

        private INetworkChannel networkChannel;

        private EventHandler<Packet> m_MsgHandler;
        private LuaFunction luaOnReceiveServerData;
        private LuaFunction luaOnNetworkConnected;
        private LuaFunction luaOnNetworkClose;
        private LuaFunction luaOnNetworkError;

        /// <summary>
        /// 获取消息包头长度。
        /// </summary>
        public int PacketHeaderLength { get { return 2; } }

        public GameServerNetworkChannelHelper() {
            m_MsgHandler = OnReceiveMsg;
            luaOnReceiveServerData = LuaManager.Instance.GetFunction("LNetMgr.OnReceveServerData");
            luaOnNetworkConnected = LuaManager.Instance.GetFunction("LNetMgr.OnNetworkConnected");
            luaOnNetworkClose = LuaManager.Instance.GetFunction("LNetMgr.OnNetworkClose");
            luaOnNetworkError = LuaManager.Instance.GetFunction("LNetMgr.OnNetworkError");
        }

        /// <summary>
        /// 初始化网络频道辅助器。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        public void Initialize(INetworkChannel channel) {
            networkChannel = channel;
            channel.SetDefaultHandler(m_MsgHandler);
            NetManager.AddNetworkConnected(channel, OnNetworkChannelConnected);
            NetManager.AddNetworkClosed(channel, OnNetworkChannelClosed);
            NetManager.AddNetworkError(channel, OnNetworkChannelError);
        }

        /// <summary>
        /// 关闭并清理网络频道辅助器。
        /// </summary>
        public void Shutdown() {
            var channel = networkChannel;
            channel.SetDefaultHandler(null);
            NetManager.RemoveNetworkConnected(channel, OnNetworkChannelConnected);
            NetManager.RemoveNetworkClosed(channel, OnNetworkChannelClosed);
            NetManager.RemoveNetworkError(channel, OnNetworkChannelError);
            networkChannel = null;
            m_MsgHandler = null;
        }

        /// <summary>
        /// 准备进行连接。
        /// </summary>
        public void PrepareForConnecting() {}

        /// <summary>
        /// 发送心跳消息包。
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        public bool SendHeartBeat() { return true; }

        /// <summary>
        /// 序列化消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="packet">要序列化的消息包。</param>
        /// <param name="destination">要序列化的目标流。</param>
        /// <returns>是否序列化成功。</returns>
        public bool Serialize(Packet packet, Stream destination) {
            var pkt = packet as GameServerPacketReq;
            if (pkt == null) return false;
            var stream = new MemoryStream(pkt.data.Length+2);
            using var writer = new BinaryWriter(stream);
            writer.Write(Converter.HostToNetworkOrder((ushort)pkt.data.Length));
            writer.Write(pkt.data);
            var data = stream.ToArray();
            destination.Write(data, 0, data.Length);
            return true;
        }

        /// <summary>
        /// 反序列化消息包头。
        /// </summary>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包头。</returns>
        public IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData) {
            customErrorData = null;
            GameServerPacketHeader header = new();
            using var reader = new BinaryReader(source, Encoding.Default, true);
            header.packageLength = Converter.NetworkToHostOrder(reader.ReadUInt16());
            return header;
        }

        /// <summary>
        /// 反序列化消息包。
        /// </summary>
        /// <param name="packetHeader">消息包头。</param>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包。</returns>
        public Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData) {
            customErrorData = null;
            using var reader = new BinaryReader(source, Encoding.Default, true);
            ushort lenOfName = Converter.NetworkToHostOrder(reader.ReadUInt16());
            string name = Encoding.UTF8.GetString(reader.ReadBytes(lenOfName));
            ushort lenOfData = Converter.NetworkToHostOrder(reader.ReadUInt16());
            byte[] data = reader.ReadBytes(lenOfData);
            return new GameServerPacket(name, data);
        }

        private void OnReceiveMsg(object sender, Packet packaet) {
            var pkg = packaet as GameServerPacket;
            luaOnReceiveServerData?.Call(pkg.name, new LuaByteBuffer(pkg.data));
        }

        private void OnNetworkChannelConnected(NetworkConnectedEventArgs args)
        {
            luaOnNetworkConnected?.Call(args.UserData);
        }

        private void OnNetworkChannelClosed(NetworkClosedEventArgs args)
        {
            luaOnNetworkClose?.Call();
        }

        private void OnNetworkChannelError(NetworkErrorEventArgs args)
        {
            luaOnNetworkError?.Call(args.ErrorCode, args.SocketErrorCode, args.ErrorMessage);
        }
    }
}
