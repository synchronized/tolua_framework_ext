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
//优先读取persistentDataPath/系统/Lua 目录下的文件（默认下载目录）
//未找到文件怎读取 Resources/Lua 目录下文件（仍没有使用LuaFileUtil读取）
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LuaInterface {

    public enum LuaLoadMode {
        SimulateMode, //模拟模式(仅在编辑器模式下可用)
        NormalMode, //正常模式
    }

    public interface ILuaLoader {
        byte[] LoadLuaFile(string fileName);
        string FindFileError(string fileName);
        void Dispose();
    }

    public class LuaLoader {

        private static LuaLoader instance;

        public static LuaLoader Instance {
            get {
                instance ??= new LuaLoader();
                return instance;
            }
            protected set {
                instance = value;
            }
        }

        public static LuaLoadMode LoadMode = LuaLoadMode.SimulateMode;

        protected List<ILuaLoader> loaderList = new List<ILuaLoader>();
        protected DefaultLuaLoader defaultLoader = new DefaultLuaLoader();

        public LuaLoader()
        {
        }

        #region instance function
        public void ODispose() {
            if (instance != null) {
                instance = null;

                foreach (var loader in loaderList) {
                    loader.Dispose();
                }
                loaderList.Clear();

                defaultLoader.Dispose();
            }
        }

        public void OEnableResourceLuaLoader() {
            GetOrAddLoader<ResourceLuaLoader>();
        }

        public bool OAddSearchPackage(string path, bool front = false) {
            return defaultLoader.AddSearchPackage(path, front);
        }

        public bool ORemoveSearchPackage(string path) {
            return defaultLoader.RemoveSearchPackage(path);
        }

        public bool OAddLoader(ILuaLoader loader, bool front = false) {
            if (front) loaderList.Insert(0, loader);
            else loaderList.Add(loader);
            return true;
        }

        public void ORemoveLoader(ILuaLoader loader) {
            loaderList.Remove(loader);
        }

        public ILuaLoader OGetLoader(Type loaderType) {
            if (loaderType == typeof(DefaultLuaLoader)) return defaultLoader;
            foreach (ILuaLoader loader in loaderList) {
                if (loader.GetType() == loaderType) return loader;
            }
            return null;
        }

        public T OGetLoader<T>() where T : class, ILuaLoader {
            if (typeof(T) == typeof(DefaultLuaLoader)) return defaultLoader as T;
            foreach (ILuaLoader loader in loaderList) {
                if (loader.GetType() == typeof(T)) return loader as T;
            }
            return null;
        }

        public ILuaLoader OGetOrAddLoader(Type loaderType) {
            var result = OGetLoader(loaderType);
            if (result != null) return null;

            result = System.Activator.CreateInstance(loaderType) as ILuaLoader;
            OAddLoader(result);
            return result;
        }

        public T OGetOrAddLoader<T>() where T : class, ILuaLoader {
            var result = OGetLoader<T>();
            if (result != null) return result;

            result = System.Activator.CreateInstance<T>();
            OAddLoader(result);
            return result;
        }

        public string OFindFile(string fileName) {
            return defaultLoader.FindFile(fileName);
        }

        public byte[] OReadFile(string fileName) {
            foreach (var loader in loaderList) {
                var content = loader.LoadLuaFile(fileName);
                if (content != null) return content;
            }
            return defaultLoader.LoadLuaFile(fileName);
        }

        public string OFindFileError(string fileName) {
            using (CString.Block()) {
                CString sb = CString.Alloc(512);

                foreach (var loader in loaderList) {
                    sb.Append(loader.FindFileError(fileName));
                }
                sb.Append(defaultLoader.FindFileError(fileName));
                return sb.ToString();
            }
        }
        #endregion

        #region 静态方法

        public static void Dispose() { Instance.ODispose(); }

        public static void EnableResourceLuaLoader() { Instance.OEnableResourceLuaLoader(); }

        public static bool AddSearchPackage(string path, bool front = false) {
            return Instance.OAddSearchPackage(path, front);
        }

        public static bool RemoveSearchPackage(string path) {
            return Instance.ORemoveSearchPackage(path);
        }

        public static bool AddLoader(ILuaLoader loader, bool front = false) {
            return Instance.OAddLoader(loader, front);
        }

        public static void RemoveLoader(ILuaLoader loader) {
            Instance.ORemoveLoader(loader);
        }

        public static ILuaLoader GetLoader(Type loaderType) {
            return Instance.OGetLoader(loaderType);
        }

        public static T GetLoader<T>() where T : class, ILuaLoader {
            return Instance.OGetLoader<T>();
        }

        public static ILuaLoader GetOrAddLoader(Type loaderType) {
            return Instance.OGetOrAddLoader(loaderType);
        }

        public static T GetOrAddLoader<T>() where T : class, ILuaLoader {
            return Instance.OGetOrAddLoader<T>();
        }

        public static string FindFile(string fileName) {
            return Instance.OFindFile(fileName);
        }

        public static byte[] ReadFile(string fileName) {
            return Instance.OReadFile(fileName);
        }

        public static string FindFileError(string fileName) {
            return Instance.OFindFileError(fileName);
        }
        #endregion
    }

    public class DefaultLuaLoader : ILuaLoader {

        protected List<string> searchPaths = new List<string>();

        public void Dispose() {
            searchPaths.Clear();
        }

        public byte[] LoadLuaFile(string fileName) {
            string path = FindFile(fileName);
            byte[] str = null;

            if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
#if !UNITY_WEBPLAYER
                str = File.ReadAllBytes(path);
#else
                throw new LuaException("can't run in web platform, please switch to other platform");
#endif
            }

            return str;
        }

        public string FindFileError(string fileName) {
            if (Path.IsPathRooted(fileName)) {
                return fileName;
            }

            if (fileName.EndsWith(".lua")) {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }

            using (CString.Block()) {
                CString sb = CString.Alloc(512);

                for (int i = 0; i < searchPaths.Count; i++) {
                    sb.Append("\n\tno file '").Append(searchPaths[i]).Append('\'');
                }

                sb = sb.Replace("?", fileName);
                return sb.ToString();
            }
        }

        public string FindFile(string fileName) {
            if (fileName == string.Empty) {
                return string.Empty;
            }

            if (Path.IsPathRooted(fileName)) {
                if (!fileName.EndsWith(".lua")) {
                    fileName += ".lua";
                }

                return fileName;
            }

            if (fileName.EndsWith(".lua")) {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }

            for (int i = 0; i < searchPaths.Count; i++) {
                string fullPath = searchPaths[i].Replace("?", fileName);
                if (File.Exists(fullPath)) {
                    return fullPath;
                }
            }

            return null;
        }

        private string ToPackagePath(string path) {
            using (CString.Block()) {
                CString sb = CString.Alloc(256);
                sb.Append(path);
                sb.Replace('\\', '/').Replace('\\', '/');

                if (!sb.EndsWith(".lua")) {
                    if (sb.Length > 0 && sb[sb.Length - 1] != '/') {
                        sb.Append('/');
                    }
                    sb.Append("?.lua");
                }
                return sb.ToString();
            }
        }

        public bool AddSearchPackage(string fullPath, bool front = false) {
            //if (!Path.IsPathRooted(fullPath)) throw new LuaException(fullPath + " is not a full path");

            fullPath = ToPackagePath(fullPath);
            return AddSearchPath(fullPath, front);
        }

        public bool RemoveSearchPackage(string fullPath) {
            //if (!Path.IsPathRooted(fullPath)) throw new LuaException(fullPath + " is not a full path");

            fullPath = ToPackagePath(fullPath);
            return RemoveSearchPath(fullPath);
        }

        private bool AddSearchPath(string path, bool front = false) {
            if (searchPaths.IndexOf(path) >= 0) return false;

            if (front) searchPaths.Insert(0, path);
            else searchPaths.Add(path);

            return true;
        }

        private bool RemoveSearchPath(string path) {
            int index = searchPaths.IndexOf(path);

            if (index >= 0) {
                searchPaths.RemoveAt(index);
                return true;
            }

            return false;
        }
    }

    public class ResourceLuaLoader : ILuaLoader {

        public void Dispose() {}

        public byte[] LoadLuaFile(string fileName) {
            if (!fileName.EndsWith(".lua")) fileName += ".lua";

            byte[] buffer = null;
            string path = "Lua/" + fileName;
            TextAsset textAsset = Resources.Load<TextAsset>(path);

            if (textAsset != null) {
                buffer = textAsset.bytes;
                Resources.UnloadAsset(textAsset);
            }

            return buffer;
        }

        public string FindFileError(string fileName) {
            if (Path.IsPathRooted(fileName)) return fileName;

            if (!fileName.EndsWith(".lua")) fileName += ".lua";

            return $"\n\tno file 'Resources/Lua/{fileName}";
        }

    }

    public class SingleAssetLuaLoader : ILuaLoader {

        protected string m_bundleName;
        protected AssetBundle m_assetBundle;

        public void Dispose() {
            m_bundleName = null;
            if (m_assetBundle != null) {
                m_assetBundle.Unload(true);
                m_assetBundle = null;
            }
        }

        public byte[] LoadLuaFile(string fileName) {
            byte[] buffer = null;

            if (!fileName.EndsWith(".lua")) fileName += ".lua";

            var _fileName = $"{ToLuaPathConfig.AssetGenLuaPath}/{fileName}.bytes";

            if (m_assetBundle != null) {
                TextAsset luaCode = m_assetBundle.LoadAsset<TextAsset>(_fileName);
                if (luaCode != null) {
                    buffer = luaCode.bytes;
                    Resources.UnloadAsset(luaCode);
                } else {
                    Debugger.LogError($"no found fileName: {fileName} in {m_bundleName}");
                    Debugger.LogError($"no found _fileName: {_fileName} in {m_bundleName}");
                }
            }

            return buffer;
        }

        public string FindFileError(string fileName) {
            if (Path.IsPathRooted(fileName)) {
                return fileName;
            }

            if (!fileName.EndsWith(".lua")) fileName += ".lua";
            fileName += ".bytes";
            if (m_bundleName != null) {
                return $"\n\tno file '{fileName}' in {m_bundleName}";
            }
            return "";
        }

        public void SetSearchBundle(string name, AssetBundle bundle) {
            m_bundleName = name;
            m_assetBundle = bundle;
        }

    }
}
