/**
 * wuqibo
 * 2021.9.2
 * 导出Unity的API已便支持EmmyLua的代码提示
 * 枚举除了代码提示，还需要参与代码逻辑引用，所以独立导出文件方便require
 * 添加类方法：在CustomSettings.cs脚本中添加到customTypeList中即可
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LuaInterface.Editor
{
    public class EmmyLuaTools
    {
        class Method
        {
            public Type returnType;
            public ParameterInfo[] parameters;
            public Method(Type returnType, ParameterInfo[] parameters)
            {
                this.returnType = returnType;
                this.parameters = parameters;
            }
        }

        public static void ExportUnityAPI()
        {

            Generator.InitConfig();

            var saveDir = ToLuaPathConfig.GetToLuaPath("Lua");
            EditorTools.CreateDirectory(saveDir);
            var list = Generator.LuaCallCSharp;
            StringBuilder sb = new StringBuilder();
            try {
                {
                    var clazzlist = list.Where(bt => bt.type.BaseType != typeof(Enum)).ToList();
                    int total = clazzlist.Count;
                    for (int i = 0; i < total; i++)
                    {
                        ExportClass(sb, clazzlist[i].type, false);
                    }

                    var file = $"{saveDir}/unity_api.lua";
                    using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
                    {
                        textWriter.Write(sb.ToString());
                        textWriter.Flush();
                        textWriter.Close();
                    }
                    sb.Clear();
                }

                {
                    var enumlist = list.Where(bt => bt.type.BaseType == typeof(Enum)).ToList();
                    int total = enumlist.Count;
                    for (int i = 0; i < total; i++)
                    {
                        ExportClass(sb, enumlist[i].type, false);
                    }

                    var file = $"{saveDir}/unity_enum.lua";
                    using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
                    {
                        textWriter.Write(sb.ToString());
                        textWriter.Flush();
                        textWriter.Close();
                    }
                    sb.Clear();
                }
            } finally {
            }
        }

        /// <summary>
        /// includeAllBaseClas等于false时，会忽略所有父类的属性和方法，且自动添加注解为 ---@class XXXXX : BaseClass，父类则需要单独导出
        /// </summary>
        public static void ExportClass(StringBuilder sb, Type clazz, bool includeAllBaseClas = true)
        {
            //处理枚举（枚举导出后需要参与逻辑运算，所以导出独立的文件，务必添加require）
            if (clazz.BaseType == typeof(Enum))
            {
                FieldInfo[] fieldInfos = clazz.GetFields();
                sb.Append("\n");
                sb.AppendFormat("---@class {0} @ -------------------------------------\n", Replace(clazz.Name));
                sb.Append(Replace(clazz.Name) + " = {\n");
                foreach (var fieldInfo in fieldInfos)
                {
                    sb.AppendFormat("    {0} = {1}.{2},\n", fieldInfo.Name, fieldInfo.DeclaringType.FullName.Replace("+", "."), fieldInfo.Name);
                }
                sb.Append("}\n");
                return;
            }

            //处理非枚举的类
            Dictionary<string, Type> properties = new Dictionary<string, Type>();
            List<string> staticProperties = new List<string>();
            Dictionary<string, Method> publicMethods = new Dictionary<string, Method>();
            Dictionary<string, Method> staticMethods = new Dictionary<string, Method>();
            //属性
            var flag = BindingFlags.Public | BindingFlags.Instance;
            if (!includeAllBaseClas) flag |= BindingFlags.DeclaredOnly;
            var _properties = clazz.GetProperties(flag);
            var _fields = clazz.GetFields(flag);
            for (int i = 0; i < _properties.Length; i++)
            {
                if (!properties.ContainsKey(_properties[i].Name)) properties.Add(_properties[i].Name, _properties[i].PropertyType);
            }
            for (int i = 0; i < _fields.Length; i++)
            {
                if (!properties.ContainsKey(_fields[i].Name)) properties.Add(_fields[i].Name, _fields[i].FieldType);
            }
            //静态变量
            PropertyInfo[] _staticProperties = clazz.GetProperties(BindingFlags.Public | BindingFlags.Static);
            FieldInfo[] _staticFields = clazz.GetFields(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < _staticProperties.Length; i++)
            {
                if (!staticProperties.Contains(_staticProperties[i].Name)) staticProperties.Add(_staticProperties[i].Name);
            }
            for (int i = 0; i < _staticFields.Length; i++)
            {
                if (!staticProperties.Contains(_staticFields[i].Name)) staticProperties.Add(_staticFields[i].Name);
            }

            //公共方法
            MethodInfo[] _publicMethods = null;
            flag = BindingFlags.Public | BindingFlags.Instance;
            if (!includeAllBaseClas) flag |= BindingFlags.DeclaredOnly;
            _publicMethods = clazz.GetMethods(flag);
            for (int i = 0; i < _publicMethods.Length; i++)
            {
                string methodName = _publicMethods[i].Name;
                char firstChar = methodName[0];
                if (firstChar >= 'A' && firstChar <= 'Z')
                {
                    //只取大写开头的方法
                    if (!publicMethods.ContainsKey(methodName)) publicMethods.Add(methodName, GetOneMethodWhenOverride(_publicMethods, methodName));
                }
            }

            //静态方法
            List<Type> allBaseClass = new List<Type>();
            Type parentType = clazz;
            for (int i = 0; i < 10; i++)
            {
                if (parentType != null)
                {
                    if (!allBaseClass.Contains(parentType))
                    {
                        allBaseClass.Add(parentType);
                    }
                    parentType = parentType.BaseType;
                }
            }
            for (int i = 0; i < allBaseClass.Count; i++)
            {
                Type _type = allBaseClass[i];
                MethodInfo[] _staticMethods = _type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                for (int j = 0; j < _staticMethods.Length; j++)
                {
                    string methodName = _staticMethods[j].Name;
                    char firstChar = methodName[0];
                    if (firstChar >= 'A' && firstChar <= 'Z')
                    {
                        //只取大写开头的方法
                        if (!staticMethods.ContainsKey(methodName)) staticMethods.Add(methodName, GetOneMethodWhenOverride(_staticMethods, methodName));
                    }
                }
            }

            //写入内容
            sb.Append("\n");
            var clazzName = Replace(clazz.Name);
            if (includeAllBaseClas && clazz.BaseType != null) clazzName = $"{clazzName} : {clazz.BaseType.Name}";
            sb.AppendFormat("---@class {0} @ -------------------------------------\n", clazzName);
            sb.Append("local " + Replace(clazz.Name) + " = {\n");
            foreach (var propertie in properties)
            {
                sb.AppendFormat("    ---@type {0}\n", CTypeToLuaType(propertie.Value));
                sb.AppendFormat("    {0} = nil,\n", propertie.Key);
            }
            sb.Append("}\n");
            foreach (var item in staticProperties)
            {
                sb.AppendFormat("{0}.{1} = nil\n", Replace(clazz.Name), item);
            }
            foreach (var item in publicMethods)
            {
                if (!"Void".Equals(item.Value.returnType.Name)) sb.AppendFormat("---@return {0}\n", CTypeToLuaType(item.Value.returnType));
                for (int j = 0; j < item.Value.parameters.Length; j++)
                {
                    sb.AppendFormat("---@param {0} {1}\n", item.Value.parameters[j].Name, CTypeToLuaType(item.Value.parameters[j].ParameterType));
                }
                sb.AppendFormat("function {0}:{1}(", Replace(clazz.Name), item.Key);
                for (int j = 0; j < item.Value.parameters.Length; j++)
                {
                    if (j > 0) sb.Append(", ");
                    sb.Append(Arg(item.Value.parameters[j].Name));
                }
                sb.Append(") end\n");
            }
            foreach (var item in staticMethods)
            {
                if (!"Void".Equals(item.Value.returnType.Name)) sb.AppendFormat("---@return {0}\n" , CTypeToLuaType(item.Value.returnType));
                for (int j = 0; j < item.Value.parameters.Length; j++)
                {
                    sb.AppendFormat("---@param {0} {1}\n", item.Value.parameters[j].Name, CTypeToLuaType(item.Value.parameters[j].ParameterType));
                }
                sb.AppendFormat( "function {0}.{1}(", Replace(clazz.Name), item.Key);
                for (int j = 0; j < item.Value.parameters.Length; j++)
                {
                    if (j > 0) sb.Append(", ");
                    sb.Append(Arg(item.Value.parameters[j].Name));
                }
                sb.Append(") end\n");
            }
        }

        //有多个重写的方法只取一个参数最多的显示，否则多个同名方法重写EmmyLua反而无法弹出代码提示
        private static Method GetOneMethodWhenOverride(MethodInfo[] publicMethods, string methodName)
        {
            Method method = null;
            int count = -1;
            for (int i = 0; i < publicMethods.Length; i++)
            {
                MethodInfo methodInfo = publicMethods[i];
                if (methodInfo.Name.Equals(methodName))
                {
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    if (parameters.Length > count)
                    {
                        count = parameters.Length;
                        method = new Method(methodInfo.ReturnType, parameters);
                    }
                }
            }
            return method;
        }

        private static string CTypeToLuaType(Type type)
        {
            if (type == typeof(string) || type == typeof(String))
            {
                return "string";
            }
            if (type == typeof(int) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64))
            {
                return "number";
            }
            else if (type == typeof(bool) || type == typeof(Boolean))
            {
                return "boolean";
            }
            return Replace(type.Name);
        }

        private static string Replace(string content)
        {
            return content.Replace("`1", "").Replace("`2", "").Replace("`3", "").Replace("`4", "").Replace("`5", "").Replace("&", "");
        }

        private static string Arg(string arg)
        {
            if ("end".Equals(arg)) return "_end";
            if ("function".Equals(arg)) return "func";
            return arg;
        }
    }
}
