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
using UnityEngine;
using LuaInterface;
using YooAsset;

namespace ToLuaGameFramework
{

    public class YooAssetLuaResLoader : ILuaLoader
    {

        private ResourcePackage luaPackage;

        public YooAssetLuaResLoader()
        {
            luaPackage = YooAssets.GetPackage(GlobalManager.DefaultPackage);
        }

        public byte[] LoadLuaFile(string fileName)
        {
            if (!fileName.EndsWith(".lua")) fileName+=".lua";
            var filePath = $"{ToLuaPathConfig.AssetGenLuaPath}/{fileName}";
            AssetHandle handle = luaPackage.LoadAssetSync<TextAsset>(filePath);
            TextAsset textAsset = handle.AssetObject as TextAsset;
            return textAsset.bytes; //二进制数据
        }

        public string FindFileError(string fileName)
        {
            if (Path.IsPathRooted(fileName)) return fileName;

            if (Path.GetExtension(fileName) == ".lua")
            {
                fileName = fileName.Substring(0, fileName.Length - 4);
            }

            return $"\n\tno file \"{fileName}.lua\" in YooAsset.Package:DefaultPackage";
        }

        public void Dispose() {
            luaPackage.UnloadUnusedAssetsAsync();
        }

        public static void AddLoader()
        {
            LuaLoader.GetOrAddLoader<YooAssetLuaResLoader>();
        }
    }
}
