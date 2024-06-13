
namespace Cricket.Common
{
    public interface INetManager<T> : IManager
    {

        /// <summary>
        /// 刷新
        /// </summary>
        void DoUpdate();

        void DoClose();

        IMsgDispatcher<T> MsgDispatcher { get; }

        /// <summary>
        /// 发送链接请求
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        public abstract void SendConnect(string address, int port);

        /// <summary>
        /// 关闭连接
        /// </summary>
        public abstract void CloseSocket();

        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public abstract bool IsConnected();

        /// <summary>
        /// 发送SOCKET消息
        /// </summary>
        /// <param name="buffer"></param>
        public abstract void SendMessage(ByteBuffer bytebuf);

        public abstract void SendMessage(T bytebuf);

        /// <summary>
        /// 增加事件
        /// </summary>
        /// <param name="msgid"></param>
        /// <param name="bytearray"></param>
        public abstract void AddEvent(T bytearray);
    }
}
