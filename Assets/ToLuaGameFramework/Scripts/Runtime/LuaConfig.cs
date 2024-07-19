using System.Collections.Generic;
using UnityEngine;
using LuaInterface;
using System.IO;

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

        public readonly static string localABPath = Application.persistentDataPath + "/LuaRes";
        public const string MD5FileName = "files.txt";

        /// <summary>
        /// 开发专用目录（Lua脚本和预制体，声音所在目录）
        /// </summary>
        public readonly static string LuaDevPath = "LuaDev";

        public readonly static string GameResPath = $"{LuaDevPath}/GameRes";

        /// <summary>
        /// 导出AB包的路径，导出后资源从该目录上传到远程服务器，并本地清除，切勿留着导入包内，建议定义在工程目录外，如"E:/ExportAssetBundles"
        /// </summary>
        public readonly static string OutputPath = "Output/ToLuaGameFramework";

        /// <summary>
        /// 远程服务器上AB资源网址(如：http://xxx.xxx.xxx.xxx:8081/res)
        /// (若临时放在StreamingAssets目录，使用UnityWebRequest时，无需加头部file://,否则反而读取失败)
        /// </summary>
        //public readonly static string RemoteUrl = OutputPath_PC;
        public static string RemoteUrl
        {
            get
            {
                string platform = LuaTools.GetPlatform();
                string projectPath = Path.GetDirectoryName(Application.dataPath);
                return $"{projectPath}/{OutputPath}/{platform}";
            }
        }

        /// <summary>
        /// 需要导出AssetBundle的资源目录(游戏启动就必须下载)
        /// </summary>
        public static Dictionary<string, string> ExportRes_For_Startup = new Dictionary<string, string>()
        {
            { "协议", "Proto/Protobuf"},
            { "预加载", "Prefabs/Preload"},
            { "通用", "Prefabs/Common"},
            { "角色", "Prefabs/Actors"},
            { "战斗", "Prefabs/Battle"},
            { "大厅", "Prefabs/Lobby"},
            { "登录", "Prefabs/Login"}
        };

        /// <summary>
        /// 需要导出AssetBundle的资源目录(打开模块前才独立下载)
        /// </summary>
        public static Dictionary<string, string> ExportRes_For_Delay = new Dictionary<string, string>()
        {
            { "每日奖励", "Prefabs/Activities/DailyReward"}
        };

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
