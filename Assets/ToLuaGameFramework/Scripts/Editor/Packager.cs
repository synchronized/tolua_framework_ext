using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LuaInterface;

using EditorTools = LuaInterface.Editor.EditorTools;

namespace ToLuaGameFramework
{
    public class Packager
    {
        public static string platform = string.Empty;
        static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();

        ///iOS///////////////////////////////////////////////////////////////////////////////////////////

        [MenuItem("ToLuaGameFramework/Build IOS AssetBundle", false, 100)]
        public static void BuildiPhoneResource()
        {
            BuildAssetResource(BuildTarget.iOS);
        }

        ///Andoird///////////////////////////////////////////////////////////////////////////////////////////

        [MenuItem("ToLuaGameFramework/Build Android AssetBundle", false, 100)]
        public static void BuildAndroidResourceAll()
        {
            BuildAssetResource(BuildTarget.Android);
        }

        ///Windows///////////////////////////////////////////////////////////////////////////////////////////

        [MenuItem("ToLuaGameFramework/Build Windows AssetBundle", false, 100)]
        public static void BuildWindowsResource()
        {
            BuildAssetResource(BuildTarget.StandaloneWindows);
        }

        /// <summary>
        /// 生成绑定素材(mode:0全部导出 1导出lua脚本 2导出资源)
        /// </summary>
        public static void BuildAssetResource(BuildTarget target)
        {
            var platform = EditorTools.GetTargetPlatform(target);
            string outputPath = EditorTools.AssetPathToAbsolutePath($"{LuaConfig.OutputPath}/{platform}");

            if (outputPath.EndsWith("/lua"))
            {
                EditorUtility.DisplayDialog("提示", "导出目录(" + outputPath + ")不能用lua命名", "确定");
                return;
            }
            EditorTools.ClearFolder(outputPath);
            EditorTools.CreateDirectory(outputPath);
            AssetDatabase.Refresh();

            maps.Clear();
            HandleLuaBundle();
            HandleResBundle(LuaConfig.ExportRes_For_Startup, LuaConfig.ExportRes_For_Delay);
            HandleResBundle(LuaConfig.ExportRes_For_Delay);
            AssetDatabase.Refresh();

            BuildPipeline.BuildAssetBundles(outputPath, maps.ToArray(), BuildAssetBundleOptions.None, target);
            ClearUnuseFiles(outputPath);

            BuildFileIndex(outputPath);
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log("AssetBundle已导出到" + outputPath);
            Application.OpenURL(outputPath);
        }

        /// <summary>
        /// 处理Lua的AssetBundle
        /// </summary>
        static void HandleLuaBundle()
        {
            LuaInterface.Editor.ToLuaMenu.CopyLuaFilesToLuaSource();

            var luaDestDir = ToLuaPathConfig.GenLuaPath;
            string[] filePaths = Directory.GetFiles(luaDestDir, "*.bytes", SearchOption.AllDirectories)
                    .Select(p => EditorTools.AbsolutePathToAssetPath(p))
                    .ToArray();

            AssetBundleBuild build = new AssetBundleBuild
            {
                assetBundleName = "lua" + LuaConst.ExtName,
                assetNames = filePaths
            };
            maps.Add(build);
        }

        /// <summary>
        /// 处理Res的AssetBundle
        /// </summary>
        static void HandleResBundle(Dictionary<string, string> resDic, Dictionary<string, string> excludeList = null)
        {
            var assetsDevPath = EditorTools.GetRegularPath($"Assets/{LuaConfig.GameResPath}/");
            foreach (var path in resDic.Values)
            {
                string resPath = EditorTools.GetRegularPath(Path.Combine(assetsDevPath , path));

                var files = Directory.GetFiles(resPath, "*", SearchOption.AllDirectories)
                        .Select(f => EditorTools.GetRegularPath(f))
                        .Where(f => !f.EndsWith(".meta"))
                        .ToList();
                if (excludeList != null)
                {
                    files = files.Where(f => {
                        var isExclude = excludeList.Where(item => { return f.Replace(assetsDevPath, "").StartsWith(item.Value); }).Count() > 0;
                        return !isExclude;
                    }).ToList();
                }

                string abName = EditorTools.Substring(resPath, assetsDevPath, false).Replace("/", "_");
                AssetBundleBuild build = new()
                {
                    assetBundleName = abName + LuaConst.ExtName,
                    assetNames = files.ToArray()
                };
                maps.Add(build);
            }
        }

        /// <summary>
        /// 清除.manifest,.meta,.DS_Store等无用文件
        /// </summary>
        static void ClearUnuseFiles(string outputPath)
        {
            List<string> paths = new List<string>();
            paths.AddRange(Directory.GetFiles(outputPath, "*.manifest", SearchOption.AllDirectories));
            paths.AddRange(Directory.GetFiles(outputPath, "*.meta", SearchOption.AllDirectories));
            paths.AddRange(Directory.GetFiles(outputPath, "*.DS_Store", SearchOption.AllDirectories));
            for (int i = 0; i < paths.Count; i++)
            {
                File.Delete(paths[i]);
            }
            string rootFileName = outputPath.Substring(outputPath.LastIndexOf("/"));
            string rootFilePath = outputPath + rootFileName;
            if (File.Exists(rootFilePath))
            {
                File.Delete(rootFilePath);
            }
            if (File.Exists(rootFilePath + ".manifest"))
            {
                File.Delete(rootFilePath + ".manifest");
            }
        }

        /// <summary>
        /// 创建资源MD5列表，以便检查更新
        /// </summary>
        static void BuildFileIndex(string outputPath)
        {
            string resPath = outputPath;
            string newFilePath = resPath + "/" + LuaConfig.MD5FileName;
            if (File.Exists(newFilePath)) File.Delete(newFilePath);

            FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
            StreamWriter sw = new StreamWriter(fs);
            //lua.zip文件
            string fileName = "lua" + LuaConst.ExtName;
            string filePath = outputPath + "/" + fileName;
            if (File.Exists(filePath))
            {
                string md5 = LUtils.MD5file(filePath);
                sw.WriteLine("0|动态|" + fileName + "|" + md5);
            }
            //自动下载的资源
            foreach (var path in LuaConfig.ExportRes_For_Startup)
            {
                fileName = path.Value + LuaConst.ExtName;
                fileName = fileName.Replace("\\", "_").Replace("/", "_").ToLower();
                filePath = outputPath + "/" + fileName;
                if (File.Exists(filePath))
                {
                    string md5 = LUtils.MD5file(filePath);
                    sw.WriteLine("0|" + path.Key + "|" + fileName + "|" + md5);
                }
            }
            //延迟下载的资源
            foreach (var path in LuaConfig.ExportRes_For_Delay)
            {
                fileName = path.Value + LuaConst.ExtName;
                fileName = fileName.Replace("\\", "_").Replace("/", "_").ToLower();
                filePath = outputPath + "/" + fileName;
                if (File.Exists(filePath))
                {
                    string md5 = LUtils.MD5file(filePath);
                    sw.WriteLine("1|" + path.Key + "|" + fileName + "|" + md5);
                }
            }
            sw.Close();
            fs.Close();
        }

    }
}
