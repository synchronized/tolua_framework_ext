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
//打开开关没有写入导出列表的纯虚类自动跳过
//#define JUMP_NODEFINED_ABSTRACT

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEditor;

using Debug = UnityEngine.Debug;

namespace LuaInterface.Editor
{

    [InitializeOnLoad]
    public static class ToLuaMenu
    {

        //private static bool beAutoGen = false;
        const string tip = "为防止误操作，已禁用该命令！";

        static ToLuaMenu()
        {
        }

        static void ClearAllLuaFiles() {
            string[] pathList = new string[]{
                //lua生成路径
                ToLuaPathConfig.GenLuaPath,

                //AssetBundle
                EditorTools.AssetPathToAbsolutePath("Output/ToLua"),
                LuaTools.GetPersistentABPath("ToLua"),
                LuaTools.GetStreamingAssetsABPath("ToLua"),
            };

            foreach (string pathi in pathList) {
                EditorTools.ClearFolder(pathi);
                EditorTools.DeleteDirectory(pathi);
            }
        }

        [MenuItem("Lua/Generate All", false, 5)]
        static void GenLuaAll()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }

            var saveDir = ToLuaPathConfig.GenCsharpPath;
            Directory.CreateDirectory(saveDir);
            Generator.InitConfig();

            try {
                BindType[] bTypeList = Generator.LuaCallCSharp.ToArray();
                var tips = "";
                var progressValue = 0;
                var totalValue = bTypeList.Length+5;

                {
                    foreach (var bType in bTypeList)
                    {
                        ToLuaExport.Clear();
                        ToLuaExport.GenerateWrap(bType, saveDir);

                        tips = $"生成Wrap {bType.wrapName}Wrap.cs";
                        EditorTools.DisplayProgressBar(tips, progressValue++, totalValue);
                    }

                    AssetDatabase.Refresh();
                    Debugger.Log("Generate lua binding wrap files over");
                }

                {
                    ToLuaExport.Clear();
                    var list = Generator.CSharpCallLua.Select(t => new DelegateType(t)).ToArray();
                    var fileName = "DelegateGenFactory";
                    ToLuaExport.GenDelegates(null, list, saveDir, fileName);
                    Debugger.Log("Create lua delegate over");

                    tips = $"生成 {fileName}.cs";
                    EditorTools.DisplayProgressBar(tips, progressValue++, totalValue);
                }

                {
                    var fileName = "LuaBinder";
                    ToLuaExport.GenLuaBinder(saveDir, fileName);
                    Debugger.Log("Generate LuaBinder over !");

                    tips = $"生成 {fileName}.cs";
                    EditorTools.DisplayProgressBar(tips, progressValue++, totalValue);
                }

                {
                    var fileName = "ToLuaAutoRegister";
                    ToLuaExport.GenAutoRegister(saveDir, fileName);
                    Debugger.Log("Generate ToLua_Gen_Initer_Register__ over !");

                    tips = $"生成 {fileName}.cs";
                    EditorTools.DisplayProgressBar(tips, progressValue++, totalValue);
                }

                {
                    var fileName = "ToLuaAutoGenConfig";
                    ToLuaExport.GenAutoGenConfig(saveDir, fileName);
                    Debugger.Log("Generate ToLuaAutoGenConfig over !");

                    tips = $"生成 {fileName}.cs";
                    EditorTools.DisplayProgressBar(tips, progressValue++, totalValue);
                }

                {
                    EmmyLuaTools.ExportUnityAPI();
                    Debugger.Log($"Gen EmmyLua files over");

                    tips = $"生成 EmmyLua提示辅助文件";
                    EditorTools.DisplayProgressBar(tips, progressValue++, totalValue);
                }

                AssetDatabase.Refresh();

            } finally {
                EditorTools.ClearProgressBar();
            }
        }

        [MenuItem("Lua/Clear wrap files", false, 6)]
        static void ClearLuaWraps()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }
            var saveDir = ToLuaPathConfig.GenCsharpPath;
            EditorTools.ClearFolder(saveDir);

            ToLuaExport.Clear();
            List<DelegateType> list = new List<DelegateType>();
            ToLuaExport.GenDelegates(null, list.ToArray(), saveDir);
            ToLuaExport.Clear();

            AssetDatabase.Refresh();
        }

        public static void CopyLuaBytesFiles(string sourceDir, string destDir, bool appendext = true, string searchPattern = "*.lua")
        {
            Func<string, string> fnFileMap = null;
            if (appendext) fnFileMap = (savePath) => { return savePath + ".bytes"; };

            EditorTools.CopyDirectoryFileMap(sourceDir, destDir, searchPattern, fnFileMap);

            Debugger.Log("== copy " + sourceDir + " " + destDir);
        }

        [MenuItem("Lua/Copy Lua files/to ToLuaSource", false, 51)]
        public static void CopyLuaFilesToLuaSource()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }

            string luaDestDir = ToLuaPathConfig.GenLuaPath;
            EditorTools.ClearFolder(luaDestDir);
            foreach (var addLuaPath in ToLuaGlobal.GetLuaSearchPaths()) {
                CopyLuaBytesFiles(addLuaPath, luaDestDir);
            }
            Debugger.Log($"Copy lua files to {luaDestDir} over");

            EmmyLuaTools.ExportUnityAPI();
            Debugger.Log($"Gen EmmyLua files to {luaDestDir} over");

            AssetDatabase.Refresh();
        }

        public static void BuildLuaAsset(string abDestDir, BuildTarget target)
        {
            //拷贝lua文件到目录
            CopyLuaFilesToLuaSource();

            string luaDestDir = ToLuaPathConfig.GenLuaPath;
            var filePaths = Directory.GetFiles(luaDestDir, "*.bytes", SearchOption.AllDirectories)
                    .Select(f => EditorTools.AbsolutePathToAssetPath(f))
                    .ToList();
            AssetBundleBuild build = new() {
                assetBundleName = "lua" + LuaConst.ExtName,
                assetNames = filePaths.ToArray()
            };
            AssetBundleBuild[] buildMap = new AssetBundleBuild[] { build };
            EditorTools.CreateDirectory(abDestDir);
            BuildPipeline.BuildAssetBundles(abDestDir, buildMap, BuildAssetBundleOptions.None, target);
            AssetDatabase.Refresh();
        }

        [MenuItem("Lua/Build Lua AssetBundle/to output", false, 53)]
        public static void BuildLuaAssetToOutput()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }
            string abDestDir = EditorTools.AssetPathToAbsolutePath("Output/ToLua");
            BuildLuaAsset(abDestDir, EditorUserBuildSettings.activeBuildTarget);

            Debug.Log($"LuaBundle build to {abDestDir}");
        }

        [MenuItem("Lua/Build Lua AssetBundle/to Persistent Path", false, 53)]
        public static void BuildLuaAssetToPersistent()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }
            string abDestDir = LuaTools.GetPersistentABPath("ToLua");
            BuildLuaAsset(abDestDir, EditorUserBuildSettings.activeBuildTarget);

            Debug.Log($"LuaBundle build to {abDestDir}");
        }

        [MenuItem("Lua/Build Lua AssetBundle/to Streaming Asset Path", false, 53)]
        public static void BuildLuaAssetToStreamingAssetPath()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }
            string abDestDir = LuaTools.GetStreamingAssetsABPath("ToLua");
            BuildLuaAsset(abDestDir, EditorUserBuildSettings.activeBuildTarget);

            Debug.Log($"LuaBundle build to {abDestDir}");
        }

        [MenuItem("Lua/Clear all Lua files", false, 57)]
        public static void ClearLuaFiles()
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("警告", "请等待编辑器完成编译再执行此功能", "确定");
                return;
            }

            ClearAllLuaFiles();
            AssetDatabase.Refresh();
        }

        [MenuItem("Lua/Open Folder/Assets", false, 61)]
        public static void OpenFolder()
        {
            var targetDir = Application.dataPath;
            Application.OpenURL(targetDir);
        }

        [MenuItem("Lua/Open Folder/Streaming Assets", false, 61)]
        public static void OpenFolderStreamingAssets()
        {
            var targetDir = Application.streamingAssetsPath;
            Application.OpenURL(targetDir);
        }

        [MenuItem("Lua/Open Folder/Persistent", false, 61)]
        public static void OpenFolderPersistent()
        {
            var targetDir = Application.persistentDataPath;
            Application.OpenURL(targetDir);
        }

        [MenuItem("Lua/Enable Lua Injection &e", false, 102)]
        static void EnableLuaInjection()
        {
            bool EnableSymbols = false;
            if (UpdateMonoCecil(ref EnableSymbols) != -1)
            {
                BuildTargetGroup curBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                string existSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(curBuildTargetGroup);
                if (!existSymbols.Contains("ENABLE_LUA_INJECTION"))
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(curBuildTargetGroup, existSymbols + ";ENABLE_LUA_INJECTION");
                }

                AssetDatabase.Refresh();
            }
        }

    #if ENABLE_LUA_INJECTION
        [MenuItem("Lua/Injection Remove &r", false, 5)]
    #endif
        static void RemoveInjection()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("警告", "游戏运行过程中无法操作", "确定");
                return;
            }

            BuildTargetGroup curBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string existSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(curBuildTargetGroup);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(curBuildTargetGroup, existSymbols.Replace("ENABLE_LUA_INJECTION", ""));
            Debug.Log("Lua Injection Removed!");
        }

        public static int UpdateMonoCecil(ref bool EnableSymbols)
        {
            string appFileName = Environment.GetCommandLineArgs()[0];
            string appPath = Path.GetDirectoryName(appFileName);
            string directory = appPath + "/Data/Managed/";
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.OSXEditor)
            {
                directory = appPath.Substring(0, appPath.IndexOf("MacOS")) + "Managed/";
            }
            string suitedMonoCecilPath = directory + "Unity.Cecil.dll";
            string suitedMonoCecilMdbPath = directory + "Unity.Cecil.Mdb.dll";
            string suitedMonoCecilPdbPath = directory + "Unity.Cecil.Pdb.dll";
            string suitedMonoCecilToolPath = directory + "Unity.CecilTools.dll";

            if (!File.Exists(suitedMonoCecilPath)
                && !File.Exists(suitedMonoCecilMdbPath)
                && !File.Exists(suitedMonoCecilPdbPath)
            )
            {
                EnableSymbols = false;
                Debug.Log("Haven't found Mono.Cecil.dll!Symbols Will Be Disabled");
                return -1;
            }

            bool bInjectionToolUpdated = false;
            string injectionToolPath = ToLuaPathConfig.GetToLuaPath(LuaConst.injectionFilesPath) + "Editor/";
            string existMonoCecilPath = injectionToolPath + Path.GetFileName(suitedMonoCecilPath);
            string existMonoCecilPdbPath = injectionToolPath + Path.GetFileName(suitedMonoCecilPdbPath);
            string existMonoCecilMdbPath = injectionToolPath + Path.GetFileName(suitedMonoCecilMdbPath);
            string existMonoCecilToolPath = injectionToolPath + Path.GetFileName(suitedMonoCecilToolPath);

            try
            {
                bInjectionToolUpdated = TryUpdate(suitedMonoCecilPath, existMonoCecilPath) ? true : bInjectionToolUpdated;
                bInjectionToolUpdated = TryUpdate(suitedMonoCecilPdbPath, existMonoCecilPdbPath) ? true : bInjectionToolUpdated;
                bInjectionToolUpdated = TryUpdate(suitedMonoCecilMdbPath, existMonoCecilMdbPath) ? true : bInjectionToolUpdated;
                TryUpdate(suitedMonoCecilToolPath, existMonoCecilToolPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return -1;
            }
            EnableSymbols = true;

            return bInjectionToolUpdated ? 1 : 0;
        }

        static bool TryUpdate(string srcPath, string destPath)
        {
            if (GetFileContentMD5(srcPath) != GetFileContentMD5(destPath))
            {
                File.Copy(srcPath, destPath, true);
                return true;
            }

            return false;
        }

        static string GetFileContentMD5(string file)
        {
            if (!File.Exists(file))
            {
                return string.Empty;
            }

            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = StringBuilderCache.Acquire();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return StringBuilderCache.GetStringAndRelease(sb);
        }


        [MenuItem("Lua/Attach Profiler", false, 151)]
        static void AttachProfiler()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("警告", "请在运行时执行此功能", "确定");
                return;
            }

            LuaClient.Instance.AttachProfiler();
        }

        [MenuItem("Lua/Detach Profiler", false, 152)]
        static void DetachProfiler()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            LuaClient.Instance.DetachProfiler();
        }
    }
}
