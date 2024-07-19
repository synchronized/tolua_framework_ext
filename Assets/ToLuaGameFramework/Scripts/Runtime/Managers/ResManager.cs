using Cysharp.Threading.Tasks;
using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static ToLuaGameFramework.UIManager;

namespace ToLuaGameFramework
{
    class FileStruct
    {
        public bool isForStartup;
        public string title;
        public string fileName;
        public string md5;
    }

    public class AssetBundleInfo
    {
        public AssetBundle ab;
        public int referencedCount;
        public AssetBundleInfo(AssetBundle ab)
        {
            this.ab = ab;
            referencedCount = 0;
        }
    }
    public class ResManager
    {
        private static ResManager m_Instance;

        public static ResManager Instance {
            get {
                if (m_Instance == null) {
                    m_Instance = new ResManager();
                }
                return m_Instance;
            }
        }


        static Dictionary<string, FileStruct> localFiles = new Dictionary<string, FileStruct>();
        static Dictionary<string, FileStruct> loadSuccessFiles = new Dictionary<string, FileStruct>();
        static Dictionary<string, AssetBundleInfo> loadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        static List<string> preloadAssetBundleNames = new List<string>();
        static int totalPreloadAssetBundles;
        UnityWebRequest resRequest;
        FileStruct currDownloadFile;
        //参数：标题，进度，是否全部完成
        Action<string, float, bool> onFinish;

        public void DoUpdate()
        {
            if (resRequest != null && currDownloadFile != null)
            {
                onFinish?.Invoke("", resRequest.downloadProgress, false);
            }
        }

        #region 下载远程AssetBundle

        /// <summary>
        /// 开始增量更新远程资源
        /// </summary>
        /// <returns></returns>
        public void StartUpdateABOnStartup(Action<string, float, bool> onFinish, Action<string, string> onError = null)
        {
            if (GlobalManager.ResLoadMode == ResLoadMode.NormalMode)
            {
                CheckAndDownloadAB(null, onFinish, onError).Forget();
            }
            else
            {
                onFinish?.Invoke("", 1, true);
            }
        }

        /// <summary>
        /// 指定AB包更新(回调参数：标题，进度，是否全部完成)
        /// </summary>
        /// <returns></returns>
        public void UpdateABsByNames(string[] abNames, Action<string, float, bool> onFinish, Action<string, string> onError)
        {
            CheckAndDownloadAB(abNames, onFinish, onError).Forget();
        }

        /// <summary>
        /// 更新AB包(abNames为null时，更新所有isForStartup的资源)(回调参数：标题，进度，是否全部完成)
        /// </summary>
        async UniTask CheckAndDownloadAB(string[] abNames, Action<string, float, bool> onFinish, Action<string, string> onError)
        {
            Debug.Log("开始下载资源");
            this.onFinish = onFinish;
            //读取本地MD5文件
            localFiles = new Dictionary<string, FileStruct>();
            string localFilesPath = LuaConfig.localABPath + "/" + LuaConfig.MD5FileName;
            Debug.Log("localFilesPath: " + localFilesPath);
            if (File.Exists(localFilesPath))
            {
                localFiles = ParseKeyValue(File.ReadAllText(localFilesPath));
            }
            //下载远程MD5文件
            string md5FilesUrl = LuaConfig.RemoteUrl + "/" + LuaConfig.MD5FileName;
            UnityWebRequest request = UnityWebRequest.Get(md5FilesUrl);
            try {
                await request.SendWebRequest();
            } catch(UnityWebRequestException) { }
            if (request.error != null)
            {
                Debug.LogError("[ " + md5FilesUrl + " ] error:" + request.error);
                onError?.Invoke(md5FilesUrl, request.error);
                return;
            }
            if (!Directory.Exists(LuaConfig.localABPath)) Directory.CreateDirectory(LuaConfig.localABPath);
            Dictionary<string, FileStruct> newestFiles = ParseKeyValue(request.downloadHandler.text);
            if (abNames != null)
            {
                for (int i = 0; i < abNames.Length; i++)
                {
                    string abName = ABName(abNames[i]);
                    if (!newestFiles.ContainsKey(abName))
                    {
                        Debug.LogError("服务器找不到" + abName);
                    }
                }
            }
            Dictionary<string, FileStruct> reloadFiles = new Dictionary<string, FileStruct>();

            if (GlobalManager.ResLoadMode == ResLoadMode.NormalMode)
            {
                foreach (var item in newestFiles)
                {
                    bool canLoad = false;
                    if (abNames != null)
                    {
                        for (int i = 0; i < abNames.Length; i++)
                        {
                            string abName = ABName(abNames[i]);
                            if (item.Key.Equals(abName))
                            {
                                canLoad = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        canLoad = item.Value.isForStartup;
                    }
                    if (canLoad)
                    {
                        bool localHaveKey = localFiles.ContainsKey(item.Key);
                        if (!localHaveKey)
                        {
                            //是新的文件，加入加载
                            Debug.Log(item.Key + " 是新的文件，加入加载");
                            if (item.Key.EndsWith(LuaConst.ExtName)) reloadFiles.Add(item.Key, item.Value);
                        }
                        else
                        {
                            bool fileExists = File.Exists(LuaConfig.localABPath + "/" + item.Key);
                            if (!fileExists)
                            {
                                //本地找不到，加入下载
                                Debug.Log(item.Key + " 本地找不到，加入下载");
                                if (item.Key.EndsWith(LuaConst.ExtName)) reloadFiles.Add(item.Key, item.Value);
                            }
                            else
                            {
                                FileStruct localInfo = localFiles[item.Key];
                                bool md5Match = localInfo.md5.Equals(item.Value.md5);
                                if (!md5Match)
                                {
                                    //文件有改动，加入下载
                                    Debug.Log(item.Key + " 文件有改动，加入下载");
                                    if (item.Key.EndsWith(LuaConst.ExtName)) reloadFiles.Add(item.Key, item.Value);
                                }
                            }
                        }
                    }
                }
            }
            Debug.Log("下载资源数量：" + reloadFiles.Count);
            foreach (var item in reloadFiles)
            {
                onFinish?.Invoke(item.Value.title, 0, false);

                string url = LuaConfig.RemoteUrl + "/" + item.Key;
                if (!item.Key.Contains(".")) continue;
                Debug.Log("下载" + item.Value.title + "资源：" + url);
                resRequest = UnityWebRequest.Get(url);
                currDownloadFile = item.Value;
                try {
                    await resRequest.SendWebRequest();
                } catch(UnityWebRequestException) { }
                if (resRequest.error != null)
                {
                    Debug.LogError(" [ " + url + " ] error:" + resRequest.error);
                    onError?.Invoke(item.Value.title, resRequest.error);
                    return;
                }
                string savePath = LuaConfig.localABPath + "/" + item.Key;
                string saveDir = savePath.Substring(0, savePath.LastIndexOf("/"));
                if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
                File.WriteAllBytes(savePath, resRequest.downloadHandler.data);
                loadSuccessFiles[item.Key] = item.Value;
            }
            if (resRequest != null)
            {
                resRequest.Dispose();
                resRequest = null;
            }
            currDownloadFile = null;
            UpdateLocalFiles();
            await UniTask.NextFrame();
            onFinish?.Invoke("", 1, true);
        }

        #endregion

        #region 预加载本地AssetBundle

        /// <summary>
        /// Lua调用,预加载AssetBundle列表，传入目录路径
        /// </summary>
        public static void PreloadLocalAssetBundles(string[] abPaths, LuaFunction onProgress)
        {

            if (GlobalManager.ResLoadMode != ResLoadMode.NormalMode)
            {
                onProgress?.Call(1);
                return;
            }
            preloadAssetBundleNames.Clear();
            for (int i = 0; i < abPaths.Length; i++)
            {
                string path = abPaths[i];
                string abName = null;
                string prefabName = null;
                ParseAssetPath(path, out abName, out prefabName);
                foreach (var item in localFiles.Keys)
                {
                    if (item.EndsWith(LuaConst.ExtName) && item.StartsWith(abName))
                    {
                        string name = item.Substring(0, item.Length - LuaConst.ExtName.Length);
                        if (!preloadAssetBundleNames.Contains(name))
                        {
                            preloadAssetBundleNames.Add(name);
                        }
                    }
                }
            }
            totalPreloadAssetBundles = preloadAssetBundleNames.Count;
            PreloadAssetBundle(onProgress).Forget();
        }

        /// <summary>
        /// Lua调用,将预加载好的AssetBundle全部卸载，是否包括它的所有Spawn,由参数传入
        /// </summary>
        public void UnloadAllAssetBundles(bool unloadAllLoadedObjects)
        {
            foreach (var item in loadedAssetBundles.Values)
            {
                AssetBundle ab = item.ab;
                ab.Unload(unloadAllLoadedObjects);
            }
            loadedAssetBundles.Clear();
            ClearMemory();
        }

        #endregion

        #region 创建对象

        /// <summary>
        /// 同步创建对象(prefabPath不带后缀名)
        /// </summary>
        public static GameObject SpawnPrefab(string prefabPath, Transform parent, bool unloadABAfterSpawn = false, bool unloadABAfterAllSpawnDestroy = false)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("prefabPath为空");
                return null;
            }
            string abName = "";
            GameObject prefab = null;
#if UNITY_EDITOR
            if (GlobalManager.ResLoadMode != ResLoadMode.NormalMode)
            {
                string prefabFullPath = Application.dataPath + "/" + LuaConfig.GameResPath + "/" + prefabPath + ".prefab";
                prefabFullPath = prefabFullPath.Substring(prefabFullPath.IndexOf("Assets/"));
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabFullPath);
            }
            else
#endif
            {
                string prefabName = null;
                ParseAssetPath(prefabPath, out abName, out prefabName);
                prefab = LoadAssetFromAssetBundleSyn<GameObject>(abName, prefabName, unloadABAfterSpawn);
            }
            if (prefab == null)
            {
                Debug.LogError(string.Format("加载失败 prefabPath:{0}", prefabPath));
                return null;
            }
            GameObject go = GameObject.Instantiate(prefab);
            if (parent) go.transform.SetParent(parent, false);
            LuaBehaviour luaBehaviour = go.AddComponent<LuaBehaviour>();
            luaBehaviour.assetBundleName = abName;
            luaBehaviour.prefabPath = prefabPath;
            luaBehaviour.unloadABAfterAllSpawnDestroy = unloadABAfterAllSpawnDestroy;
            return luaBehaviour.gameObject;
        }

        /// <summary>
        /// 异步创建对象(prefabPath不带后缀名)
        /// </summary>
        public static void SpawnPrefabAsyn(string prefabPath, Transform parent, LuaFunction callback, bool unloadABAfterSpawn = false, bool unloadABAfterAllSpawnDestroy = false)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("prefabPath为空");
                return;
            }
            string abName = "";
            void createCallback(GameObject prefab)
            {
                if (prefab == null) {
                    Debug.LogError(string.Format("加载失败 prefabPath:{0}", prefabPath));
                    return;
                }
                GameObject go = GameObject.Instantiate(prefab);
                if (parent) go.transform.SetParent(parent, false);
                LuaBehaviour luaBehaviour = go.AddComponent<LuaBehaviour>();
                luaBehaviour.assetBundleName = abName;
                luaBehaviour.prefabPath = prefabPath;
                luaBehaviour.unloadABAfterAllSpawnDestroy = unloadABAfterAllSpawnDestroy;
                if (!string.IsNullOrEmpty(callback + ""))
                {
                    if (callback.GetType() == typeof(M_LuaFunction))
                    {
                        ((M_LuaFunction)callback).action.Invoke(luaBehaviour.gameObject);
                    }
                    else
                    {
                        callback.Call(luaBehaviour.gameObject, false);
                    }
                }
            }
#if UNITY_EDITOR
            if (GlobalManager.ResLoadMode != ResLoadMode.NormalMode)
            {
                string prefabFullPath = Application.dataPath + "/" + LuaConfig.GameResPath + "/" + prefabPath + ".prefab";
                prefabFullPath = prefabFullPath.Substring(prefabFullPath.IndexOf("Assets/"));
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabFullPath);
                createCallback(prefab);
            }
            else
#endif
            {
                string prefabName = null;
                ParseAssetPath(prefabPath, out abName, out prefabName);
                LoadAssetFromAssetBundleAsyn(abName, prefabName, (Action<GameObject>)createCallback, unloadABAfterSpawn).Forget();
            }
        }

        /// <summary>
        /// 清除AssetBundle(自动判断引用数量为0时回收AssetBundle)
        /// </summary>
        public void OnSpawnDestroy(string bundleName, bool unloadABAfterAllSpawnDestroy)
        {
            if (loadedAssetBundles.ContainsKey(bundleName))
            {
                AssetBundleInfo abInfo = loadedAssetBundles[bundleName];
                abInfo.referencedCount--;
                if (abInfo.referencedCount <= 0 && unloadABAfterAllSpawnDestroy)
                {
                    abInfo.ab.Unload(true);
                    loadedAssetBundles.Remove(bundleName);
                }
            }
        }

        /// <summary>
        /// lua调用，获取二进制数据
        /// </summary>
        [LuaByteBufferAttribute]
        public static byte[] LLoadBinaryAssetSyn(string assetPath, bool unloadABAfterSpawn = false)
        {
            TextAsset asset = LoadAssetSyn<TextAsset>(assetPath, unloadABAfterSpawn);
            return asset.bytes;
        }


        public static T LoadAssetSyn<T>(string assetPath, bool unloadABAfterSpawn = false) where T : UnityEngine.Object
        {
            return LoadAssetSyn(typeof(T), assetPath, unloadABAfterSpawn) as T;
        }

        /// <summary>
        /// 同步获取资源(assetPath不带后缀名)
        /// </summary>
        public static UnityEngine.Object LoadAssetSyn(Type type, string assetPath, bool unloadABAfterSpawn = false)
        {
#if UNITY_EDITOR
            if (GlobalManager.ResLoadMode != ResLoadMode.NormalMode)
            {
                string prefabFullPath = AddSuffix(Application.dataPath + "/" + LuaConfig.GameResPath + "/" + assetPath);
                prefabFullPath = prefabFullPath.Substring(prefabFullPath.IndexOf("Assets/"));
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(prefabFullPath, type);
                return asset;
            }
            else
#endif
            {
                string abName = null;
                string assetName = null;
                ParseAssetPath(assetPath, out abName, out assetName);
                return LoadAssetFromAssetBundleSyn(type, abName, assetName, unloadABAfterSpawn);
            }
        }

        /// <summary>
        /// 异步获取资源(assetPath不带后缀名)
        /// </summary>
        public static void LoadAssetAsyn<T>(string assetPath, Action<T> callback, bool unloadABAfterSpawn = false) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if (GlobalManager.ResLoadMode != ResLoadMode.NormalMode)
            {
                string prefabFullPath = AddSuffix(Application.dataPath + "/" + LuaConfig.GameResPath + "/" + assetPath);
                prefabFullPath = prefabFullPath.Substring(prefabFullPath.IndexOf("Assets/"));
                T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(prefabFullPath);
                callback?.Invoke(asset);
            }
            else
#endif
            {
                string abName = null;
                string assetName = null;
                ParseAssetPath(assetPath, out abName, out assetName);
                LoadAssetFromAssetBundleAsyn(abName, assetName, callback, unloadABAfterSpawn).Forget();
            }
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public void ClearMemory()
        {
            //Debug.LogError("主动GC");
            Resources.UnloadUnusedAssets();
            GC.Collect();
            LuaManager.Instance.LuaGC();
        }
        #endregion

        #region 内部方法

        /// <summary>
        /// 自动查找文件加上后缀名
        /// </summary>
        static string AddSuffix(string assetPath)
        {
            string path = assetPath.Substring(0, assetPath.LastIndexOf('/'));
            string name = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
            string[] files = Directory.GetFiles(path, name + ".*", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < files.Length; i++)
            {
                string _path = files[i];
                if (!_path.EndsWith(".meta"))
                {
                    return _path;
                }
            }
            return null;
        }


        static T LoadAssetFromAssetBundleSyn<T>(string abName, string assetName, bool unloadABAfterSpawn = false) where T : UnityEngine.Object
        {
            return LoadAssetFromAssetBundleSyn(typeof(T), abName, assetName, unloadABAfterSpawn) as T;
        }

        /// <summary>
        /// 从AssetBundle里同步获取资源
        /// </summary>
        static UnityEngine.Object LoadAssetFromAssetBundleSyn(Type type, string abName, string assetName, bool unloadABAfterSpawn = false)
        {
            UnityEngine.Object prefab = null;
            AssetBundleInfo abInfo = null;
            loadedAssetBundles.TryGetValue(abName, out abInfo);
            if (abInfo == null)
            {
                string localUrl = LuaConfig.localABPath + "/" + abName + LuaConst.ExtName;
                AssetBundle ab = AssetBundle.LoadFromFile(localUrl);
                if (unloadABAfterSpawn)
                {
                    prefab = ab.LoadAsset(assetName, type);
                    ab.Unload(false);
                }
                else
                {
                    abInfo = new AssetBundleInfo(ab);
                    prefab = abInfo.ab.LoadAsset(assetName, type);
                    abInfo.referencedCount++;
                    loadedAssetBundles.Add(abName, abInfo);
                }
            }
            else
            {
                prefab = abInfo.ab.LoadAsset(assetName, type);
                abInfo.referencedCount++;
                if (unloadABAfterSpawn)
                {
                    abInfo.ab.Unload(false);
                    loadedAssetBundles.Remove(abName);
                }
            }
            return prefab;
        }

        /// <summary>
        /// 从AssetBundle里异步获取资源
        /// </summary>
        async static UniTask LoadAssetFromAssetBundleAsyn<T>(string abName, string assetName, Action<T> callback, bool unloadABAfterSpawn = false) where T : UnityEngine.Object
        {
            AssetBundleInfo abInfo = null;
            loadedAssetBundles.TryGetValue(abName, out abInfo);
            if (abInfo == null)
            {
                string localUrl = LuaConfig.localABPath + "/" + abName + LuaConst.ExtName;
                UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(localUrl);
                await request.SendWebRequest();
                if (request.error != null)
                {
                    Debug.LogError(" [ " + localUrl + " ] " + request.error);
                }
                AssetBundle ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                if (unloadABAfterSpawn)
                {
                    T prefab = ab.LoadAsset<T>(assetName);
                    callback?.Invoke(prefab);
                    ab.Unload(false);
                }
                else
                {
                    abInfo = new AssetBundleInfo(ab);
                    T prefab = abInfo.ab.LoadAsset<T>(assetName);
                    if (callback != null)
                    {
                        callback(prefab);
                        abInfo.referencedCount++;
                    }
                    loadedAssetBundles.Add(abName, abInfo);
                }
            }
            else
            {
                T prefab = abInfo.ab.LoadAsset<T>(assetName);
                if (callback != null)
                {
                    callback(prefab);
                    abInfo.referencedCount++;
                }
                if (unloadABAfterSpawn)
                {
                    abInfo.ab.Unload(false);
                    loadedAssetBundles.Remove(abName);
                }
            }
        }

        static void ParseAssetPath(string assetPath, out string abName, out string assetName)
        {
            abName = assetPath.ToLower();
            assetName = assetPath;
            if (assetPath.Contains("/"))
            {
                assetName = assetPath.Substring(assetPath.LastIndexOf("/") + 1);
                abName = assetPath.Substring(0, assetPath.LastIndexOf("/")).ToLower();
                if (abName.Contains("/"))
                {
                    abName = abName.Replace("/", "_");
                    string localUrl = LuaConfig.localABPath + "/" + abName + LuaConst.ExtName;
                    while (!File.Exists(localUrl) && abName.Contains("_"))
                    {
                        abName = abName.Substring(0, abName.LastIndexOf("_"));
                        localUrl = LuaConfig.localABPath + "/" + abName + LuaConst.ExtName;
                    }
                }
            }
        }

        async static UniTask PreloadAssetBundle(LuaFunction onProgress)
        {
            while (preloadAssetBundleNames.Count > 0)
            {
                string abName = preloadAssetBundleNames[0];
                preloadAssetBundleNames.RemoveAt(0);
                AssetBundleInfo abInfo = null;
                loadedAssetBundles.TryGetValue(abName, out abInfo);
                if (abInfo == null)
                {
                    string localUrl = LuaConfig.localABPath + "/" + abName + LuaConst.ExtName;
                    UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(localUrl);
                    await request.SendWebRequest();
                    if (request.error != null)
                    {
                        Debug.LogError(" [ " + localUrl + " ] " + request.error);
                    }
                    AssetBundle ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                    abInfo = new AssetBundleInfo(ab);
                    loadedAssetBundles.Add(abName, abInfo);
                }
                onProgress?.Call((totalPreloadAssetBundles - preloadAssetBundleNames.Count) / (float)totalPreloadAssetBundles);
            }
        }

        Dictionary<string, FileStruct> ParseKeyValue(string filesContent)
        {
            Dictionary<string, FileStruct> infos = new Dictionary<string, FileStruct>();
            try
            {
                string[] files = filesContent.Split('\n');
                for (int i = 0; i < files.Length; i++)
                {
                    if (string.IsNullOrEmpty(files[i])) continue;
                    FileStruct info = new FileStruct();
                    string[] keyValue = files[i].Split('|');
                    info.isForStartup = "0".Equals(keyValue[0]);
                    info.title = keyValue[1];
                    info.fileName = keyValue[2];
                    info.md5 = keyValue[3].Replace("\n", "").Trim();
                    infos.Add(info.fileName, info);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return infos;
        }
        void UpdateLocalFiles()
        {
            //仅更新下载成功的文件
            foreach (var item in loadSuccessFiles)
            {
                localFiles[item.Key] = item.Value;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in localFiles)
            {
                string isForStartup = item.Value.isForStartup ? "0" : "1";
                stringBuilder.Append(isForStartup + "|" + item.Value.title + "|" + item.Key + "|" + item.Value.md5 + "\n");
            }
            File.WriteAllText(LuaConfig.localABPath + "/" + LuaConfig.MD5FileName, stringBuilder.ToString());
        }

        string ABName(string abName)
        {
            abName = abName.Replace("/", "_").ToLower();
            if (!abName.EndsWith(".zip")) abName += ".zip";
            return abName;
        }
        #endregion
    }

}
