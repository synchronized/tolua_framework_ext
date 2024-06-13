using System;
using System.Collections;
using System.Collections.Generic;

namespace Cricket.Common
{
    public interface IMsgSender<T, TReqMsg> : IManager
    {
        void SendMessage(string name, TReqMsg data, Action<bool, int> cb);

        void SendMessage(string name, TReqMsg data);

        void OnCallback(uint sessionId, bool result, int errCode);
    }
}
