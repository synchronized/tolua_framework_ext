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

namespace LuaInterface.Editor
{
    public static class Generator
    {

        static Generator()
        {
        }

        public static void InitConfig() {
            GetGenConfig(Utils.GetAllTypes());
        }

        static bool IsOverride(MethodBase method)
        {
            var m = method as MethodInfo;
            return m != null && !m.IsConstructor && m.IsVirtual && (m.GetBaseDefinition().DeclaringType != m.DeclaringType);
        }

        static int OverloadCosting(MethodBase mi)
        {
            int costing = 0;

            if (!mi.IsStatic)
            {
                costing++;
            }

            foreach (var paraminfo in mi.GetParameters())
            {
                if ((!paraminfo.ParameterType.IsPrimitive ) && (paraminfo.IsIn || !paraminfo.IsOut))
                {
                    costing++;
                }
            }
            costing = costing * 10000 + (mi.GetParameters().Length + (mi.IsStatic ? 0 : 1));
            return costing;
        }

        static bool isObsolete(MemberInfo mb)
        {
            if (mb == null) return false;
            ObsoleteAttribute oa = GetCustomAttribute(mb, typeof(ObsoleteAttribute)) as ObsoleteAttribute;
            return oa != null;
        }

        static bool isObsolete(Type type)
        {
            if (type == null) return false;
            if (isObsolete(type as MemberInfo))
            {
                return true;
            }
            return (type.DeclaringType != null) ? isObsolete(type.DeclaringType) : false;
        }

        public static bool IsMemberInBlackList(MemberInfo mb)
        {
            if (isDefined(mb, typeof(ToLuaBlackListAttribute))) return true;
            if (mb is FieldInfo && (mb as FieldInfo).FieldType.IsPointer) return true;
            if (mb is PropertyInfo && (mb as PropertyInfo).PropertyType.IsPointer) return true;

            foreach(var filter in memberFilters)
            {
                if (filter(mb))
                {
                    return true;
                }
            }

            foreach (var exclude in BlackList)
            {
                if (mb.DeclaringType.ToString() == exclude[0] && mb.Name == exclude[1])
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsMethodInBlackList(MethodBase mb)
        {
            if (isDefined(mb, typeof(ToLuaBlackListAttribute))) return true;

            //指针目前不支持，先过滤
            if (mb.GetParameters().Any(pInfo => pInfo.ParameterType.IsPointer)) return true;
            if (mb is MethodInfo && (mb as MethodInfo).ReturnType.IsPointer) return true;

            foreach (var filter in memberFilters)
            {
                if (filter(mb))
                {
                    return true;
                }
            }

            foreach (var exclude in BlackList)
            {
                if (mb.DeclaringType.ToString() == exclude[0] && mb.Name == exclude[1])
                {
                    var parameters = mb.GetParameters();
                    if (parameters.Length != exclude.Count - 2)
                    {
                        continue;
                    }
                    bool paramsMatch = true;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType.ToString() != exclude[i + 2])
                        {
                            paramsMatch = false;
                            break;
                        }
                    }
                    if (paramsMatch) return true;
                }
            }
            return false;
        }

        public static bool IsTypeInBlackList(Type type)
        {
            return TypeBlackList.Contains(type);
        }

        class ParameterInfoSimulation
        {
            public string Name;
            public bool IsOut;
            public bool IsIn;
            public Type ParameterType;
            public bool IsParamArray;
        }

        class MethodInfoSimulation
        {
            public Type ReturnType;
            public ParameterInfoSimulation[] ParameterInfos;

            public int HashCode;

            public ParameterInfoSimulation[]  GetParameters()
            {
                return ParameterInfos;
            }

            public Type DeclaringType = null;
            public string DeclaringTypeName = null;
        }

        static MethodInfoSimulation makeMethodInfoSimulation(MethodInfo method)
        {
            int hashCode = method.ReturnType.GetHashCode();

            List<ParameterInfoSimulation> paramsExpect = new List<ParameterInfoSimulation>();

            foreach (var param in method.GetParameters())
            {
                if (param.IsOut)
                {
                    hashCode++;
                }
                hashCode += param.ParameterType.GetHashCode();
                paramsExpect.Add(new ParameterInfoSimulation()
                {
                    Name = param.Name,
                    IsOut = param.IsOut,
                    IsIn = param.IsIn,
                    ParameterType = param.ParameterType,
                    IsParamArray = param.IsDefined(typeof(System.ParamArrayAttribute), false)
                });
            }

            return new MethodInfoSimulation()
            {
                ReturnType = method.ReturnType,
                HashCode = hashCode,
                ParameterInfos = paramsExpect.ToArray(),
                DeclaringType = method.DeclaringType
            };
        }

        static bool hasGenericParameter(Type type)
        {
            if (type.IsByRef || type.IsArray)
            {
                return hasGenericParameter(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (hasGenericParameter(typeArg))
                    {
                        return true;
                    }
                }
                return false;
            }
            return type.IsGenericParameter;
        }


        class MethodInfoSimulationComparer : IEqualityComparer<MethodInfoSimulation>
        {
            public bool Equals(MethodInfoSimulation x, MethodInfoSimulation y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (x == null || y == null)
                {
                    return false;
                }
                if (x.ReturnType != y.ReturnType)
                {
                    return false;
                }
                var xParams = x.GetParameters();
                var yParams = y.GetParameters();
                if (xParams.Length != yParams.Length)
                {
                    return false;
                }

                for (int i = 0; i < xParams.Length; i++)
                {
                    if (xParams[i].ParameterType != yParams[i].ParameterType || xParams[i].IsOut != yParams[i].IsOut)
                    {
                        return false;
                    }
                }

                var lastPos = xParams.Length - 1;
                return lastPos < 0 || xParams[lastPos].IsParamArray == yParams[lastPos].IsParamArray;
            }
            public int GetHashCode(MethodInfoSimulation obj)
            {
                return obj.HashCode;
            }
        }

        static void clear(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                AssetDatabase.DeleteAsset(path.Substring(path.IndexOf("Assets") + "Assets".Length));

                AssetDatabase.Refresh();
            }
        }

        class DelegateByMethodDecComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return Utils.IsParamsMatch(x.GetMethod("Invoke"), y.GetMethod("Invoke"));
            }
            public int GetHashCode(Type obj)
            {
                int hc = 0;
                var method = obj.GetMethod("Invoke");
                hc += method.ReturnType.GetHashCode();
                foreach (var pi in method.GetParameters())
                {
                    hc += pi.ParameterType.GetHashCode();
                }
                return hc;
            }
        }

        static MethodInfo makeGenericMethodIfNeeded(MethodInfo method)
        {
            if (!method.ContainsGenericParameters) return method;

            var genericArguments = method.GetGenericArguments();
            var constraintedArgumentTypes = new Type[genericArguments.Length];
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var argumentType = genericArguments[i];
                var parameterConstraints = argumentType.GetGenericParameterConstraints();
                Type parameterConstraint = parameterConstraints[0];
                foreach(var type in argumentType.GetGenericParameterConstraints())
                {
                    if (parameterConstraint.IsAssignableFrom(type))
                    {
                        parameterConstraint = type;
                    }
                }

                constraintedArgumentTypes[i] = parameterConstraint;
            }
            return method.MakeGenericMethod(constraintedArgumentTypes);
        }

        //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
        public static List<BindType> LuaCallCSharp = null;

        //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
        public static List<Type> CSharpCallLua = null;

        //黑名单
        public static List<List<string>> BlackList = null;

        public static List<Func<MemberInfo, bool>> memberFilters = null;

        public static List<Type> TypeBlackList = null;

        static void AddToList<T>(List<T> list, Func<object> get, object attr) where T: class
        {
            object obj = get();
            if (obj is T)
            {
                list.Add(obj as T);
            }
            else if (obj is IEnumerable<T>)
            {
                list.AddRange(obj as IEnumerable<T>);
            }
            else
            {
                var typeName = typeof(T).Name;
                throw new InvalidOperationException("Only field/property with the type IEnumerable<"+typeName+"> can be marked " + attr.GetType().Name);
            }
        }

        static bool isDefined(MemberInfo test, Type type)
        {
            return test.IsDefined(type, false);
        }

        static object GetCustomAttribute(MemberInfo test, Type type)
        {
            return test.GetCustomAttributes(type, false).FirstOrDefault();
        }

        static void MergeCfg(MemberInfo test, Type cfg_type, Func<object> get_cfg)
        {
            if (isDefined(test, typeof(ToLuaLuaCallCSharpAttribute)))
            {
                object ccla = GetCustomAttribute(test, typeof(ToLuaLuaCallCSharpAttribute));
                AddToList(LuaCallCSharp, get_cfg, ccla);
            }
            if (isDefined(test, typeof(ToLuaCSharpCallLuaAttribute)))
            {
                object ccla = GetCustomAttribute(test, typeof(ToLuaCSharpCallLuaAttribute));
                AddToList(CSharpCallLua, get_cfg, ccla);
            }
            if (isDefined(test, typeof(ToLuaBlackListAttribute)))
            {
                if (typeof(List<List<string>>).IsAssignableFrom(cfg_type))
                {
                    BlackList.AddRange(get_cfg() as List<List<string>>);
                }
                if (typeof(Func<MemberInfo, bool>).IsAssignableFrom(cfg_type))
                {
                    memberFilters.Add(get_cfg() as Func<MemberInfo, bool>);
                }
            }
            if (isDefined(test, typeof(ToLuaTypeBlackListAttribute)))
            {
                object ccla = GetCustomAttribute(test, typeof(ToLuaTypeBlackListAttribute));
                AddToList(TypeBlackList, get_cfg, ccla);
            }
        }

        static bool IsPublic(Type type)
        {
            if (type.IsPublic || type.IsNestedPublic)
            {
                if (type.DeclaringType != null)
                {
                    return IsPublic(type.DeclaringType);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        static bool typeHasEditorRef(Type type)
        {
            if (type.Namespace != null && (type.Namespace == "UnityEditor" || type.Namespace.StartsWith("UnityEditor.")))
            {
                return true;
            }
            if (type.IsNested)
            {
                return typeHasEditorRef(type.DeclaringType);
            }
            if (type.IsByRef || type.IsArray)
            {
                return typeHasEditorRef(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (typeArg.IsGenericParameter) {
                        //skip unsigned type parameter
                        continue;
                    }
                    if (typeHasEditorRef(typeArg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static bool delegateHasEditorRef(Type delegateType)
        {
            if (typeHasEditorRef(delegateType)) return true;
            var method = delegateType.GetMethod("Invoke");
            if (method == null)
            {
                return false;
            }
            if (typeHasEditorRef(method.ReturnType)) return true;
            return method.GetParameters().Any(pinfo => typeHasEditorRef(pinfo.ParameterType));
        }

        public static IEnumerable<Type> LuaCallCSharpToCSharpCallLua(IEnumerable<BindType> lua_call_csharp)
        {
            var delegate_types = new List<Type>();
            var flag = BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.IgnoreCase;

            //添加属性
            foreach (var field in lua_call_csharp.SelectMany(type => type.type.GetFields(flag)))
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    delegate_types.Add(field.FieldType);
                }
            }

            foreach (var prop in lua_call_csharp.SelectMany(type => type.type.GetProperties(flag)))
            {
                if (typeof(Delegate).IsAssignableFrom(prop.PropertyType))
                {
                    delegate_types.Add(prop.PropertyType);
                }
            }

            foreach (var method in lua_call_csharp.SelectMany(type => type.type.GetMethods(flag)))
            {
                if (typeof(Delegate).IsAssignableFrom(method.ReturnType))
                {
                    delegate_types.Add(method.ReturnType);
                }
                foreach (var param in method.GetParameters())
                {
                    var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                    if (typeof(Delegate).IsAssignableFrom(paramType))
                    {
                        delegate_types.Add(paramType);
                    }
                }
            }
            return delegate_types.Where(t => t.BaseType == typeof(MulticastDelegate) && !hasGenericParameter(t) && !delegateHasEditorRef(t)).Distinct().ToList();
        }

        public static void GetGenConfig(IEnumerable<Type> check_types)
        {
            LuaCallCSharp = new List<BindType>();
            CSharpCallLua = new List<Type>();
            BlackList = new List<List<string>>();
            memberFilters = new List<Func<MemberInfo, bool>>();
            TypeBlackList = new();

            foreach (var t in check_types)
            {
                MergeCfg(t, null, () => t);

                if (!t.IsAbstract || !t.IsSealed) continue;

                var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    MergeCfg(field, field.FieldType, () => field.GetValue(null));
                }

                var props = t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    MergeCfg(prop, prop.PropertyType, () => prop.GetValue(null, null));
                }
            }

            var DelegateType = typeof(System.Delegate);
            LuaCallCSharp = LuaCallCSharp.Distinct()
                .Where(type => IsPublic(type.type) && !isObsolete(type.type) && !type.type.IsGenericTypeDefinition)
                .Where(type => DelegateType == type.type || !DelegateType.IsAssignableFrom(type.type))
                .Where(type => DelegateType == type.type || !DelegateType.IsAssignableFrom(type.type))
                .Where(type => !type.type.Name.Contains("<"))
                .Where(type => !IsTypeInBlackList(type.type)) //去除黑名单中的类型
                .Select(type => checkBaseType(type))          //整理基类
                .OrderBy(type => type.type.FullName)
                .ToList();

            CSharpCallLua.AddRange(LuaCallCSharpToCSharpCallLua(LuaCallCSharp));
            CSharpCallLua = CSharpCallLua.Distinct()
                .Where(type => IsPublic(type) && !isObsolete(type) && !type.IsGenericTypeDefinition)
                .Where(type => type != typeof(Delegate) && type != typeof(MulticastDelegate))
                .Where(type => !IsTypeInBlackList(type)) //去除黑名单中的类型
                .OrderBy(type => type.FullName)
                .ToList();

        }

        static BindType checkBaseType(BindType bt)
        {
            Type t = bt.baseType;
            while (t != null) {
                if (t.IsInterface) {
                    Debugger.LogWarning("{0} has a base type {1} is Interface, use SetBaseType to jump it", bt.name, t.FullName);
                    bt.baseType = t.BaseType;
                    t = bt.baseType;
                    continue;
                }
                else if (Generator.IsTypeInBlackList(t)) {
                    Debugger.LogWarning("{0} has a base type {1} is a drop type", bt.name, t.FullName);
                    bt.baseType = t.BaseType;
                    t = bt.baseType;
                    continue;
                }
                break;
            }
            return bt;
        }

        /*
        [MenuItem("XLua/Generate Code", false, 1)]
        public static void GenAll()
        {
            var start = DateTime.Now;
            Directory.CreateDirectory(GeneratorConfig.common_path);
            GetGenConfig(Utils.GetAllTypes());
            GenDelegateBridges(Utils.GetAllTypes(false));
            GenEnumWraps();
            GenCodeForClass();
            GenLuaRegister();
            Debug.Log("finished! use " + (DateTime.Now - start).TotalMilliseconds + " ms");
            AssetDatabase.Refresh();
        }

        [MenuItem("XLua/Clear Generated Code", false, 2)]
        public static void ClearAll()
        {
            clear(GeneratorConfig.common_path);
        }
        */
    }
}
