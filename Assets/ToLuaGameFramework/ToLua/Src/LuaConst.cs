using UnityEngine;

namespace LuaInterface
{
    public static class LuaConst
    {
        public static string injectionStateKey = "ToLua_InjectStatus"; //
        public static string injectionFilesPath = "/Injection/";
        public const string ExtName = ".zip";//用.u3d容易被服务器MIME限制

    #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        public static string zbsDir = "D:/ZeroBraneStudio/lualibs/mobdebug";        //ZeroBraneStudio目录
    #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        public static string zbsDir = "/Applications/ZeroBraneStudio.app/Contents/ZeroBraneStudio/lualibs/mobdebug";
    #else
        public static string zbsDir = luaResDir + "/mobdebug/";
    #endif

        public static bool openLuaDebugger = false;         //是否连接lua调试器
    }
}