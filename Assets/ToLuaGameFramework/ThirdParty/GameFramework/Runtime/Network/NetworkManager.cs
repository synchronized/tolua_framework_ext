//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace GameFramework.Network
{
    /// <summary>
    /// 网络管理器。
    /// </summary>
    public partial class NetworkManager : INetworkManager
    {
        private readonly Dictionary<string, NetworkChannelBase> m_NetworkChannels;

        private EventHandler<NetworkConnectedEventArgs> m_NetworkConnectedEventHandler;
        private EventHandler<NetworkClosedEventArgs> m_NetworkClosedEventHandler;
        private EventHandler<NetworkMissHeartBeatEventArgs> m_NetworkMissHeartBeatEventHandler;
        private EventHandler<NetworkErrorEventArgs> m_NetworkErrorEventHandler;
        private EventHandler<NetworkCustomErrorEventArgs> m_NetworkCustomErrorEventHandler;

        /// <summary>
        /// 初始化网络管理器的新实例。
        /// </summary>
        public NetworkManager()
        {
            m_NetworkChannels = new Dictionary<string, NetworkChannelBase>(StringComparer.Ordinal);
            m_NetworkConnectedEventHandler = null;
            m_NetworkClosedEventHandler = null;
            m_NetworkMissHeartBeatEventHandler = null;
            m_NetworkErrorEventHandler = null;
            m_NetworkCustomErrorEventHandler = null;
        }

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public int NetworkChannelCount
        {
            get { return m_NetworkChannels.Count; }
        }

        /// <summary>
        /// 网络连接成功事件。
        /// </summary>
        public event EventHandler<NetworkConnectedEventArgs> NetworkConnected
        {
            add { m_NetworkConnectedEventHandler += value; }
            remove { m_NetworkConnectedEventHandler -= value; }
        }

        /// <summary>
        /// 网络连接关闭事件。
        /// </summary>
        public event EventHandler<NetworkClosedEventArgs> NetworkClosed
        {
            add { m_NetworkClosedEventHandler += value; }
            remove { m_NetworkClosedEventHandler -= value; }
        }

        /// <summary>
        /// 网络心跳包丢失事件。
        /// </summary>
        public event EventHandler<NetworkMissHeartBeatEventArgs> NetworkMissHeartBeat
        {
            add { m_NetworkMissHeartBeatEventHandler += value; }
            remove { m_NetworkMissHeartBeatEventHandler -= value; }
        }

        /// <summary>
        /// 网络错误事件。
        /// </summary>
        public event EventHandler<NetworkErrorEventArgs> NetworkError
        {
            add { m_NetworkErrorEventHandler += value; }
            remove { m_NetworkErrorEventHandler -= value; }
        }

        /// <summary>
        /// 用户自定义网络错误事件。
        /// </summary>
        public event EventHandler<NetworkCustomErrorEventArgs> NetworkCustomError
        {
            add { m_NetworkCustomErrorEventHandler += value; }
            remove { m_NetworkCustomErrorEventHandler -= value; }
        }

        public void AddNetworkConnected(INetworkChannel channel, Action<NetworkConnectedEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelConnected += eventHandler;
        }
        public void RemoveNetworkConnected(INetworkChannel channel, Action<NetworkConnectedEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelConnected -= eventHandler;
        }
        public void AddNetworkClosed(INetworkChannel channel, Action<NetworkClosedEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelClosed += eventHandler;
        }
        public void RemoveNetworkClosed(INetworkChannel channel, Action<NetworkClosedEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelClosed -= eventHandler;
        }
        public void AddNetworkMissHeartBeat(INetworkChannel channel, Action<NetworkMissHeartBeatEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelMissHeartBeat += eventHandler;
        }
        public void RemoveNetworkMissHeartBeat(INetworkChannel channel, Action<NetworkMissHeartBeatEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelMissHeartBeat -= eventHandler;
        }
        public void AddNetworkError(INetworkChannel channel, Action<NetworkErrorEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelError += eventHandler;
        }
        public void RemoveNetworkError(INetworkChannel channel, Action<NetworkErrorEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelError -= eventHandler;
        }
        public void AddNetworkCustomError(INetworkChannel channel, Action<NetworkCustomErrorEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelCustomError += eventHandler;
        }
        public void RemoveNetworkCustomError(INetworkChannel channel, Action<NetworkCustomErrorEventArgs> eventHandler) {
            var networkChannel = channel as NetworkChannelBase;
            networkChannel.NetworkChannelCustomError -= eventHandler;
        }


        /// <summary>
        /// 网络管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in m_NetworkChannels)
            {
                networkChannel.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理网络管理器。
        /// </summary>
        public void Shutdown()
        {
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in m_NetworkChannels)
            {
                NetworkChannelBase networkChannelBase = networkChannel.Value;
                networkChannelBase.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannelBase.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannelBase.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannelBase.NetworkChannelError -= OnNetworkChannelError;
                networkChannelBase.NetworkChannelCustomError -= OnNetworkChannelCustomError;
                networkChannelBase.Shutdown();
            }

            m_NetworkChannels.Clear();
        }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        public bool HasNetworkChannel(string name)
        {
            return m_NetworkChannels.ContainsKey(name ?? string.Empty);
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        public INetworkChannel GetNetworkChannel(string name)
        {
            NetworkChannelBase networkChannel = null;
            if (m_NetworkChannels.TryGetValue(name ?? string.Empty, out networkChannel))
            {
                return networkChannel;
            }

            return null;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <returns>所有网络频道。</returns>
        public INetworkChannel[] GetAllNetworkChannels()
        {
            int index = 0;
            INetworkChannel[] results = new INetworkChannel[m_NetworkChannels.Count];
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in m_NetworkChannels)
            {
                results[index++] = networkChannel.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <param name="results">所有网络频道。</param>
        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, NetworkChannelBase> networkChannel in m_NetworkChannels)
            {
                results.Add(networkChannel.Value);
            }
        }

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <param name="serviceType">网络服务类型。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        /// <returns>要创建的网络频道。</returns>
        protected virtual NetworkChannelBase DoCreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper)
        {
            if (networkChannelHelper == null)
            {
                throw new GameFrameworkException("Network channel helper is invalid.");
            }

            if (networkChannelHelper.PacketHeaderLength < 0)
            {
                throw new GameFrameworkException("Packet header length is invalid.");
            }

            NetworkChannelBase networkChannel = null;
            switch (serviceType)
            {
                case ServiceType.Tcp:
                    networkChannel = new TcpNetworkChannel(name, networkChannelHelper);
                    break;

                case ServiceType.TcpWithSyncReceive:
                    networkChannel = new TcpWithSyncReceiveNetworkChannel(name, networkChannelHelper);
                    break;

                default:
                    throw new GameFrameworkException(string.Format("Not supported service type '{0}'.", serviceType));
            }

            networkChannel.NetworkChannelConnected += OnNetworkChannelConnected;
            networkChannel.NetworkChannelClosed += OnNetworkChannelClosed;
            networkChannel.NetworkChannelMissHeartBeat += OnNetworkChannelMissHeartBeat;
            networkChannel.NetworkChannelError += OnNetworkChannelError;
            networkChannel.NetworkChannelCustomError += OnNetworkChannelCustomError;
            return networkChannel;
        }

        public INetworkChannel CreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper)
        {
            if (HasNetworkChannel(name))
            {
                throw new GameFrameworkException(string.Format("Already exist network channel '{0}'.", name ?? string.Empty));
            }
            var networkChannel = DoCreateNetworkChannel(name, serviceType, networkChannelHelper);
            m_NetworkChannels.Add(name, networkChannel);
            return networkChannel;
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        protected virtual void DoDestroyNetworkChannel(NetworkChannelBase networkChannel)
        {
            networkChannel.NetworkChannelConnected -= OnNetworkChannelConnected;
            networkChannel.NetworkChannelClosed -= OnNetworkChannelClosed;
            networkChannel.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
            networkChannel.NetworkChannelError -= OnNetworkChannelError;
            networkChannel.NetworkChannelCustomError -= OnNetworkChannelCustomError;
            networkChannel.Shutdown();
        }

        public bool DestroyNetworkChannel(string name)
        {
            NetworkChannelBase networkChannel = null;
            if (m_NetworkChannels.TryGetValue(name ?? string.Empty, out networkChannel))
            {
                DoDestroyNetworkChannel(networkChannel);
                return m_NetworkChannels.Remove(name);
            }

            return false;
        }

        private void OnNetworkChannelConnected(NetworkConnectedEventArgs networkConnectedEventArgs)
        {
            m_NetworkConnectedEventHandler?.Invoke(this, networkConnectedEventArgs);
        }

        private void OnNetworkChannelClosed(NetworkClosedEventArgs networkClosedEventArgs)
        {
            m_NetworkClosedEventHandler?.Invoke(this, networkClosedEventArgs);
        }

        private void OnNetworkChannelMissHeartBeat(NetworkMissHeartBeatEventArgs networkMissHeartBeatEventArgs)
        {
            m_NetworkMissHeartBeatEventHandler?.Invoke(this, networkMissHeartBeatEventArgs);
        }

        private void OnNetworkChannelError(NetworkErrorEventArgs networkErrorEventArgs)
        {
            m_NetworkErrorEventHandler?.Invoke(this, networkErrorEventArgs);
        }

        private void OnNetworkChannelCustomError(NetworkCustomErrorEventArgs networkCustomErrorEventArgs)
        {
            m_NetworkCustomErrorEventHandler?.Invoke(this, networkCustomErrorEventArgs);
        }
    }
}
