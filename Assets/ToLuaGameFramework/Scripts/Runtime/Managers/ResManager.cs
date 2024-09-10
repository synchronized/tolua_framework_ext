using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;
using LuaInterface;

using static ToLuaGameFramework.UIManager;
using System.Collections.Generic;

namespace ToLuaGameFramework
{
    public static class ResManager
    {
        #region 创建对象

        interface IProgressHandler
        {
            void OnUpdate();
        }

        class SingleProgressHandler : IProgressHandler
        {
            public AssetHandle assetHandle;
            public Action<float> onProgress;

            public void OnUpdate() {
                onProgress?.Invoke(assetHandle.Progress);
            }
        }

        class MultiProgressHandler : IProgressHandler
        {
            private float progress;

            public Action<float> onProgress;

            public void SetProgress(float progress) {
                this.progress = progress;
            }

            public void OnUpdate() {
                onProgress?.Invoke(progress);
            }
        }

        static List<IProgressHandler> assetHandleList = new();

        /// <summary>
        /// 同步创建对象(prefabPath不带后缀名)
        /// </summary>
        public static GameObject SpawnPrefab(string prefabPath, Transform parent)
        {
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("prefabPath为空");
                return null;
            }
            GameObject prefab = LoadAssetFromAssetBundleSyn<GameObject>(prefabPath);
            GameObject go = GameObject.Instantiate(prefab);
            if (parent) go.transform.SetParent(parent, false);
            LuaBehaviour luaBehaviour = go.AddComponent<LuaBehaviour>();
            luaBehaviour.prefabPath = prefabPath;
            return luaBehaviour.gameObject;
        }

        /// <summary>
        /// 异步创建对象(prefabPath不带后缀名)
        /// </summary>
        public static void SpawnPrefabAsyn(string prefabPath, Transform parent, LuaFunction callback)
        {
            Action<Exception, GameObject> cb = (Exception e, GameObject go) => {
                if (callback != null)
                {
                    if (callback is M_LuaFunction function)
                    {
                        function.action.Invoke(e, go);
                    }
                    else
                    {
                        callback.Call(e, go);
                    }
                }
            };
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("prefabPath为空");
                throw new LuaException("prefabPath为空");
            }
            LoadAssetFromAssetBundleAsyn(prefabPath, (Exception e, GameObject prefab) => {
                if (e != null) {
                    cb(e, null);
                } else {
                    GameObject go = GameObject.Instantiate(prefab);
                    if (parent) go.transform.SetParent(parent, false);
                    LuaBehaviour luaBehaviour = go.AddComponent<LuaBehaviour>();
                    luaBehaviour.prefabPath = prefabPath;
                    cb(null, luaBehaviour.gameObject);
                }
            }, null).Forget();
        }

        /// <summary>
        /// lua调用，获取二进制数据
        /// </summary>
        [LuaByteBuffer]
        public static byte[] LLoadBinaryAssetSyn(string assetPath)
        {
            TextAsset asset = LoadAssetSyn<TextAsset>(assetPath);
            if (asset == null) return null;
            return asset.bytes;
        }

        public static T LoadAssetSyn<T>(string assetPath) where T : UnityEngine.Object
        {
            return LoadAssetFromAssetBundleSyn<T>(assetPath);
        }

        /// <summary>
        /// 异步获取资源(assetPath不带后缀名)
        /// </summary>
        public static void LoadAssetAsyn<T>(string assetPath, Action<Exception, T> onComplete, Action<float> onProgress) where T : UnityEngine.Object
        {
            LoadAssetFromAssetBundleAsyn<T>(assetPath, onComplete, onProgress).Forget();
        }

        public static void LLoadAssetListAsyn(string[] assetPaths, LuaFunction onComplete, LuaFunction onProgress)
        {
            LoadAssetListFromAssetBundleAsyn(assetPaths,
                (e) => onComplete?.Call(e?.Message),
                (p) => onProgress?.Call(p)).Forget();
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory()
        {
            //Debug.LogError("主动GC");
            Resources.UnloadUnusedAssets();
            GC.Collect();
            LuaManager.Instance.LuaGC();
        }
        #endregion

        static T LoadAssetFromAssetBundleSyn<T>(string _assetPath) where T : UnityEngine.Object
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetPath = $"{LuaConfig.GameResPath}/{_assetPath}";
            AssetHandle assetHandle = package.LoadAssetSync<T>(assetPath);
            if (assetHandle.Status != EOperationStatus.Succeed) {
                throw new ToLuaGameFrameworkException($"加载资源失败:{_assetPath}");
            }
            return assetHandle.AssetObject as T;
        }

        /// <summary>
        /// 从AssetBundle里同步获取资源
        /// </summary>
        static UnityEngine.Object LoadAssetFromAssetBundleSyn(Type type, string _assetPath)
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetPath = $"{LuaConfig.GameResPath}/{_assetPath}";
            AssetHandle assetHandle = package.LoadAssetSync(assetPath, type);
            if (assetHandle.Status != EOperationStatus.Succeed) {
                throw new ToLuaGameFrameworkException($"加载资源失败:{_assetPath}");
            }
            return assetHandle.AssetObject;
        }

        /// <summary>
        /// 从AssetBundle里异步获取资源
        /// </summary>
        static async UniTask LoadAssetFromAssetBundleAsyn<T>(string _assetPath, Action<Exception, T> onComplete, Action<float> onProgress) where T : UnityEngine.Object
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetPath = $"{LuaConfig.GameResPath}/{_assetPath}";
            AssetHandle assetHandle = package.LoadAssetAsync<T>(assetPath);
            IProgressHandler handle = null;
            if (onProgress != null) {
                handle = new SingleProgressHandler
                {
                    assetHandle = assetHandle,
                    onProgress = onProgress
                };
                assetHandleList.Add(handle);
            }
            await assetHandle;
            if (onProgress != null) {
                assetHandleList.Remove(handle);
            }
            if (assetHandle.Status != EOperationStatus.Succeed) {
                var e = new ToLuaGameFrameworkException($"加载资源失败:{_assetPath}");
                onComplete?.Invoke(e, null);
            } else {
                onComplete?.Invoke(null, assetHandle.AssetObject as T);
            }
        }

        static async UniTask LoadAssetListFromAssetBundleAsyn(string[] assetPathList, Action<Exception> onComplete, Action<float> onProgress)
        {
            if (assetPathList.Length == 0) {
                onComplete?.Invoke(null);
                return;
            }
            if (assetPathList.Length == 1) {
                await LoadAssetFromAssetBundleAsyn<GameObject>(assetPathList[0],
                    (e, go) => onComplete(e),
                    onProgress);
                return;
            }
            MultiProgressHandler handle = new()
            {
                onProgress = onProgress
            };
            assetHandleList.Add(handle);
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            for (var i=0; i<assetPathList.Length; i++) {
                var _assetPath = assetPathList[i];
                var assetPath = $"{LuaConfig.GameResPath}/{_assetPath}";
                AssetHandle assetHandle = package.LoadAssetAsync<GameObject>(assetPath);
                await assetHandle;
                if (assetHandle.Status != EOperationStatus.Succeed) {
                    var e = new ToLuaGameFrameworkException($"加载资源失败:{_assetPath}");
                    onComplete?.Invoke(e);
                    assetHandleList.Remove(handle);
                    return;
                }
                handle.SetProgress(1.0f*(i+1)/assetPathList.Length);
            }
            assetHandleList.Remove(handle);
            onComplete?.Invoke(null);
        }

        /// <summary>
        /// 网络管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal static void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var handle in assetHandleList) {
                handle.OnUpdate();
            }
        }
    }
}
