
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LuaInterface
{
    public static class ToLuaGlobal
    {
        //默认lua搜索目录
        private static List<string> luaSearchPaths = new();

        static ToLuaGlobal()
        {
            InitLuaSearchPath();
        }

        private static void InitLuaSearchPath()
        {
#if UNITY_EDITOR
            ClearLuaSearchPaths();
            AddLuaSearchPath($"{ToLuaPathConfig.RootPath}/Lua");

            foreach(var type in (from type in Utils.GetAllTypes()
            where type.IsAbstract && type.IsSealed
            select type))
            {
                var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var field in fields)
                {
                    if (field.IsDefined(typeof(ToLuaAddLuaPathAttribute), false))
                    {
                        var obj = field.GetValue(null);
                        if (obj is string) {
                            AddLuaSearchPath(obj as string);
                        } else if (obj is IEnumerable<string>) {
                            AddRangeLuaSearchPath(obj as IEnumerable<string>);
                        } else {
                            Debugger.LogWarning(String.Format("{0}.{1} type is {2} expect string or IEnumerable<string>", type.Name, field.Name, field.FieldType.Name));
                        }
                    }
                }

                var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (var prop in props)
                {
                    if (prop.IsDefined(typeof(ToLuaAddLuaPathAttribute), false))
                    {
                        var obj = prop.GetValue(null, null);
                        if (obj is string) {
                            AddLuaSearchPath(obj as string);
                        } else if (obj is IEnumerable<string>) {
                            AddRangeLuaSearchPath(obj as IEnumerable<string>);
                        } else {
                            Debugger.LogWarning(String.Format("{0}.{1} type is {2} expect string or IEnumerable<string>", type.Name, prop.Name, prop.PropertyType.Name));
                        }
                    }
                }
            }
#endif
        }

        public static void ClearLuaSearchPaths()
        {
            luaSearchPaths.Clear();
        }

        public static void AddLuaSearchPath(string luaSearchPath)
        {
            luaSearchPaths.Add(luaSearchPath);
        }

        public static void AddRangeLuaSearchPath(IEnumerable<string> luaSearchPath)
        {
            luaSearchPaths.AddRange(luaSearchPath);
        }

        public static string[] GetLuaSearchPaths()
        {
            return luaSearchPaths.ToArray();
        }
    }
}
