/*
Copyright (c) 2015-2021 topameng(topameng@qq.com)
https://github.com/topameng/tolua

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using LuaInterface;
using System.IO;
#if UNITY_5_4_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace LuaInterface
{
    public class LuaClient : MonoBehaviour
    {
        public static LuaClient Instance
        {
            get;
            protected set;
        }

        protected LuaState luaState = null;
        protected LuaLooper loop = null;

        protected void Init()
        {
            luaState = new LuaState();

            OnInitOpenLibs(); //添加c库

            luaState.LuaSetTop(0);  //清掉可能残留的堆栈

            OnInitBinding(); //添加csharp绑定

            OnLoadFinished(); //启动loop
        }

        //添加c库
        protected virtual void OnInitOpenLibs() {
            //保持库名字与5.1.5库中一致
            luaState.BeginPreLoad();
            //luaState.AddPreLoadLib("pb2", new LuaCSFunction(LuaDLL.luaopen_pb));
            luaState.AddPreLoadLib("struct", new LuaCSFunction(LuaDLL.luaopen_struct));
            luaState.AddPreLoadLib("lpeg", new LuaCSFunction(LuaDLL.luaopen_lpeg));
            luaState.AddPreLoadLib("cjson", new LuaCSFunction(LuaDLL.luaopen_cjson));
            luaState.AddPreLoadLib("cjson.safe", new LuaCSFunction(LuaDLL.luaopen_cjson_safe));

            //新
            luaState.AddPreLoadLib("pb", new LuaCSFunction(LuaDLL.luaopen_pb));
            luaState.AddPreLoadLib("pb.io", new LuaCSFunction(LuaDLL.luaopen_pb_io));
            luaState.AddPreLoadLib("pb.conv", new LuaCSFunction(LuaDLL.luaopen_pb_conv));
            luaState.AddPreLoadLib("pb.buffer", new LuaCSFunction(LuaDLL.luaopen_pb_buffer));
            luaState.AddPreLoadLib("pb.slice", new LuaCSFunction(LuaDLL.luaopen_pb_slice));
            luaState.AddPreLoadLib("pb.unsafe", new LuaCSFunction(LuaDLL.luaopen_pb_unsafe));
            luaState.AddPreLoadLib("sproto.core", new LuaCSFunction(LuaDLL.luaopen_sproto_core));
            luaState.AddPreLoadLib("crypt", new LuaCSFunction(LuaDLL.luaopen_crypt));

            luaState.AddPreLoadLib("socket.core", new LuaCSFunction(LuaDLL.luaopen_socket_core));
            luaState.AddPreLoadLib("mime.core", new LuaCSFunction(LuaDLL.luaopen_mime_core));

            luaState.EndPreLoad();

            if (LuaConst.openLuaDebugger)
            {
                OpenZbsDebugger();
            }
        }

        //添加csharp绑定
        protected virtual void OnInitBinding() {
            LuaCoroutine.Register(luaState, this);
        }

        public void OpenZbsDebugger(string ip = "localhost")
        {
            if (!Directory.Exists(LuaConst.zbsDir))
            {
                Debugger.LogWarning("ZeroBraneStudio not install or LuaConst.zbsDir not right");
                return;
            }

            if (!string.IsNullOrEmpty(LuaConst.zbsDir))
            {
                luaState.AddSearchPackage(LuaConst.zbsDir);
            }

            luaState.LuaDoString(string.Format("DebugServerIp = '{0}'", ip), "@LuaClient.cs");
        }

        protected virtual void OnLoadFinished()
        {
            luaState.Start();
            loop = gameObject.AddComponent<LuaLooper>();
            loop.luaState = luaState;
        }


        protected void Awake()
        {
            Instance = this;
            Init();
        }

        public virtual void Destroy()
        {
            if (luaState != null)
            {
                luaState.Call("OnApplicationQuit", false);
                DetachProfiler();
                LuaState state = luaState;
                luaState = null;

                if (loop != null)
                {
                    loop.Destroy();
                    loop = null;
                }

                state.Dispose();
                Instance = null;
            }
        }

        protected void OnDestroy()
        {
            Destroy();
        }

        protected void OnApplicationQuit()
        {
            Destroy();
        }

        public static LuaState GetMainState()
        {
            return Instance.luaState;
        }

        public LuaLooper GetLooper()
        {
            return loop;
        }

        LuaTable profiler = null;

        public void AttachProfiler()
        {
            if (profiler == null)
            {
                profiler = luaState.Require<LuaTable>("UnityEngine.Profiler");
                profiler.Call("start", profiler);
            }
        }
        public void DetachProfiler()
        {
            if (profiler != null)
            {
                profiler.Call("stop", profiler);
                profiler.Dispose();
                LuaProfiler.Clear();
            }
        }
    }

}
