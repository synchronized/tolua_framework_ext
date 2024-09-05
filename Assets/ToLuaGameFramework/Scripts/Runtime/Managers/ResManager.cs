using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using YooAsset;
using LuaInterface;

using static ToLuaGameFramework.UIManager;

namespace ToLuaGameFramework
{
    public static class ResManager
    {
        #region 创建对象

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
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("prefabPath为空");
                return;
            }
            LoadAssetFromAssetBundleAsyn(prefabPath, (GameObject prefab) => {
                GameObject go = GameObject.Instantiate(prefab);
                if (parent) go.transform.SetParent(parent, false);
                LuaBehaviour luaBehaviour = go.AddComponent<LuaBehaviour>();
                luaBehaviour.prefabPath = prefabPath;
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
            }).Forget();
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
            return LoadAssetSyn(typeof(T), assetPath) as T;
        }

        /// <summary>
        /// 同步获取资源(assetPath不带后缀名)
        /// </summary>
        public static UnityEngine.Object LoadAssetSyn(Type type, string assetPath)
        {
            return LoadAssetFromAssetBundleSyn(type, assetPath);
        }

        /// <summary>
        /// 异步获取资源(assetPath不带后缀名)
        /// </summary>
        public static void LoadAssetAsyn<T>(string assetPath, Action<T> callback) where T : UnityEngine.Object
        {
            LoadAssetFromAssetBundleAsyn<T>(assetPath, callback).Forget();
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
            AssetHandle  assetHandle = package.LoadAssetSync<T>(assetPath);
            return assetHandle.AssetObject as T;
        }

        /// <summary>
        /// 从AssetBundle里同步获取资源
        /// </summary>
        static UnityEngine.Object LoadAssetFromAssetBundleSyn(Type type, string _assetPath)
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetPath = $"{LuaConfig.GameResPath}/{_assetPath}";
            AssetHandle  assetHandle = package.LoadAssetSync(assetPath, type);
            return assetHandle.AssetObject;
        }

        /// <summary>
        /// 从AssetBundle里异步获取资源
        /// </summary>
        static async UniTask LoadAssetFromAssetBundleAsyn<T>(string _assetPath, Action<T> callback) where T : UnityEngine.Object
        {
            var package = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            var assetPath = $"{LuaConfig.GameResPath}/{_assetPath}";
            AssetHandle  assetHandle = package.LoadAssetAsync<T>(assetPath);
            await assetHandle;
            callback?.Invoke(assetHandle.AssetObject as T);
        }

    }
}
