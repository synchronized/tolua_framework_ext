using System.Collections;
using System.Collections.Generic;

using QFramework;
using Google.Protobuf;

using Cricket.Common;

namespace Cricket.Game
{
    public class GameArchWrap 
    {

        public static IArchitecture GetArchitecture()
        {
            return GameArch.Interface;
        }

        public static IMsgDispatcher<byte[]> GetMsgDispatcher() {
            return GetArchitecture().GetUtility<IMsgDispatcher<byte[]>>();
        }

        public static INetManager<byte[]> GetNetManager() {
            return GetArchitecture().GetUtility<INetManager<byte[]>>();
        }

        public static IMsgSender<byte[], IMessage> GetMsgSender() {
            return GetArchitecture().GetUtility<IMsgSender<byte[], IMessage>>();
        }


    }
}

