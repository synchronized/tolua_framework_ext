using System.IO;
using UnityEngine;
using LuaInterface;
using System;

namespace ToLuaGameFramework
{
    public class LuaManager
    {
        private static LuaManager m_Instance;

        public static LuaManager Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = new LuaManager();
                }
                return m_Instance;
            }
        }

        private MonoBehaviour behaviour;

        private LuaState lua;
        private LuaLooper loop = null;

        internal Action OnStartComplete;

        private LuaManager()
        {
            lua = new LuaState();

            InitLuaEnv();

            lua.LuaSetTop(0);
        }

        internal void Initalize(MonoBehaviour behaviour){
            this.behaviour = behaviour;
            LuaCoroutine.Register(lua, behaviour);
        }

        //被Lua调用
        public static void StartComplete() {
            Instance.OnStartComplete?.Invoke();
            Instance.OnStartComplete = null;
        }

        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        void InitLuaEnv()
        {
            lua.BeginPreLoad();
            lua.AddPreLoadLib("struct", new LuaCSFunction(LuaDLL.luaopen_struct));
            lua.AddPreLoadLib("lpeg", new LuaCSFunction(LuaDLL.luaopen_lpeg));
            lua.AddPreLoadLib("cjson", new LuaCSFunction(LuaDLL.luaopen_cjson));
            lua.AddPreLoadLib("cjson.safe", new LuaCSFunction(LuaDLL.luaopen_cjson_safe));

            lua.AddPreLoadLib("pb", new LuaCSFunction(LuaDLL.luaopen_pb));
            lua.AddPreLoadLib("pb.io", new LuaCSFunction(LuaDLL.luaopen_pb_io));
            lua.AddPreLoadLib("pb.conv", new LuaCSFunction(LuaDLL.luaopen_pb_conv));
            lua.AddPreLoadLib("pb.buffer", new LuaCSFunction(LuaDLL.luaopen_pb_buffer));
            lua.AddPreLoadLib("pb.slice", new LuaCSFunction(LuaDLL.luaopen_pb_slice));
            lua.AddPreLoadLib("pb.unsafe", new LuaCSFunction(LuaDLL.luaopen_pb_unsafe));
            lua.AddPreLoadLib("sproto.core", new LuaCSFunction(LuaDLL.luaopen_sproto_core));
            lua.AddPreLoadLib("crypt", new LuaCSFunction(LuaDLL.luaopen_crypt));

            lua.AddPreLoadLib("socket.core", new LuaCSFunction(LuaDLL.luaopen_socket_core));
            lua.AddPreLoadLib("mime.core", new LuaCSFunction(LuaDLL.luaopen_mime_core));

            lua.EndPreLoad();
        }

        /// <summary>
        /// 启动Lua框架
        /// </summary>
        public void StartLua()
        {
            lua.Start();

            lua.DoFile("Main.lua");

            LuaFunction main = lua.GetFunction("Main");
            main.Call();
            main.Dispose();
            main = null;

            loop = behaviour.gameObject.AddComponent<LuaLooper>();
            loop.luaState = lua;
        }

        public void DoFile(string filename)
        {
            lua.DoFile(filename);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName)
        {
            GetFunction(funcName).Call();
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param)
        {
            GetFunction(funcName).Call(param);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2)
        {
            GetFunction(funcName).Call(param1, param2);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2, object param3)
        {
            GetFunction(funcName).Call(param1, param2, param3);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2, object param3, object param4)
        {
            GetFunction(funcName).Call(param1, param2, param3, param4);
        }

        /// <summary>
        /// 执行Lua全局方法
        /// </summary>
        public void CallFunction(string funcName, object param1, object param2, object param3, object param4, object param5)
        {
            GetFunction(funcName).Call(param1, param2, param3, param4, param5);
        }

        /// <summary>
        /// 获取Lua全局方法
        /// </summary>
        public LuaFunction GetFunction(string funcName)
        {
            return lua.GetFunction(funcName);
        }

        public void LuaGC()
        {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public void Close()
        {
            loop.Destroy();
            loop = null;

            lua.Dispose();
            lua = null;
        }
    }
}
