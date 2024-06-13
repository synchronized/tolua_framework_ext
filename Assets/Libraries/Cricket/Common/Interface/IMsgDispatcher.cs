using System;
using System.Collections;
using System.Collections.Generic;

namespace Cricket.Common
{
    public interface IMsgDispatcher<T> : IManager
    {
        public void Register(string key, Action<T> onEvent);

        public void UnRegister(string key, Action<T> onEvent);

        public void Dispatcher(T data);

    }
}
