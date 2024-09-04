using System;
using System.Collections.Generic;
using LuaInterface;
using LuaInterface.Editor;

namespace GameClient.Config
{

    public static class ToLuaGenConfig {

        [ToLuaLuaCallCSharp]
        //在这里添加你要导出注册到lua的类型列表
        public static BindType[] customTypeList =
        {
            _GT(typeof(System.Net.Sockets.SocketError)),
            _GT(typeof(ToLuaGameFramework.NetManager)),
            //_GT(typeof(ToLuaGameFramework.MsgDispatcher)),
            //_GT(typeof(ToLuaGameFramework.MsgSender)),

            _GT(typeof(GameFramework.Network.NetworkManager.NetworkChannelBase)),
            _GT(typeof(GameFramework.Network.NetworkManager.TcpNetworkChannel)),
            _GT(typeof(GameFramework.Network.ServiceType)),

            _GT(typeof(GameClient.Network.NetManager)),
            _GT(typeof(GameClient.Network.GameServerPacket)),
            _GT(typeof(GameClient.Network.GameServerPacketReq)),
            _GT(typeof(GameClient.Network.GameServerNetworkChannelHelper)),
        };

        [ToLuaCSharpCallLua]
        public static List<Type> customDelegateList = new List<Type>()
        {
        };

        //黑名单
        [ToLuaBlackList]
        public static List<List<string>> BlackList = new List<List<string>>
        {
        };

        public static BindType _GT(Type t)
        {
            return new BindType(t);
        }
    }
}
