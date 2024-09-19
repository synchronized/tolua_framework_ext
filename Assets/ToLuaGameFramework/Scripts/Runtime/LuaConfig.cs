using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

namespace ToLuaGameFramework
{
    /// <summary>
    /// 项目配置
    /// </summary>
    public static class LuaConfig
    {
        public readonly static string frameworkRoot = Application.dataPath + "/ToLuaGameFramework";

        [ToLuaRootPath]
        public readonly static string toluaRootPath = $"{frameworkRoot}/ToLua";

        /// <summary>
        /// 开发专用目录（Lua脚本和预制体，声音所在目录）
        /// </summary>
        public readonly static string LuaDevPath = "LuaDev";

        public readonly static string GameResPath = $"Assets/{LuaDevPath}/GameRes";

        [ToLuaAddLuaPath]
        public static IEnumerable<string> AddLuaPath {
            get {
                return new List<string>() {
                    $"{LuaConfig.frameworkRoot}/Lua",
                    $"{Application.dataPath}/{LuaConfig.LuaDevPath}/Lua",
                };
            }
        }
    }
}
