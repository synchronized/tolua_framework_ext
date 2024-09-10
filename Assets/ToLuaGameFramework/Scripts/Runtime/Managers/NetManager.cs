using System;

using GameFramework.Network;

namespace ToLuaGameFramework
{
    public static class NetManager
    {

        private static NetworkManager m_NetworkMgr = new();

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public static int NetworkChannelCount
        {
            get { return m_NetworkMgr.NetworkChannelCount; }
        }

        /// <summary>
        /// 网络连接成功事件。
        /// </summary>
        public static event EventHandler<NetworkConnectedEventArgs> NetworkConnected
        {
            add { m_NetworkMgr.NetworkConnected += value; }
            remove { m_NetworkMgr.NetworkConnected -= value; }
        }

        /// <summary>
        /// 网络连接关闭事件。
        /// </summary>
        public static event EventHandler<NetworkClosedEventArgs> NetworkClosed
        {
            add { m_NetworkMgr.NetworkClosed += value; }
            remove { m_NetworkMgr.NetworkClosed -= value; }
        }

        /// <summary>
        /// 网络心跳包丢失事件。
        /// </summary>
        public static event EventHandler<NetworkMissHeartBeatEventArgs> NetworkMissHeartBeat
        {
            add { m_NetworkMgr.NetworkMissHeartBeat += value; }
            remove { m_NetworkMgr.NetworkMissHeartBeat -= value; }
        }

        /// <summary>
        /// 网络错误事件。
        /// </summary>
        public static event EventHandler<NetworkErrorEventArgs> NetworkError
        {
            add { m_NetworkMgr.NetworkError += value; }
            remove { m_NetworkMgr.NetworkError -= value; }
        }

        /// <summary>
        /// 用户自定义网络错误事件。
        /// </summary>
        public static event EventHandler<NetworkCustomErrorEventArgs> NetworkCustomError
        {
            add { m_NetworkMgr.NetworkCustomError += value; }
            remove { m_NetworkMgr.NetworkCustomError -= value; }
        }

        public static void AddNetworkConnected(INetworkChannel channel, Action<NetworkConnectedEventArgs> eventHandler) {
            m_NetworkMgr.AddNetworkConnected(channel, eventHandler);
        }
        public static void RemoveNetworkConnected(INetworkChannel channel, Action<NetworkConnectedEventArgs> eventHandler) {
            m_NetworkMgr.RemoveNetworkConnected(channel, eventHandler);
        }
        public static void AddNetworkClosed(INetworkChannel channel, Action<NetworkClosedEventArgs> eventHandler) {
            m_NetworkMgr.AddNetworkClosed(channel, eventHandler);
        }
        public static void RemoveNetworkClosed(INetworkChannel channel, Action<NetworkClosedEventArgs> eventHandler) {
            m_NetworkMgr.RemoveNetworkClosed(channel, eventHandler);
        }
        public static void AddNetworkMissHeartBeat(INetworkChannel channel, Action<NetworkMissHeartBeatEventArgs> eventHandler) {
            m_NetworkMgr.AddNetworkMissHeartBeat(channel, eventHandler);
        }
        public static void RemoveNetworkMissHeartBeat(INetworkChannel channel, Action<NetworkMissHeartBeatEventArgs> eventHandler) {
            m_NetworkMgr.RemoveNetworkMissHeartBeat(channel, eventHandler);
        }
        public static void AddNetworkError(INetworkChannel channel, Action<NetworkErrorEventArgs> eventHandler) {
            m_NetworkMgr.AddNetworkError(channel, eventHandler);
        }
        public static void RemoveNetworkError(INetworkChannel channel, Action<NetworkErrorEventArgs> eventHandler) {
            m_NetworkMgr.RemoveNetworkError(channel, eventHandler);
        }
        public static void AddNetworkCustomError(INetworkChannel channel, Action<NetworkCustomErrorEventArgs> eventHandler) {
            m_NetworkMgr.AddNetworkCustomError(channel, eventHandler);
        }
        public static void RemoveNetworkCustomError(INetworkChannel channel, Action<NetworkCustomErrorEventArgs> eventHandler) {
            m_NetworkMgr.RemoveNetworkCustomError(channel, eventHandler);
        }

        /// <summary>
        /// 网络管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal static void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_NetworkMgr.Update(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理网络管理器。
        /// </summary>
        public static void Shutdown()
        {
            m_NetworkMgr.Shutdown();
        }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        public static bool HasNetworkChannel(string name)
        {
            return m_NetworkMgr.HasNetworkChannel(name);
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        public static INetworkChannel GetNetworkChannel(string name)
        {
            return m_NetworkMgr.GetNetworkChannel(name);
        }

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <param name="serviceType">网络服务类型。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        /// <returns>要创建的网络频道。</returns>
        public static INetworkChannel CreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper)
        {
            return m_NetworkMgr.CreateNetworkChannel(name, serviceType, networkChannelHelper);
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        public static bool DestroyNetworkChannel(string name)
        {
            return m_NetworkMgr.DestroyNetworkChannel(name);
        }
    }
}


