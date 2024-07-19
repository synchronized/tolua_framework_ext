/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

namespace LuaInterface
{
    public static class ToLuaPathConfig
    {
        public static string RootPath = Path.Combine(Application.dataPath, "ToLua");
        public static string GenCsharpPath = null;
        public static string GenLuaPath = null;
        public static string AssetRootPath = null;
        public static string AssetGenLuaPath = null;

        static ToLuaPathConfig()
        {
            InitToLuaRootPath();
        }

        private static void InitToLuaRootPath()
        {
#if UNITY_EDITOR
            foreach(var type in (from type in Utils.GetAllTypes()
            where type.IsAbstract && type.IsSealed
            select type))
            {
                var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (field.IsDefined(typeof(ToLuaRootPathAttribute), false))
                    {
                        if (field.FieldType == typeof(string))
                        {
                            RootPath = field.GetValue(null) as string;
                        }
                        else
                        {
                            Debugger.LogWarning(String.Format("{0}.{1} type is {2} expect string", type.Name, field.Name, field.FieldType.Name));
                        }
                    }
                }

                var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var prop in props)
                {
                    if (prop.IsDefined(typeof(ToLuaRootPathAttribute), false))
                    {
                        if (prop.PropertyType == typeof(string))
                        {
                            RootPath = prop.GetValue(null, null) as string;
                        }
                        else {
                            Debugger.LogWarning(String.Format("{0}.{1} type is {2} expect string", type.Name, prop.Name, prop.PropertyType.Name));
                        }
                    }
                }
            }
            RootPath = LuaTools.GetRegularPath(RootPath);
            //去掉最后的斜杠
            if (RootPath.EndsWith("/")) RootPath = RootPath.Substring(0, RootPath.Length-1);
#else
            RootPath = LuaInterface.ObjectWrap.Config.ToLuaAutoGenConfig.ToLuaRootPath;
#endif

            GenCsharpPath = GetGenPath("Csharp");
            GenLuaPath = GetGenPath("Lua");

            AssetRootPath = LuaTools.AbsolutePathToAssetPath(RootPath);
            AssetGenLuaPath = LuaTools.AbsolutePathToAssetPath(GenLuaPath);
        }


        public static string GetToLuaPath(string path) {
            return LuaTools.GetRegularPath(Path.Combine(RootPath, path));
        }

        private static string GetGenPath(string path) {
            path = LuaTools.GetRegularPath(path);
            return GetToLuaPath($"Gen/{path}");
        }
    }

    public class ToLuaRootPathAttribute : Attribute
    {

    }

    public class ToLuaAddLuaPathAttribute : Attribute
    {
    }

}

namespace LuaInterface.ObjectWrap.Config
{
}
