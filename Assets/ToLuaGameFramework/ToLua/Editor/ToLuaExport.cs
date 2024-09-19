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
using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;

using UnityEngine;

namespace LuaInterface.Editor
{

    public class BindType
    {
        public string name;                 //类名称
        public Type type;
        public bool IsStatic;
        public bool IsDynamic;
        public bool IsOut;
        public string wrapName = "";        //产生的wrap文件名字
        public string libName = "";         //注册到lua的名字
        public Type baseType = null;
        public string nameSpace = null;     //注册到lua的table层级

        public List<Type> extendList = new List<Type>();

        public BindType(Type t)
        {
            if (typeof(System.MulticastDelegate).IsAssignableFrom(t))
            {
                throw new NotSupportedException(string.Format("\nDon't export Delegate {0} as a class, register it in customTypeList", LuaMisc.GetTypeName(t)));
            }

            //if (IsObsolete(t))
            //{
            //    throw new Exception(string.Format("\n{0} is obsolete, don't export it!", LuaMisc.GetTypeName(t)));
            //}

            type = t;
            nameSpace = ToLuaExport.GetNameSpace(t, out libName);
            name = ToLuaExport.CombineTypeStr(nameSpace, libName);
            libName = ToLuaExport.ConvertToLibSign(libName);

            if (name == "object")
            {
                wrapName = "System_Object";
                name = "System.Object";
            }
            else if (name == "string")
            {
                wrapName = "System_String";
                name = "System.String";
            }
            else
            {
                wrapName = name.Replace('.', '_');
                wrapName = ToLuaExport.ConvertToLibSign(wrapName);
            }

            if (type.IsAbstract && type.IsSealed)
            {
                IsStatic = true;
            }

            baseType = LuaMisc.GetExportBaseType(type);
        }

        public BindType SetBaseType(Type t)
        {
            baseType = t;
            return this;
        }

        public BindType AddExtendType(Type t)
        {
            if (!extendList.Contains(t))
            {
                extendList.Add(t);
            }

            return this;
        }

        public BindType SetWrapName(string str)
        {
            wrapName = str;
            return this;
        }

        public BindType SetLibName(string str)
        {
            libName = str;
            return this;
        }

        public BindType SetNameSpace(string space)
        {
            nameSpace = space;
            return this;
        }

        public BindType SetStatic(bool isStatic)
        {
            IsStatic = isStatic;
            return this;
        }

        // public static List<Type> CustomSettings.dynamicList = new List<Type>()
        public BindType SetDynamic(bool isDynamic)
        {
            IsDynamic = isDynamic;
            return this;
        }


        //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
        //使用方法参见例子14
        //public static List<Type> CustomSettings.outList = new List<Type>()
        public BindType SetOut(bool isOut)
        {
            IsOut = isOut;
            return this;
        }
    }

    public class DelegateType
    {
        public string name;
        public Type type;

        public string strType = "";

        public DelegateType(Type t)
        {
            type = t;
            strType = ToLuaExport.GetTypeStr(null, t);
            name = ToLuaExport.ConvertToLibSign(strType);
        }
    }

    public enum MetaOp
    {
        None = 0,
        Add = 1,
        Sub = 2,
        Mul = 4,
        Div = 8,
        Eq = 16,
        Neg = 32,
        ToStr = 64,
        Le = 128,
        Lt = 256,
        ALL = Add | Sub | Mul | Div | Eq | Neg | ToStr,
    }

    public enum ObjAmbig
    {
        None = 0,
        U3dObj = 1,
        NetObj = 2,
        All = 3
    }


    public static class ToLuaExport
    {
        //public static string className = string.Empty;
        //public static Type type = null;
        //public static Type baseType = null;

        //public static bool isStaticClass = true;
        public static string wrapFileExtions = "Wrap.cs";

        static HashSet<string> usingList = new HashSet<string>();
        static MetaOp op = MetaOp.None;
        static StringBuilder sb = null;
        static List<_MethodBase> methods = new List<_MethodBase>();
        static Dictionary<string, int> nameCounter = new Dictionary<string, int>();
        static FieldInfo[] fields = null;
        static PropertyInfo[] props = null;
        static List<PropertyInfo> propList = new List<PropertyInfo>();  //非静态属性
        static List<PropertyInfo> allProps = new List<PropertyInfo>();
        static EventInfo[] events = null;
        static List<EventInfo> eventList = new List<EventInfo>();
        static List<_MethodBase> ctorList = new List<_MethodBase>();
        static List<ConstructorInfo> ctorExtList = new List<ConstructorInfo>();
        static List<_MethodBase> getItems = new List<_MethodBase>();   //特殊属性
        static List<_MethodBase> setItems = new List<_MethodBase>();

        static BindingFlags binding = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

        static ObjAmbig ambig = ObjAmbig.NetObj;
        //wrapClaaName + "Wrap" = 导出文件名，导出类名
        //public static string wrapClassName = "";

        //public static string libClassName = "";
        public static string extendName = "";
        public static Type extendType = null;

        public static HashSet<Type> eventSet = new HashSet<Type>();
        //public static List<Type> extendList = new List<Type>();


        class _MethodBase
        {
            public bool IsStatic
            {
                get
                {
                    return method.IsStatic;
                }
            }

            public bool IsConstructor
            {
                get
                {
                    return method.IsConstructor;
                }
            }

            public string Name
            {
                get
                {
                    return method.Name;
                }
            }

            public MethodBase Method
            {
                get
                {
                    return method;
                }
            }

            public bool IsGenericMethod
            {
                get
                {
                    return method.IsGenericMethod;
                }
            }


            MethodBase method;
            ParameterInfo[] args;

            public _MethodBase(MethodBase m, int argCount = -1)
            {
                method = m;
                ParameterInfo[] infos = m.GetParameters();
                argCount = argCount != -1 ? argCount : infos.Length;
                args = new ParameterInfo[argCount];
                Array.Copy(infos, args, argCount);
            }

            public _MethodBase(MethodBase m, bool beExtend): this(m)
            {
                BeExtend = beExtend;
            }

            public ParameterInfo[] GetParameters()
            {
                return args;
            }

            public int GetParamsCount()
            {
                int c = method.IsStatic ? 0 : 1;
                return args.Length + c;
            }

            public int GetEqualParamsCount(BindType bType, _MethodBase b)
            {
                var type = bType.type;
                int count = 0;
                List<Type> list1 = new List<Type>();
                List<Type> list2 = new List<Type>();

                if (!IsStatic)
                {
                    list1.Add(type);
                }

                if (!b.IsStatic)
                {
                    list2.Add(type);
                }

                for (int i = 0; i < args.Length; i++)
                {
                    list1.Add(GetParameterType(bType, args[i]));
                }

                ParameterInfo[] p = b.args;

                for (int i = 0; i < p.Length; i++)
                {
                    list2.Add(GetParameterType(bType, p[i]));
                }

                for (int i = 0; i < list1.Count; i++)
                {
                    if (list1[i] != list2[i])
                    {
                        break;
                    }

                    ++count;
                }

                return count;
            }

            public string GenParamTypes(BindType bType, int offset = 0)
            {
                var type = bType.type;
                StringBuilder sb = new StringBuilder();
                List<Type> list = new List<Type>();

                if (!method.IsStatic)
                {
                    list.Add(type);
                }

                for (int i = 0; i < args.Length; i++)
                {
                    if (IsParams(args[i]))
                    {
                        continue;
                    }

                    if (args[i].ParameterType.IsByRef && (args[i].Attributes & ParameterAttributes.Out) != ParameterAttributes.None)
                    {
                        Type genericClass = typeof(LuaOut<>);
                        Type t = genericClass.MakeGenericType(args[i].ParameterType.GetElementType());
                        list.Add(t);
                    }
                    else
                    {
                        list.Add(GetGenericBaseType(method, args[i].ParameterType));
                    }
                }

                for (int i = offset; i < list.Count - 1; i++)
                {
                    sb.Append(GetTypeOf(bType, list[i], ", "));
                }

                if (list.Count > 0)
                {
                    sb.Append(GetTypeOf(bType, list[list.Count - 1], ""));
                }

                return sb.ToString();
            }

            public bool HasSetIndex(BindType bType)
            {
                var type = bType.type;
                if (method.Name == "set_Item")
                {
                    return true;
                }

                object[] attrs = type.GetCustomAttributes(true);

                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i] is DefaultMemberAttribute)
                    {
                        return method.Name == "set_ItemOf";
                    }
                }

                return false;
            }

            public bool HasGetIndex(BindType bType)
            {
                var type = bType.type;
                if (method.Name == "get_Item")
                {
                    return true;
                }

                object[] attrs = type.GetCustomAttributes(true);

                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i] is DefaultMemberAttribute)
                    {
                        return method.Name == "get_ItemOf";
                    }
                }

                return false;
            }

            public Type GetReturnType()
            {
                MethodInfo m = method as MethodInfo;

                if (m != null)
                {
                    return m.ReturnType;
                }

                return null;
            }

            public string GetTotalName(BindType bType)
            {
                string[] ss = new string[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    ss[i] = GetTypeStr(bType, args[i].GetType());
                }

                if (!ToLuaExport.IsGenericMethod(method))
                {
                    return Name + "(" + string.Join(",", ss) + ")";
                }
                else
                {
                    Type[] gts = method.GetGenericArguments();
                    string[] ts = new string[gts.Length];

                    for (int i = 0; i < gts.Length; i++)
                    {
                        ts[i] = GetTypeStr(bType, gts[i]);
                    }

                    return Name + "<" + string.Join(",", ts) + ">" + "(" + string.Join(",", ss) + ")";
                }
            }

            public bool BeExtend = false;

            public int ProcessParams(BindType bType, int tab, bool beConstruct, int checkTypePos)
            {
                ParameterInfo[] paramInfos = args;

                if (BeExtend)
                {
                    ParameterInfo[] pt = new ParameterInfo[paramInfos.Length - 1];
                    Array.Copy(paramInfos, 1, pt, 0, pt.Length);
                    paramInfos = pt;
                }

                int count = paramInfos.Length;
                string head = string.Empty;
                PropertyInfo pi = null;
                int methodType = GetMethodType(method, out pi);
                int offset = ((method.IsStatic && !BeExtend) || beConstruct) ? 1 : 2;

                if (method.Name == "op_Equality")
                {
                    checkTypePos = -1;
                }

                for (int i = 0; i < tab; i++)
                {
                    head += "\t";
                }

                var type = bType.type;
                var className = bType.name;
                if ((!method.IsStatic && !beConstruct) || BeExtend)
                {
                    if (checkTypePos > 0)
                    {
                        CheckObject(head, type, className, 1);
                    }
                    else
                    {
                        if (method.Name == "Equals")
                        {
                            if (!bType.type.IsValueType && checkTypePos > 0)
                            {
                                CheckObject(head, type, className, 1);
                            }
                            else
                            {
                                sb.AppendLineFormat("{0}{1} obj = ({1})ToLua.ToObject(L, 1);", head, className);
                            }
                        }
                        else if (checkTypePos > 0)// && methodType == 0)
                        {
                            CheckObject(head, type, className, 1);
                        }
                        else
                        {
                            ToObject(head, type, className, 1);
                        }
                    }
                }

                StringBuilder sbArgs = new StringBuilder();
                List<string> refList = new List<string>();
                List<Type> refTypes = new List<Type>();
                checkTypePos = checkTypePos - offset + 1;

                for (int j = 0; j < count; j++)
                {
                    ParameterInfo param = paramInfos[j];
                    string arg = "arg" + j;
                    bool beOutArg = param.ParameterType.IsByRef && ((param.Attributes & ParameterAttributes.Out) != ParameterAttributes.None);
                    bool beParams = IsParams(param);
                    Type t = GetGenericBaseType(method, param.ParameterType);
                    ProcessArg(bType, t, head, arg, offset + j, j >= checkTypePos, beParams, beOutArg);
                }

                for (int j = 0; j < count; j++)
                {
                    ParameterInfo param = paramInfos[j];

                    if (!param.ParameterType.IsByRef || ((param.Attributes & ParameterAttributes.In) != ParameterAttributes.None))
                    {
                        sbArgs.Append("arg");
                    }
                    else
                    {
                        if ((param.Attributes & ParameterAttributes.Out) != ParameterAttributes.None)
                        {
                            sbArgs.Append("out arg");
                        }
                        else if (param.Attributes == ParameterAttributes.In)
                        {
                            sbArgs.Append("arg");
                        }
                        else
                        {
                            sbArgs.Append("ref arg");
                        }

                        refList.Add("arg" + j);
                        refTypes.Add(GetRefBaseType(param.ParameterType));
                    }

                    sbArgs.Append(j);

                    if (j != count - 1)
                    {
                        sbArgs.Append(", ");
                    }
                }

                if (beConstruct)
                {
                    sb.AppendLineFormat("{2}{0} obj = new {0}({1});", className, sbArgs.ToString(), head);
                    string str = GetPushFunction(type);
                    sb.AppendLineFormat("{0}ToLua.{1}(L, obj);", head, str);

                    for (int i = 0; i < refList.Count; i++)
                    {
                        GenPushStr(refTypes[i], refList[i], head);
                    }

                    return refList.Count + 1;
                }

                string obj = (method.IsStatic && !BeExtend) ? className : "obj";
                Type retType = GetReturnType();

                if (retType == typeof(void))
                {
                    if (HasSetIndex(bType))
                    {
                        if (methodType == 2)
                        {
                            string str = sbArgs.ToString();
                            string[] ss = str.Split(',');
                            str = string.Join(",", ss, 0, ss.Length - 1);

                            sb.AppendLineFormat("{0}{1}[{2}] ={3};", head, obj, str, ss[ss.Length - 1]);
                        }
                        else if (methodType == 1)
                        {
                            sb.AppendLineFormat("{0}{1}.Item = arg0;", head, obj, pi.Name);
                        }
                        else
                        {
                            sb.AppendLineFormat("{0}{1}.{2}({3});", head, obj, method.Name, sbArgs.ToString());
                        }
                    }
                    else if (methodType == 1)
                    {
                        sb.AppendLineFormat("{0}{1}.{2} = arg0;", head, obj, pi.Name);
                    }
                    else
                    {
                        sb.AppendLineFormat("{3}{0}.{1}({2});", obj, method.Name, sbArgs.ToString(), head);
                    }
                }
                else
                {
                    Type genericType = GetGenericBaseType(method, retType);
                    string ret = GetTypeStr(bType, genericType);

                    if (method.Name.StartsWith("op_"))
                    {
                        CallOpFunction(method.Name, tab, ret);
                    }
                    else if (HasGetIndex(bType))
                    {
                        if (methodType == 2)
                        {
                            sb.AppendLineFormat("{0}{1} o = {2}[{3}];", head, ret, obj, sbArgs.ToString());
                        }
                        else if (methodType == 1)
                        {
                            sb.AppendLineFormat("{0}{1} o = {2}.Item;", head, ret, obj);
                        }
                        else
                        {
                            sb.AppendLineFormat("{0}{1} o = {2}.{3}({4});", head, ret, obj, method.Name, sbArgs.ToString());
                        }
                    }
                    else if (method.Name == "Equals")
                    {
                        if (type.IsValueType || method.GetParameters().Length > 1)
                        {
                            sb.AppendLineFormat("{0}{1} o = obj.Equals({2});", head, ret, sbArgs.ToString());
                        }
                        else
                        {
                            sb.AppendLineFormat("{0}{1} o = obj != null ? obj.Equals({2}) : arg0 == null;", head, ret, sbArgs.ToString());
                        }
                    }
                    else if (methodType == 1)
                    {
                        sb.AppendLineFormat("{0}{1} o = {2}.{3};", head, ret, obj, pi.Name);
                    }
                    else
                    {
                        sb.AppendLineFormat("{0}{1} o = {2}.{3}({4});", head, ret, obj, method.Name, sbArgs.ToString());
                    }

                    bool isbuffer = IsByteBuffer();
                    GenPushStr(retType, "o", head, isbuffer);
                }

                for (int i = 0; i < refList.Count; i++)
                {
                    if (refTypes[i] == typeof(RaycastHit) && method.Name == "Raycast" && (type == typeof(Physics) || type == typeof(Collider)))
                    {
                        sb.AppendLineFormat("{0}if (o) ToLua.Push(L, {1}); else LuaDLL.lua_pushnil(L);", head, refList[i]);
                    }
                    else
                    {
                        GenPushStr(refTypes[i], refList[i], head);
                    }
                }

                if (!method.IsStatic && type.IsValueType && method.Name != "ToString")
                {
                    sb.Append(head + "ToLua.SetBack(L, 1, obj);\r\n");
                }

                return refList.Count;
            }

            bool IsByteBuffer()
            {
                return method.IsDefined(typeof(LuaByteBufferAttribute), true);
            }
        }

        public static bool IsMemberFilter(BindType bType, MemberInfo mi)
        {
            var type = bType.type;
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(Dictionary<,>) && mi.Name == "Remove")
                {
                    MethodBase mb = (MethodBase)mi;
                    return mb.GetParameters().Length == 2;
                }
                if (genericType == typeof(HashSet<>) && mi.Name == "TryGetValue")
                {
                    MethodBase mb = (MethodBase)mi;
                    return mb.GetParameters().Length == 2;
                }

            }

            return Generator.IsMemberInBlackList(mi);
        }

        static ToLuaExport()
        {
            Debugger.useLog = true;
        }

        public static void Clear()
        {
            usingList.Clear();
            op = MetaOp.None;
            sb = new StringBuilder();
            fields = null;
            props = null;
            methods.Clear();
            allProps.Clear();
            propList.Clear();
            eventList.Clear();
            ctorList.Clear();
            ctorExtList.Clear();
            ambig = ObjAmbig.NetObj;
            extendName = "";
            eventSet.Clear();
            extendType = null;
            nameCounter.Clear();
            events = null;
            getItems.Clear();
            setItems.Clear();
        }

        private static MetaOp GetOp(BindType bType, string name)
        {
            var isStatic = bType.IsStatic;
            if (name == "op_Addition")
            {
                return MetaOp.Add;
            }
            else if (name == "op_Subtraction")
            {
                return MetaOp.Sub;
            }
            else if (name == "op_Equality")
            {
                return MetaOp.Eq;
            }
            else if (name == "op_Multiply")
            {
                return MetaOp.Mul;
            }
            else if (name == "op_Division")
            {
                return MetaOp.Div;
            }
            else if (name == "op_UnaryNegation")
            {
                return MetaOp.Neg;
            }
            else if (name == "ToString" && !isStatic)
            {
                return MetaOp.ToStr;
            }
            else if(name == "op_LessThanOrEqual")
            {
                return MetaOp.Le;
            }
            else if(name == "op_GreaterThanOrEqual")
            {
                return MetaOp.Lt;
            }

            return MetaOp.None;
        }

        //操作符函数无法通过继承metatable实现
        static void GenBaseOpFunction(BindType bType, List<_MethodBase> list)
        {
            var type = bType.type;
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                MethodInfo[] methods = baseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

                for (int i = 0; i < methods.Length; i++)
                {
                    MetaOp baseOp = GetOp(bType, methods[i].Name);

                    if (baseOp != MetaOp.None && (op & baseOp) == 0)
                    {
                        if (baseOp != MetaOp.ToStr)
                        {
                            list.Add(new _MethodBase(methods[i]));
                        }

                        op |= baseOp;
                    }
                }

                baseType = baseType.BaseType;
            }
        }

        public static void GenerateWrap(BindType bType, string saveDir)
        {
            var type = bType.type;
            var className = bType.name;
    #if !EXPORT_INTERFACE
            Type iterType = typeof(System.Collections.IEnumerator);

            if (type.IsInterface && type != iterType)
            {
                return;
            }
    #endif

            //Debugger.Log("Begin Generate lua Wrap for class {0}", className);
            sb = new StringBuilder();
            usingList.Add("System");

            if (bType.wrapName == "")
            {
                bType.wrapName = className;
            }

            if (type.IsEnum)
            {
                BeginCodeGen(bType);
                GenEnum(bType);
                EndCodeGen(bType, saveDir);
                return;
            }

            InitMethods(bType);
            InitPropertyList(bType);
            InitCtorList(bType);

            BeginCodeGen(bType);

            GenRegisterFunction(bType);
            GenConstructFunction(bType);
            GenItemPropertyFunction(bType);
            GenFunctions(bType);
            //GenToStringFunction();
            GenIndexFunc(bType);
            GenNewIndexFunc(bType);
            GenOutFunction(bType);
            GenEventFunctions(bType);

            EndCodeGen(bType, saveDir);
        }

        //是否为委托类型，没处理废弃
        public static bool IsDelegateType(Type t)
        {
            return typeof(System.MulticastDelegate).IsAssignableFrom(t) && t != typeof(System.MulticastDelegate);
        }

        static void BeginCodeGen(BindType bType)
        {
            var wrapClassName = bType.wrapName;

            sb.AppendLineEx("namespace LuaInterface.ObjectWrap");
            sb.AppendLineEx("{");

            sb.AppendLineFormat("public class {0}Wrap", wrapClassName);
            sb.AppendLineEx("{");
        }

        static void EndCodeGen(BindType bType, string saveDir)
        {
            var wrapClassName = bType.wrapName;
            sb.AppendLineEx("} //end class");

            sb.AppendLineEx("} //end namespace LuaInterface.ObjectWrap");
            SaveFile($"{saveDir}/{wrapClassName}Wrap.cs");
        }

        static void InitMethods(BindType bType)
        {
            var type = bType.type;
            var baseType = bType.baseType;
            var isStaticClass = bType.IsStatic;

            var lbinding = binding;
            if (baseType != null || isStaticClass)
            {
                lbinding |= BindingFlags.DeclaredOnly;
            }

            List<_MethodBase> list = type.GetMethods(BindingFlags.Instance | lbinding)
                    //去掉操作符函数
                    .Where(m => {
                        var name = m.Name;
                        return !name.StartsWith("op_") && !name.StartsWith("add_") && !name.StartsWith("remove_") || IsNeedOp(bType, name);
                    })
                    //扔掉 unity3d 废弃的函数
                    .Where(m => !IsObsolete(bType, m))
                    .Select(m => new _MethodBase(m))
                    .ToList();

            List<PropertyInfo> ps = type.GetProperties().ToList();

            list.RemoveAll((m) => {
                var _m = m.Method;
                return ps.Find(p => IsObsolete(bType, p) && (_m == p.GetGetMethod() || _m == p.GetSetMethod())) != null;
            });

            for (int i = 0; i < ps.Count; i++)
            {
                MethodInfo md = ps[i].GetGetMethod();

                if (md != null)
                {
                    int index = list.FindIndex((m) => { return m.Method == md; });

                    if (index >= 0)
                    {
                        if (md.GetParameters().Length == 0)
                        {
                            list.RemoveAt(index);
                        }
                        else if (list[index].HasGetIndex(bType))
                        {
                            getItems.Add(list[index]);
                        }
                    }
                }

                md = ps[i].GetSetMethod();

                if (md != null)
                {
                    int index = list.FindIndex((m) => { return m.Method == md; });

                    if (index >= 0)
                    {
                        if (md.GetParameters().Length == 1)
                        {
                            list.RemoveAt(index);
                        }
                        else if (list[index].HasSetIndex(bType))
                        {
                            setItems.Add(list[index]);
                        }
                    }
                }
            }

            if (!isStaticClass)
            {
                //将基类中与子类同名函数
                var methodFlag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;
                List<MethodInfo> baseList = type.GetMethods(methodFlag)
                        .Where(m => m.DeclaringType != bType.type)
                        .ToList();

                var addList = baseList
                        .Where(_mb => list.Find((p) => {
                            return p.Name == _mb.Name;
                        }) != null)
                        .Distinct()
                        .ToList();


                list.AddRange(addList.Select(mb => new _MethodBase(mb)));
            }

            var itemEventSet = list.Where(m => !m.IsGenericMethod)
                    .SelectMany(m => {
                        return m.GetParameters()
                            .Where(mp => IsDelegateType(mp.ParameterType))
                            .Select(mp => mp.ParameterType);
                    });
            eventSet.AddRange(itemEventSet);

            ProcessExtends(bType, list);
            GenBaseOpFunction(bType, list);

            for (int i = 0; i < list.Count; i++)
            {
                int count = GetDefalutParamCount(list[i].Method);
                int length = list[i].GetParameters().Length;

                for (int j = 0; j < count + 1; j++)
                {
                    _MethodBase r = new _MethodBase(list[i].Method, length - j);
                    r.BeExtend = list[i].BeExtend;
                    methods.Add(r);
                }
            }
        }

        static void InitPropertyList(BindType bType)
        {
            var type = bType.type;
            props = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | binding);
            propList.AddRange(type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase));
            fields = type.GetFields(BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance | binding);
            events = type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            eventList.AddRange(type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public));

            List<FieldInfo> fieldList = new List<FieldInfo>();
            fieldList.AddRange(fields);

            for (int i = fieldList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(bType, fieldList[i]))
                {
                    fieldList.RemoveAt(i);
                }
                else if (IsDelegateType(fieldList[i].FieldType))
                {
                    eventSet.Add(fieldList[i].FieldType);
                }
            }

            fields = fieldList.ToArray();

            List<PropertyInfo> piList = new List<PropertyInfo>();
            piList.AddRange(props);

            for (int i = piList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(bType, piList[i]))
                {
                    piList.RemoveAt(i);
                }
                else if (piList[i].Name == "Item" && IsItemThis(piList[i]))
                {
                    piList.RemoveAt(i);
                }
                else if (piList[i].GetGetMethod() != null && HasGetIndex(bType, piList[i].GetGetMethod()))
                {
                    piList.RemoveAt(i);
                }
                else if (piList[i].GetSetMethod() != null && HasSetIndex(bType, piList[i].GetSetMethod()))
                {
                    piList.RemoveAt(i);
                }
                else if (IsDelegateType(piList[i].PropertyType))
                {
                    eventSet.Add(piList[i].PropertyType);
                }
            }

            props = piList.ToArray();

            for (int i = propList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(bType, propList[i]))
                {
                    propList.RemoveAt(i);
                }
            }

            allProps.AddRange(props);
            allProps.AddRange(propList);

            List<EventInfo> evList = new List<EventInfo>();
            evList.AddRange(events);

            for (int i = evList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(bType, evList[i]))
                {
                    evList.RemoveAt(i);
                }
                else if (IsDelegateType(evList[i].EventHandlerType))
                {
                    eventSet.Add(evList[i].EventHandlerType);
                }
            }

            events = evList.ToArray();

            for (int i = eventList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(bType, eventList[i]))
                {
                    eventList.RemoveAt(i);
                }
            }
        }

        static void SaveFile(string file)
        {
            using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
            {
                StringBuilder usb = new StringBuilder();
                usb.AppendLineEx("//this source code was auto-generated by tolua#, do not modify it");

                foreach (string str in usingList)
                {
                    usb.AppendLineFormat("using {0};", str);
                }

                usb.AppendLineEx("using LuaInterface;");

                if (ambig == ObjAmbig.All)
                {
                    usb.AppendLineEx("using Object = UnityEngine.Object;");
                }

                usb.AppendLineEx();

                textWriter.Write(usb.ToString());
                textWriter.Write(sb.ToString());
                textWriter.Flush();
                textWriter.Close();
            }
        }

        static string GetMethodName(MethodBase md)
        {
            if (md.Name.StartsWith("op_"))
            {
                return md.Name;
            }

            object[] attrs = md.GetCustomAttributes(true);

            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is LuaRenameAttribute)
                {
                    LuaRenameAttribute attr = attrs[i] as LuaRenameAttribute;
                    return attr.Name;
                }
            }

            return md.Name;
        }

        static bool HasGetIndex(BindType bType, MemberInfo md)
        {
            var type = bType.type;
            if (md.Name == "get_Item")
            {
                return true;
            }

            object[] attrs = type.GetCustomAttributes(true);

            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is DefaultMemberAttribute)
                {
                    return md.Name == "get_ItemOf";
                }
            }

            return false;
        }

        static bool HasSetIndex(BindType bType, MemberInfo md)
        {
            var type = bType.type;
            if (md.Name == "set_Item")
            {
                return true;
            }

            object[] attrs = type.GetCustomAttributes(true);

            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is DefaultMemberAttribute)
                {
                    return md.Name == "set_ItemOf";
                }
            }

            return false;
        }

        static bool IsThisArray(MethodBase md, int count)
        {
            ParameterInfo[] pis = md.GetParameters();

            if (pis.Length != count)
            {
                return false;
            }

            if (pis[0].ParameterType == typeof(int))
            {
                return true;
            }

            return false;
        }

        static void GenRegisterFuncItems(BindType bType)
        {
            var type = bType.type;
            var wrapClassName = bType.wrapName;


            if (type.IsArray || type == typeof(Array)) {
                sb.AppendLineFormat("\t\tL.RegFunction(\".geti\", new LuaCSFunction(StaticLuaCallbacks.Array_GetItem));");
                sb.AppendLineFormat("\t\tL.RegFunction(\".seti\", new LuaCSFunction(StaticLuaCallbacks.Array_SetItem));");
                sb.AppendLineFormat("\t\tL.RegFunction(\"ToTable\", new LuaCSFunction(StaticLuaCallbacks.Array_ToTable));");
            } else if (typeof(IList).IsAssignableFrom(type)) {
                sb.AppendLineFormat("\t\tL.RegFunction(\".geti\", new LuaCSFunction(StaticLuaCallbacks.List_GetItem));");
                sb.AppendLineFormat("\t\tL.RegFunction(\".seti\", new LuaCSFunction(StaticLuaCallbacks.List_SetItem));");
            }

            //bool isList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            //注册库函数
            for (int i = 0; i < methods.Count; i++)
            {
                _MethodBase m = methods[i];
                int count = 1;

                if (IsGenericMethod(m.Method))
                {
                    continue;
                }

                string name = GetMethodName(m.Method);

                if (!nameCounter.TryGetValue(name, out count))
                {
                    if (name == "get_Item" && IsThisArray(m.Method, 1))
                    {
                        sb.AppendLineFormat("\t\tL.RegFunction(\"{0}\", new LuaCSFunction(get_Item));", ".geti");
                    }
                    else if (name == "set_Item" && IsThisArray(m.Method, 2))
                    {
                        sb.AppendLineFormat("\t\tL.RegFunction(\"{0}\", new LuaCSFunction(set_Item));", ".seti");
                    }

                    if (!name.StartsWith("op_"))
                    {
                        sb.AppendLineFormat("\t\tL.RegFunction(\"{0}\", new LuaCSFunction({1}));", name, name == "Register" ? "_Register" : name);
                    }

                    nameCounter[name] = 1;
                }
                else
                {
                    nameCounter[name] = count + 1;
                }
            }

            if (ctorList.Count > 0 || type.IsValueType || ctorExtList.Count > 0)
            {
                sb.AppendLineFormat("\t\tL.RegFunction(\"New\", new LuaCSFunction(_Create{0}));", wrapClassName);
            }

            if (getItems.Count > 0 || setItems.Count > 0)
            {
                sb.AppendLineEx("\t\tL.RegVar(\"this\", _this, null);");
            }
        }

        static void GenRegisterOpItems()
        {
            if ((op & MetaOp.Add) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__add\", new LuaCSFunction(op_Addition));");
            }

            if ((op & MetaOp.Sub) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__sub\", new LuaCSFunction(op_Subtraction));");
            }

            if ((op & MetaOp.Mul) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__mul\", new LuaCSFunction(op_Multiply));");
            }

            if ((op & MetaOp.Div) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__div\", new LuaCSFunction(op_Division));");
            }

            if ((op & MetaOp.Eq) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__eq\", new LuaCSFunction(op_Equality));");
            }

            if ((op & MetaOp.Neg) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__unm\", new LuaCSFunction(op_UnaryNegation));");
            }

            if ((op & MetaOp.ToStr) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__tostring\", new LuaCSFunction(ToLua.op_ToString));");
            }

            if ((op & MetaOp.Le) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__le\", new LuaCSFunction(op_LessThanOrEqual));");
            }

            if ((op & MetaOp.Lt) != 0)
            {
                sb.AppendLineEx("\t\tL.RegFunction(\"__lt\", new LuaCSFunction(op_GreaterThanOrEqual));");
            }
        }

        static bool IsItemThis(PropertyInfo info)
        {
            MethodInfo md = info.GetGetMethod();

            if (md != null)
            {
                return md.GetParameters().Length != 0;
            }

            md = info.GetSetMethod();

            if (md != null)
            {
                return md.GetParameters().Length != 1;
            }

            return true;
        }

        static void GenRegisterVariables(BindType bType)
        {
            var baseType = bType.baseType;
            var isStaticClass = bType.IsStatic;
            if (fields.Length == 0 && props.Length == 0 && events.Length == 0 && isStaticClass && baseType == null)
            {
                return;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsLiteral || fields[i].IsPrivate || fields[i].IsInitOnly)
                {
                    if (fields[i].IsLiteral && fields[i].FieldType.IsPrimitive && !fields[i].FieldType.IsEnum)
                    {
                        double d = Convert.ToDouble(fields[i].GetValue(null));
                        sb.AppendLineFormat("\t\tL.RegConstant(\"{0}\", {1});", fields[i].Name, d);
                    }
                    else
                    {
                        sb.AppendLineFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction(get_{0}), null);", fields[i].Name);
                    }
                }
                else
                {
                    sb.AppendLineFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction(get_{0}), new LuaCSFunction(set_{0}));", fields[i].Name);
                }
            }

            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].CanRead && props[i].CanWrite && props[i].GetSetMethod(true).IsPublic)
                {
                    _MethodBase md = methods.Find((p) => { return p.Name == "get_" + props[i].Name; });
                    string get = md == null ? "get" : "_get";
                    md = methods.Find((p) => { return p.Name == "set_" + props[i].Name; });
                    string set = md == null ? "set" : "_set";
                    sb.AppendLineFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction({1}_{0}), new LuaCSFunction({2}_{0}));", props[i].Name, get, set);
                }
                else if (props[i].CanRead)
                {
                    _MethodBase md = methods.Find((p) => { return p.Name == "get_" + props[i].Name; });
                    sb.AppendLineFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction({1}_{0}), null);", props[i].Name, md == null ? "get" : "_get");
                }
                else if (props[i].CanWrite)
                {
                    _MethodBase md = methods.Find((p) => { return p.Name == "set_" + props[i].Name; });
                    sb.AppendLineFormat("\t\tL.RegVar(\"{0}\", null, new LuaCSFunction({1}_{0}));", props[i].Name, md == null ? "set" : "_set");
                }
            }

            for (int i = 0; i < events.Length; i++)
            {
                sb.AppendLineFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction(get_{0}), new LuaCSFunction(set_{0}));", events[i].Name);
            }
        }

        static void GenRegisterEventTypes(BindType bType)
        {
            var className = bType.name;
            List<Type> list = new List<Type>();

            foreach (Type t in eventSet)
            {
                string funcName = null;
                string space = GetNameSpace(t, out funcName);

                if (space != className)
                {
                    list.Add(t);
                    continue;
                }

                funcName = ConvertToLibSign(funcName);
                string abr = funcName;
                funcName = ConvertToLibSign(space) + "_" + funcName;

                sb.AppendLineFormat("\t\tL.RegFunction(\"{0}\", new LuaCSFunction({1}));", abr, funcName);
            }

            for (int i = 0; i < list.Count; i++)
            {
                eventSet.Remove(list[i]);
            }
        }

        static void GenRegisterFunction(BindType bType)
        {
            var isStaticClass = bType.IsStatic;
            var libClassName = bType.libName;
            var className = bType.name;
            var type = bType.type;
            var baseType = bType.baseType;

            sb.AppendLineEx("\tpublic static void Register(LuaState L)");
            sb.AppendLineEx("\t{");

            if (isStaticClass)
            {
                sb.AppendLineFormat("\t\tL.BeginStaticLibs(\"{0}\");", libClassName);
            }
            else if (!type.IsGenericType)
            {
                if (baseType == null)
                {
                    sb.AppendLineFormat("\t\tL.BeginClass(typeof({0}), null);", className);
                }
                else
                {
                    sb.AppendLineFormat("\t\tL.BeginClass(typeof({0}), typeof({1}));", className, GetBaseTypeStr(baseType));
                }
            }
            else
            {
                if (baseType == null)
                {
                    sb.AppendLineFormat("\t\tL.BeginClass(typeof({0}), null, \"{1}\");", className, libClassName);
                }
                else
                {
                    sb.AppendLineFormat("\t\tL.BeginClass(typeof({0}), typeof({1}), \"{2}\");", className, GetBaseTypeStr(baseType), libClassName);
                }
            }

            GenRegisterFuncItems(bType);
            GenRegisterOpItems();
            GenRegisterVariables(bType);
            GenRegisterEventTypes(bType);            //注册事件类型

            if (!isStaticClass)
            {
                if (bType.IsOut)
                {
                    sb.AppendLineEx("\t\tL.RegVar(\"out\", get_out, null);");
                }

                sb.AppendLineFormat("\t\tL.EndClass();");
            }
            else
            {
                sb.AppendLineFormat("\t\tL.EndStaticLibs();");
            }

            sb.AppendLineEx("\t}");
        }

        static bool IsParams(ParameterInfo param)
        {
            return param.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }

        static void GenFunction(BindType bType, _MethodBase m)
        {
            string name = GetMethodName(m.Method);
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int {0}(IntPtr L)", name == "Register" ? "_Register" : name);
            sb.AppendLineEx("\t{");

            if (HasAttribute(m.Method, typeof(UseDefinedAttribute)))
            {
                FieldInfo field = extendType.GetField(name + "Defined");
                string strfun = field.GetValue(null) as string;
                sb.AppendLineEx(strfun);
                sb.AppendLineEx("\t}");
                return;
            }

            ParameterInfo[] paramInfos = m.GetParameters();
            int offset = m.IsStatic ? 0 : 1;
            bool haveParams = HasOptionalParam(paramInfos);
            int rc = m.GetReturnType() == typeof(void) ? 0 : 1;

            BeginTry();

            if (!haveParams)
            {
                int count = paramInfos.Length + offset;
                if (m.Name == "op_UnaryNegation") count = 2;
                sb.AppendLineFormat("\t\t\tToLua.CheckArgsCount(L, {0});", count);
            }
            else
            {
                sb.AppendLineEx("\t\t\tint count = LuaDLL.lua_gettop(L);");
            }

            rc += m.ProcessParams(bType, 3, false, int.MaxValue);
            sb.AppendLineFormat("\t\t\treturn {0};", rc);
            EndTry();
            sb.AppendLineEx("\t}");
        }

        //没有未知类型的模版类型List<int> 返回false, List<T>返回true
        static bool IsGenericConstraintType(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.IsGenericParameter || t == typeof(System.ValueType);
            }

            Type[] types = t.GetGenericArguments();

            for (int i = 0; i < types.Length; i++)
            {
                Type t1 = types[i];

                if (t1.IsGenericParameter || t1 == typeof(System.ValueType))
                {
                    return true;
                }

                if (IsGenericConstraintType(t1))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsGenericConstraints(Type[] constraints)
        {
            for (int i = 0; i < constraints.Length; i++)
            {
                if (!IsGenericConstraintType(constraints[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static bool IsGenericMethod(MethodBase md)
        {
            if (md.IsGenericMethod)
            {
                Type[] gts = md.GetGenericArguments();
                List<ParameterInfo> list = new List<ParameterInfo>(md.GetParameters());

                for (int i = 0; i < gts.Length; i++)
                {
                    Type[] ts = gts[i].GetGenericParameterConstraints();

                    if (ts == null || ts.Length == 0 || IsGenericConstraints(ts))
                    {
                        return true;
                    }

                    ParameterInfo p = list.Find((iter) => { return iter.ParameterType == gts[i]; });

                    if (p == null)
                    {
                        return true;
                    }

                    list.RemoveAll((iter) => { return iter.ParameterType == gts[i]; });
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Type t = list[i].ParameterType;

                    if (IsGenericConstraintType(t))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static void GenFunctions(BindType bType)
        {
            var type = bType.type;
            HashSet<string> set = new HashSet<string>();

            for (int i = 0; i < methods.Count; i++)
            {
                _MethodBase m = methods[i];

                if (IsGenericMethod(m.Method))
                {
                    Debugger.Log("Generic Method {0}.{1} cannot be export to lua", LuaMisc.GetTypeName(type), m.GetTotalName(bType));
                    continue;
                }

                string name = GetMethodName(m.Method);

                if (nameCounter[name] > 1)
                {
                    if (!set.Contains(name))
                    {
                        _MethodBase mi = GenOverrideFunc(bType, name);

                        if (mi == null)
                        {
                            set.Add(name);
                            continue;
                        }
                        else
                        {
                            m = mi;     //非重载函数，或者折叠之后只有一个函数
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                set.Add(name);
                GenFunction(bType, m);
            }
        }

        static bool IsSealedType(Type t)
        {
            if (t.IsSealed)
            {
                return true;
            }

            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>) || t.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                return true;
            }

            return false;
        }

        static bool IsNotCheckGeneric(Type t)
        {
            if (t.IsEnum || t.IsValueType)
            {
                return true;
            }

            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>) || t.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                return true;
            }

            return false;
        }

        static bool IsIEnumerator(Type t)
        {
            if (t == typeof(IEnumerator) || t == typeof(CharEnumerator)) return true;

            if (typeof(IEnumerator).IsAssignableFrom(t))
            {
                if (t.IsGenericType)
                {
                    Type gt = t.GetGenericTypeDefinition();

                    if (gt == typeof(List<>.Enumerator) || gt == typeof(Dictionary<,>.Enumerator))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static string GetPushFunction(Type t, bool isByteBuffer = false)
        {
            if (t.IsEnum || t.IsPrimitive || t == typeof(string) || t == typeof(LuaTable) || t == typeof(LuaCSFunction) || t == typeof(LuaThread) || t == typeof(LuaFunction)
                || t == typeof(Type) || t == typeof(IntPtr) || typeof(Delegate).IsAssignableFrom(t) || t == typeof(LuaByteBuffer) // || t == typeof(LuaInteger64)
                || t == typeof(Vector3) || t == typeof(Vector2) || t == typeof(Vector4) || t == typeof(Quaternion) || t == typeof(Color) || t == typeof(RaycastHit)
                || t == typeof(Ray) || t == typeof(Touch) || t == typeof(Bounds) || t == typeof(object))
            {
                return "Push";
            }
            else if (t.IsArray || t == typeof(System.Array))
            {
                return "Push";
            }
            else if (IsIEnumerator(t))
            {
                return "Push";
            }
            else if (t == typeof(LayerMask))
            {
                return "PushLayerMask";
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(t) || typeof(UnityEngine.TrackedReference).IsAssignableFrom(t))
            {
                return IsSealedType(t) ? "PushSealed" : "Push";
            }
            else if (t.IsValueType)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return "PusNullable";
                }

                return "PushValue";
            }
            else if (IsSealedType(t))
            {
                return "PushSealed";
            }

            return "PushObject";
        }

        static void DefaultConstruct(BindType bType)
        {
            var wrapClassName = bType.wrapName;
            var className = bType.name;
            var type = bType.type;
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int _Create{0}(IntPtr L)", wrapClassName);
            sb.AppendLineEx("\t{");
            sb.AppendLineFormat("\t\t{0} obj = new {0}();", className);
            GenPushStr(type, "obj", "\t\t");
            sb.AppendLineEx("\t\treturn 1;");
            sb.AppendLineEx("\t}");
        }

        static string GetCountStr(int count)
        {
            if (count != 0)
            {
                return string.Format("count - {0}", count);
            }

            return "count";
        }

        static void GenOutFunction(BindType bType)
        {
            var type = bType.type;
            var isStaticClass = bType.IsStatic;
            var className = bType.name;
            if (isStaticClass || !bType.IsOut)
            {
                return;
            }

            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineEx("\tstatic int get_out(IntPtr L)");
            sb.AppendLineEx("\t{");
            sb.AppendLineFormat("\t\tToLua.PushOut<{0}>(L, new LuaOut<{0}>());", className);
            sb.AppendLineEx("\t\treturn 1;");
            sb.AppendLineEx("\t}");
        }

        static int GetDefalutParamCount(MethodBase md)
        {
            int count = 0;
            ParameterInfo[] infos = md.GetParameters();

            for (int i = 0; i < infos.Length; i++)
            {
                if (!(infos[i].DefaultValue is DBNull))
                {
                    ++count;
                }
            }

            return count;
        }

        static void InitCtorList(BindType bType)
        {
            var type = bType.type;
            var isStaticClass = bType.IsStatic;
            if (isStaticClass || type.IsAbstract || typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return;
            }

            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | binding);

            if (extendType != null)
            {
                ConstructorInfo[] ctorExtends = extendType.GetConstructors(BindingFlags.Instance | binding);

                if (HasAttribute(ctorExtends[0], typeof(UseDefinedAttribute)))
                {
                    ctorExtList.AddRange(ctorExtends);
                }
            }

            if (constructors.Length == 0)
            {
                return;
            }

            bool isGenericType = type.IsGenericType;
            Type genericType = isGenericType ? type.GetGenericTypeDefinition() : null;
            Type dictType = typeof(Dictionary<,>);
            Type hashType = typeof(HashSet<>);

            for (int i = 0; i < constructors.Length; i++)
            {
                if (IsObsolete(bType, constructors[i]))
                {
                    continue;
                }

                int count = GetDefalutParamCount(constructors[i]);
                int length = constructors[i].GetParameters().Length;

                if (genericType == dictType && length >= 1)
                {
                    Type pt = constructors[i].GetParameters()[0].ParameterType;

                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>))
                    {
                        continue;
                    }
                }
                else if (genericType == hashType && length >= 1)
                {
                    Type pt = constructors[i].GetParameters()[0].ParameterType;

                    if (pt == typeof(int))
                    {
                        continue;
                    }
                }

                for (int j = 0; j < count + 1; j++)
                {
                    _MethodBase r = new _MethodBase(constructors[i], length - j);
                    int index = ctorList.FindIndex((p) => { return CompareMethod(bType, p, r) >= 0; });

                    if (index >= 0)
                    {
                        if (CompareMethod(bType, ctorList[index], r) == 2)
                        {
                            ctorList.RemoveAt(index);
                            ctorList.Add(r);
                        }
                    }
                    else
                    {
                        ctorList.Add(r);
                    }
                }
            }
        }

        static void GenConstructFunction(BindType bType)
        {
            var wrapClassName = bType.wrapName;
            var type = bType.type;
            if (ctorExtList.Count > 0)
            {
                if (HasAttribute(ctorExtList[0], typeof(UseDefinedAttribute)))
                {
                    sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
                    sb.AppendLineFormat("\tstatic int _Create{0}(IntPtr L)", wrapClassName);
                    sb.AppendLineEx("\t{");

                    FieldInfo field = extendType.GetField(extendName + "Defined");
                    string strfun = field.GetValue(null) as string;
                    sb.AppendLineEx(strfun);
                    sb.AppendLineEx("\t}");
                    return;
                }
            }

            if (ctorList.Count == 0)
            {
                if (type.IsValueType)
                {
                    DefaultConstruct(bType);
                }

                return;
            }

            ctorList.Sort(Compare);
            int[] checkTypeMap = CheckCheckTypePos(bType, ctorList);
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int _Create{0}(IntPtr L)", wrapClassName);
            sb.AppendLineEx("\t{");

            BeginTry();
            sb.AppendLineEx("\t\t\tint count = LuaDLL.lua_gettop(L);");
            sb.AppendLineEx();

            _MethodBase md = ctorList[0];
            bool hasEmptyCon = ctorList[0].GetParameters().Length == 0 ? true : false;

            //处理重载构造函数
            if (HasOptionalParam(md.GetParameters()))
            {
                ParameterInfo[] paramInfos = md.GetParameters();
                ParameterInfo param = paramInfos[paramInfos.Length - 1];
                string str = GetTypeStr(bType, param.ParameterType.GetElementType());

                if (paramInfos.Length > 1)
                {
                    string strParams = md.GenParamTypes(bType, 1);
                    sb.AppendLineFormat("\t\t\tif (TypeChecker.CheckTypes<{0}>(L, 1) && TypeChecker.CheckParamsType<{1}>(L, {2}, {3}))", strParams, str, paramInfos.Length, GetCountStr(paramInfos.Length - 1));
                }
                else
                {
                    sb.AppendLineFormat("\t\t\tif (TypeChecker.CheckParamsType<{0}>(L, {1}, {2}))", str, paramInfos.Length, GetCountStr(paramInfos.Length - 1));
                }
            }
            else
            {
                ParameterInfo[] paramInfos = md.GetParameters();

                if (ctorList.Count == 1 || paramInfos.Length == 0 || paramInfos.Length + 1 <= checkTypeMap[0])
                {
                    sb.AppendLineFormat("\t\t\tif (count == {0})", paramInfos.Length);
                }
                else
                {
                    string strParams = md.GenParamTypes(bType, checkTypeMap[0]);
                    sb.AppendLineFormat("\t\t\tif (count == {0} && TypeChecker.CheckTypes<{1}>(L, {2}))", paramInfos.Length, strParams, checkTypeMap[0]);
                }
            }

            sb.AppendLineEx("\t\t\t{");
            int rc = md.ProcessParams(bType, 4, true, checkTypeMap[0] - 1);
            sb.AppendLineFormat("\t\t\t\treturn {0};", rc);
            sb.AppendLineEx("\t\t\t}");

            for (int i = 1; i < ctorList.Count; i++)
            {
                hasEmptyCon = ctorList[i].GetParameters().Length == 0 ? true : hasEmptyCon;
                md = ctorList[i];
                ParameterInfo[] paramInfos = md.GetParameters();

                if (!HasOptionalParam(md.GetParameters()))
                {
                    string strParams = md.GenParamTypes(bType, checkTypeMap[i]);

                    if (paramInfos.Length + 1 > checkTypeMap[i])
                    {
                        sb.AppendLineFormat("\t\t\telse if (count == {0} && TypeChecker.CheckTypes<{1}>(L, {2}))", paramInfos.Length, strParams, checkTypeMap[i]);
                    }
                    else
                    {
                        sb.AppendLineFormat("\t\t\telse if (count == {0})", paramInfos.Length);
                    }
                }
                else
                {
                    ParameterInfo param = paramInfos[paramInfos.Length - 1];
                    string str = GetTypeStr(bType, param.ParameterType.GetElementType());

                    if (paramInfos.Length > 1)
                    {
                        string strParams = md.GenParamTypes(bType, 1);
                        sb.AppendLineFormat("\t\t\telse if (TypeChecker.CheckTypes<{0}>(L, 1) && TypeChecker.CheckParamsType<{1}>(L, {2}, {3}))", strParams, str, paramInfos.Length, GetCountStr(paramInfos.Length - 1));
                    }
                    else
                    {
                        sb.AppendLineFormat("\t\t\telse if (TypeChecker.CheckParamsType<{0}>(L, {1}, {2}))", str, paramInfos.Length, GetCountStr(paramInfos.Length - 1));
                    }
                }

                sb.AppendLineEx("\t\t\t{");
                rc = md.ProcessParams(bType, 4, true, checkTypeMap[i] - 1);
                sb.AppendLineFormat("\t\t\t\treturn {0};", rc);
                sb.AppendLineEx("\t\t\t}");
            }

            var className = bType.name;
            if (type.IsValueType && !hasEmptyCon)
            {
                sb.AppendLineEx("\t\t\telse if (count == 0)");
                sb.AppendLineEx("\t\t\t{");
                sb.AppendLineFormat("\t\t\t\t{0} obj = new {0}();", className);
                GenPushStr(type, "obj", "\t\t\t\t");
                sb.AppendLineEx("\t\t\t\treturn 1;");
                sb.AppendLineEx("\t\t\t}");
            }

            sb.AppendLineEx("\t\t\telse");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineFormat("\t\t\t\treturn LuaDLL.luaL_throw(L, \"invalid arguments to ctor method: {0}.New\");", className);
            sb.AppendLineEx("\t\t\t}");

            EndTry();
            sb.AppendLineEx("\t}");
        }


        //this[] 非静态函数
        static void GenItemPropertyFunction(BindType bType)
        {
            int flag = 0;

            var className = bType.name;
            if (getItems.Count > 0)
            {
                sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
                sb.AppendLineEx("\tstatic int _get_this(IntPtr L)");
                sb.AppendLineEx("\t{");
                BeginTry();

                if (getItems.Count == 1)
                {
                    _MethodBase m = getItems[0];
                    int count = m.GetParameters().Length + 1;
                    sb.AppendLineFormat("\t\t\tToLua.CheckArgsCount(L, {0});", count);
                    m.ProcessParams(bType, 3, false, int.MaxValue);
                    sb.AppendLineEx("\t\t\treturn 1;\r\n");
                }
                else
                {
                    getItems.Sort(Compare);
                    int[] checkTypeMap = CheckCheckTypePos(bType, getItems);

                    sb.AppendLineEx("\t\t\tint count = LuaDLL.lua_gettop(L);");
                    sb.AppendLineEx();

                    for (int i = 0; i < getItems.Count; i++)
                    {
                        GenOverrideFuncBody(bType, getItems[i], i == 0, checkTypeMap[i]);
                    }

                    sb.AppendLineEx("\t\t\telse");
                    sb.AppendLineEx("\t\t\t{");
                    sb.AppendLineFormat("\t\t\t\treturn LuaDLL.luaL_throw(L, \"invalid arguments to operator method: {0}.this\");", className);
                    sb.AppendLineEx("\t\t\t}");
                }

                EndTry();
                sb.AppendLineEx("\t}");
                flag |= 1;
            }

            if (setItems.Count > 0)
            {
                sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
                sb.AppendLineEx("\tstatic int _set_this(IntPtr L)");
                sb.AppendLineEx("\t{");
                BeginTry();

                if (setItems.Count == 1)
                {
                    _MethodBase m = setItems[0];
                    int count = m.GetParameters().Length + 1;
                    sb.AppendLineFormat("\t\t\tToLua.CheckArgsCount(L, {0});", count);
                    m.ProcessParams(bType, 3, false, int.MaxValue);
                    sb.AppendLineEx("\t\t\treturn 0;\r\n");
                }
                else
                {
                    setItems.Sort(Compare);
                    int[] checkTypeMap = CheckCheckTypePos(bType, setItems);

                    sb.AppendLineEx("\t\t\tint count = LuaDLL.lua_gettop(L);");
                    sb.AppendLineEx();

                    for (int i = 0; i < setItems.Count; i++)
                    {
                        GenOverrideFuncBody(bType, setItems[i], i == 0, checkTypeMap[i]);
                    }

                    sb.AppendLineEx("\t\t\telse");
                    sb.AppendLineEx("\t\t\t{");
                    sb.AppendLineFormat("\t\t\t\treturn LuaDLL.luaL_throw(L, \"invalid arguments to operator method: {0}.this\");", className);
                    sb.AppendLineEx("\t\t\t}");
                }


                EndTry();
                sb.AppendLineEx("\t}");
                flag |= 2;
            }

            if (flag != 0)
            {
                sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
                sb.AppendLineEx("\tstatic int _this(IntPtr L)");
                sb.AppendLineEx("\t{");
                BeginTry();
                sb.AppendLineEx("\t\t\tLuaDLL.lua_pushvalue(L, 1);");
                sb.AppendLineFormat("\t\t\tLuaDLL.tolua_bindthis(L, {0}, {1});", (flag & 1) == 1 ? "_get_this" : "null", (flag & 2) == 2 ? "_set_this" : "null");
                sb.AppendLineEx("\t\t\treturn 1;");
                EndTry();
                sb.AppendLineEx("\t}");
            }
        }

        static int GetOptionalParamPos(ParameterInfo[] infos)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                if (IsParams(infos[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        static bool Is64bit(Type t)
        {
            return t == typeof(long) || t == typeof(ulong);
        }

        static int Compare(_MethodBase lhs, _MethodBase rhs)
        {
            int off1 = lhs.IsStatic ? 0 : 1;
            int off2 = rhs.IsStatic ? 0 : 1;

            ParameterInfo[] lp = lhs.GetParameters();
            ParameterInfo[] rp = rhs.GetParameters();

            int pos1 = GetOptionalParamPos(lp);
            int pos2 = GetOptionalParamPos(rp);

            if (pos1 >= 0 && pos2 < 0)
            {
                return 1;
            }
            else if (pos1 < 0 && pos2 >= 0)
            {
                return -1;
            }
            else if (pos1 >= 0 && pos2 >= 0)
            {
                pos1 += off1;
                pos2 += off2;

                if (pos1 != pos2)
                {
                    return pos1 > pos2 ? -1 : 1;
                }
                else
                {
                    pos1 -= off1;
                    pos2 -= off2;

                    if (lp[pos1].ParameterType.GetElementType() == typeof(object) && rp[pos2].ParameterType.GetElementType() != typeof(object))
                    {
                        return 1;
                    }
                    else if (lp[pos1].ParameterType.GetElementType() != typeof(object) && rp[pos2].ParameterType.GetElementType() == typeof(object))
                    {
                        return -1;
                    }
                }
            }

            int c1 = off1 + lp.Length;
            int c2 = off2 + rp.Length;

            if (c1 > c2)
            {
                return 1;
            }
            else if (c1 == c2)
            {
                List<ParameterInfo> list1 = new List<ParameterInfo>(lp);
                List<ParameterInfo> list2 = new List<ParameterInfo>(rp);

                if (list1.Count > list2.Count)
                {
                    if (list1[0].ParameterType == typeof(object))
                    {
                        return 1;
                    }
                    else if (list1[0].ParameterType.IsPrimitive)
                    {
                        return -1;
                    }

                    list1.RemoveAt(0);
                }
                else if (list2.Count > list1.Count)
                {
                    if (list2[0].ParameterType == typeof(object))
                    {
                        return -1;
                    }
                    else if (list2[0].ParameterType.IsPrimitive)
                    {
                        return 1;
                    }

                    list2.RemoveAt(0);
                }

                for (int i = 0; i < list1.Count; i++)
                {
                    if (list1[i].ParameterType == typeof(object) && list2[i].ParameterType != typeof(object))
                    {
                        return 1;
                    }
                    else if (list1[i].ParameterType != typeof(object) && list2[i].ParameterType == typeof(object))
                    {
                        return -1;
                    }
                    else if (list1[i].ParameterType.IsPrimitive && !list2[i].ParameterType.IsPrimitive)
                    {
                        return -1;
                    }
                    else if (!list1[i].ParameterType.IsPrimitive && list2[i].ParameterType.IsPrimitive)
                    {
                        return 1;
                    }
                    else if (list1[i].ParameterType.IsPrimitive && list2[i].ParameterType.IsPrimitive)
                    {
                        if (Is64bit(list1[i].ParameterType) && !Is64bit(list2[i].ParameterType))
                        {
                            return 1;
                        }
                        else if (!Is64bit(list1[i].ParameterType) && Is64bit(list2[i].ParameterType))
                        {
                            return -1;
                        }
                        else if (Is64bit(list1[i].ParameterType) && Is64bit(list2[i].ParameterType) && list1[i].ParameterType != list2[i].ParameterType)
                        {
                            if (list1[i].ParameterType == typeof(ulong))
                            {
                                return 1;
                            }

                            return -1;
                        }
                    }
                }

                return 0;
            }
            else
            {
                return -1;
            }
        }

        static bool HasOptionalParam(ParameterInfo[] infos)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                if (IsParams(infos[i]))
                {
                    return true;
                }
            }

            return false;
        }

        static void CheckObject(string head, Type type, string className, int pos)
        {
            if (type == typeof(object))
            {
                sb.AppendLineFormat("{0}object obj = ToLua.CheckObject(L, {1});", head, pos);
            }
            else if (type == typeof(Type))
            {
                sb.AppendLineFormat("{0}{1} obj = ToLua.CheckMonoType(L, {2});", head, className, pos);
            }
            else if (IsIEnumerator(type))
            {
                sb.AppendLineFormat("{0}{1} obj = ToLua.CheckIter(L, {2});", head, className, pos);
            }
            else
            {
                if (IsNotCheckGeneric(type))
                {
                    sb.AppendLineFormat("{0}{1} obj = ({1})ToLua.CheckObject(L, {2}, TypeTraits<{1}>.type);", head, className, pos);
                }
                else
                {
                    sb.AppendLineFormat("{0}{1} obj = ({1})ToLua.CheckObject<{1}>(L, {2});", head, className, pos);
                }
            }
        }

        static void ToObject(string head, Type type, string className, int pos)
        {
            if (type == typeof(object))
            {
                sb.AppendLineFormat("{0}object obj = ToLua.ToObject(L, {1});", head, pos);
            }
            else
            {
                sb.AppendLineFormat("{0}{1} obj = ({1})ToLua.ToObject(L, {2});", head, className, pos);
            }
        }

        static void BeginTry()
        {
            sb.AppendLineEx("\t\ttry");
            sb.AppendLineEx("\t\t{");
        }

        static void EndTry()
        {
            sb.AppendLineEx("\t\t}");
            sb.AppendLineEx("\t\tcatch (Exception e)");
            sb.AppendLineEx("\t\t{");
            sb.AppendLineEx("\t\t\treturn LuaDLL.toluaL_exception(L, e);");
            sb.AppendLineEx("\t\t}");
        }

        static Type GetRefBaseType(Type argType)
        {
            if (argType.IsByRef)
            {
                return argType.GetElementType();
            }

            return argType;
        }

        static void ProcessArg(BindType bType, Type varType, string head, string arg, int stackPos, bool beCheckTypes = false, bool beParams = false, bool beOutArg = false)
        {
            varType = GetRefBaseType(varType);
            string str = GetTypeStr(bType, varType);
            string checkStr = beCheckTypes ? "To" : "Check";

            if (beOutArg)
            {
                if (varType.IsValueType)
                {
                    sb.AppendLineFormat("{0}{1} {2};", head, str, arg);
                }
                else
                {
                    sb.AppendLineFormat("{0}{1} {2} = null;", head, str, arg);
                }
            }
            else if (varType == typeof(bool))
            {
                string chkstr = beCheckTypes ? "lua_toboolean" : "luaL_checkboolean";
                sb.AppendLineFormat("{0}bool {1} = LuaDLL.{2}(L, {3});", head, arg, chkstr, stackPos);
            }
            else if (varType == typeof(string))
            {
                sb.AppendLineFormat("{0}string {1} = ToLua.{2}String(L, {3});", head, arg, checkStr, stackPos);
            }
            else if (varType == typeof(IntPtr))
            {
                sb.AppendLineFormat("{0}{1} {2} = ToLua.CheckIntPtr(L, {3});", head, str, arg, stackPos);
            }
            else if (varType == typeof(long))
            {
                string chkstr = beCheckTypes ? "tolua_toint64" : "tolua_checkint64";
                sb.AppendLineFormat("{0}{1} {2} = LuaDLL.{3}(L, {4});", head, str, arg, chkstr, stackPos);
            }
            else if (varType == typeof(ulong))
            {
                string chkstr = beCheckTypes ? "tolua_touint64" : "tolua_checkuint64";
                sb.AppendLineFormat("{0}{1} {2} = LuaDLL.{3}(L, {4});", head, str, arg, chkstr, stackPos);
            }
            else if (varType.IsPrimitive || IsNumberEnum(varType))
            {
                string chkstr = beCheckTypes ? "lua_tointeger" : "luaL_checkinteger";

                if (varType == typeof(float) || varType == typeof(double) || varType == typeof(decimal))
                {
                    chkstr = beCheckTypes ? "lua_tonumber" : "luaL_checknumber";
                }

                sb.AppendLineFormat("{0}{1} {2} = ({1})LuaDLL.{3}(L, {4});", head, str, arg, chkstr, stackPos);
            }
            else if (varType == typeof(LuaFunction))
            {
                sb.AppendLineFormat("{0}LuaFunction {1} = ToLua.{2}LuaFunction(L, {3});", head, arg, checkStr, stackPos);
            }
            else if (varType.IsSubclassOf(typeof(System.MulticastDelegate)))
            {
                if (beCheckTypes)
                {
                    sb.AppendLineFormat("{0}{1} {2} = ({1})ToLua.ToObject(L, {3});", head, str, arg, stackPos);
                }
                else
                {
                    sb.AppendLineFormat("{0}{1} {2} = ({1})ToLua.CheckDelegate<{1}>(L, {3});", head, str, arg, stackPos);
                }
            }
            else if (varType == typeof(LuaTable))
            {
                sb.AppendLineFormat("{0}LuaTable {1} = ToLua.{2}LuaTable(L, {3});", head, arg, checkStr, stackPos);
            }
            else if (varType == typeof(Vector2))
            {
                sb.AppendLineFormat("{0}UnityEngine.Vector2 {1} = ToLua.ToVector2(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(Vector3))
            {
                sb.AppendLineFormat("{0}UnityEngine.Vector3 {1} = ToLua.ToVector3(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(Vector4))
            {
                sb.AppendLineFormat("{0}UnityEngine.Vector4 {1} = ToLua.ToVector4(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(Quaternion))
            {
                sb.AppendLineFormat("{0}UnityEngine.Quaternion {1} = ToLua.ToQuaternion(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(Color))
            {
                sb.AppendLineFormat("{0}UnityEngine.Color {1} = ToLua.ToColor(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(Ray))
            {
                sb.AppendLineFormat("{0}UnityEngine.Ray {1} = ToLua.ToRay(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(Bounds))
            {
                sb.AppendLineFormat("{0}UnityEngine.Bounds {1} = ToLua.ToBounds(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(LayerMask))
            {
                sb.AppendLineFormat("{0}UnityEngine.LayerMask {1} = ToLua.ToLayerMask(L, {2});", head, arg, stackPos);
            }
            else if (varType == typeof(object))
            {
                if (bType.type == typeof(Array)) {
                    sb.AppendLineFormat("{0}object {1} = ToLua.ToVarObject(L, {2});", head, arg, stackPos);
                } else {
                    sb.AppendLineFormat("{0}object {1} = ToLua.ToVarObject(L, {2});", head, arg, stackPos);
                }
            }
            else if (varType == typeof(LuaByteBuffer))
            {
                sb.AppendLineFormat("{0}LuaByteBuffer {1} = new LuaByteBuffer(ToLua.CheckByteBuffer(L, {2}));", head, arg, stackPos);
            }
            else if (varType == typeof(Type))
            {
                if (beCheckTypes)
                {
                    sb.AppendLineFormat("{0}System.Type {1} = (System.Type)ToLua.ToObject(L, {2});", head, arg, stackPos);
                }
                else
                {
                    sb.AppendLineFormat("{0}System.Type {1} = ToLua.CheckMonoType(L, {2});", head, arg, stackPos);
                }
            }
            else if (IsIEnumerator(varType))
            {
                if (beCheckTypes)
                {
                    sb.AppendLineFormat("{0}System.Collections.IEnumerator {1} = (System.Collections.IEnumerator)ToLua.ToObject(L, {2});", head, arg, stackPos);
                }
                else
                {
                    sb.AppendLineFormat("{0}System.Collections.IEnumerator {1} = ToLua.CheckIter(L, {2});", head, arg, stackPos);
                }
            }
            else if (varType.IsArray && varType.GetArrayRank() == 1)
            {
                Type et = varType.GetElementType();
                string atstr = GetTypeStr(bType, et);
                string fname;
                bool flag = false;                          //是否模版函数
                bool isObject = false;

                if (et.IsPrimitive)
                {
                    if (beParams)
                    {
                        if (et == typeof(bool))
                        {
                            fname = beCheckTypes ? "ToParamsBool" : "CheckParamsBool";
                        }
                        else if (et == typeof(char))
                        {
                            //char用的多些，特殊处理一下减少gcalloc
                            fname = beCheckTypes ? "ToParamsChar" : "CheckParamsChar";
                        }
                        else
                        {
                            flag = true;
                            fname = beCheckTypes ? "ToParamsNumber" : "CheckParamsNumber";
                        }
                    }
                    else if (et == typeof(char))
                    {
                        fname = "CheckCharBuffer";
                    }
                    else if (et == typeof(byte))
                    {
                        fname = "CheckByteBuffer";
                    }
                    else if (et == typeof(bool))
                    {
                        fname = "CheckBoolArray";
                    }
                    else
                    {
                        fname = beCheckTypes ? "ToNumberArray" : "CheckNumberArray";
                        flag = true;
                    }
                }
                else if (et == typeof(string))
                {
                    if (beParams)
                    {
                        fname = beCheckTypes ? "ToParamsString" : "CheckParamsString";
                    }
                    else
                    {
                        fname = beCheckTypes ? "ToStringArray" : "CheckStringArray";
                    }
                }
                else //if (et == typeof(object))
                {
                    flag = true;

                    if (et == typeof(object))
                    {
                        isObject = true;
                        flag = false;
                    }

                    if (beParams)
                    {
                        fname = (isObject || beCheckTypes) ? "ToParamsObject" : "CheckParamsObject";
                    }
                    else
                    {
                        if (et.IsValueType)
                        {
                            fname = beCheckTypes ? "ToStructArray" : "CheckStructArray";
                        }
                        else
                        {
                            fname = beCheckTypes ? "ToObjectArray" : "CheckObjectArray";
                        }
                    }

                    if (et == typeof(UnityEngine.Object))
                    {
                        ambig |= ObjAmbig.U3dObj;
                    }
                }

                if (flag)
                {
                    if (beParams)
                    {
                        if (!isObject)
                        {
                            sb.AppendLineFormat("{0}{1}[] {2} = ToLua.{3}<{1}>(L, {4}, {5});", head, atstr, arg, fname, stackPos, GetCountStr(stackPos - 1));
                        }
                        else
                        {
                            sb.AppendLineFormat("{0}object[] {1} = ToLua.{2}(L, {3}, {4});", head, arg, fname, stackPos, GetCountStr(stackPos - 1));
                        }
                    }
                    else
                    {
                        sb.AppendLineFormat("{0}{1}[] {2} = ToLua.{3}<{1}>(L, {4});", head, atstr, arg, fname, stackPos);
                    }
                }
                else
                {
                    if (beParams)
                    {
                        sb.AppendLineFormat("{0}{1}[] {2} = ToLua.{3}(L, {4}, {5});", head, atstr, arg, fname, stackPos, GetCountStr(stackPos - 1));
                    }
                    else
                    {
                        sb.AppendLineFormat("{0}{1}[] {2} = ToLua.{3}(L, {4});", head, atstr, arg, fname, stackPos);
                    }
                }
            }
            else if (varType.IsGenericType && varType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type t = TypeChecker.GetNullableType(varType);

                if (beCheckTypes)
                {
                    sb.AppendLineFormat("{0}{1} {2} = ToLua.ToNullable<{3}>(L, {4});", head, str, arg, GetTypeStr(bType, t), stackPos);
                }
                else
                {
                    sb.AppendLineFormat("{0}{1} {2} = ToLua.CheckNullable<{3}>(L, {4});", head, str, arg, GetTypeStr(bType, t), stackPos);
                }
            }
            else if (varType.IsValueType && !varType.IsEnum)
            {
                string func = beCheckTypes ? "To" : "Check";
                sb.AppendLineFormat("{0}{1} {2} = StackTraits<{1}>.{3}(L, {4});", head, str, arg, func, stackPos);
            }
            else //从object派生但不是object
            {
                if (beCheckTypes)
                {
                    sb.AppendLineFormat("{0}{1} {2} = ({1})ToLua.ToObject(L, {3});", head, str, arg, stackPos);
                }
                //else if (varType == typeof(UnityEngine.TrackedReference) || typeof(UnityEngine.TrackedReference).IsAssignableFrom(varType))
                //{
                //    sb.AppendLineFormat("{3}{0} {1} = ({0})ToLua.CheckTrackedReference(L, {2}, typeof({0}));", str, arg, stackPos, head);
                //}
                //else if (typeof(UnityEngine.Object).IsAssignableFrom(varType))
                //{
                //    sb.AppendLineFormat("{3}{0} {1} = ({0})ToLua.CheckUnityObject(L, {2}, typeof({0}));", str, arg, stackPos, head);
                //}
                else
                {
                    if (IsNotCheckGeneric(varType))
                    {
                        sb.AppendLineFormat("{0}{1} {2} = ({1})ToLua.CheckObject(L, {3}, TypeTraits<{1}>.type);", head, str, arg, stackPos);
                    }
                    else
                    {
                        sb.AppendLineFormat("{0}{1} {2} = ({1})ToLua.CheckObject<{1}>(L, {3});", head, str, arg, stackPos);
                    }
                }
            }
        }

        static int GetMethodType(MethodBase md, out PropertyInfo pi)
        {
            pi = null;

            if (!md.IsSpecialName)
            {
                return 0;
            }

            int methodType = 0;
            int pos = allProps.FindIndex((p) => { return p.GetGetMethod() == md || p.GetSetMethod() == md; });

            if (pos >= 0)
            {
                methodType = 1;
                pi = allProps[pos];

                if (md == pi.GetGetMethod())
                {
                    if (md.GetParameters().Length > 0)
                    {
                        methodType = 2;
                    }
                }
                else if (md == pi.GetSetMethod())
                {
                    if (md.GetParameters().Length > 1)
                    {
                        methodType = 2;
                    }
                }
            }

            return methodType;
        }

        static Type GetGenericBaseType(MethodBase md, Type t)
        {
            if (!md.IsGenericMethod)
            {
                return t;
            }

            List<Type> list = new List<Type>(md.GetGenericArguments());

            if (list.Contains(t))
            {
                return t.BaseType;
            }

            return t;
        }

        static bool IsNumberEnum(Type t)
        {
            if (t == typeof(BindingFlags))
            {
                return true;
            }

            return false;
        }

        static void GenPushStr(Type t, string arg, string head, bool isByteBuffer = false)
        {
            if (t == typeof(int))
            {
                sb.AppendLineFormat("{0}LuaDLL.lua_pushinteger(L, {1});", head, arg);
            }
            else if (t == typeof(bool))
            {
                sb.AppendLineFormat("{0}LuaDLL.lua_pushboolean(L, {1});", head, arg);
            }
            else if (t == typeof(string))
            {
                sb.AppendLineFormat("{0}LuaDLL.lua_pushstring(L, {1});", head, arg);
            }
            else if (t == typeof(IntPtr))
            {
                sb.AppendLineFormat("{0}LuaDLL.lua_pushlightuserdata(L, {1});", head, arg);
            }
            else if (t == typeof(long))
            {
                sb.AppendLineFormat("{0}LuaDLL.tolua_pushint64(L, {1});", head, arg);
            }
            else if (t == typeof(ulong))
            {
                sb.AppendLineFormat("{0}LuaDLL.tolua_pushuint64(L, {1});", head, arg);
            }
            else if ((t.IsPrimitive))
            {
                if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                {
                    sb.AppendLineFormat("{0}LuaDLL.lua_pushnumber(L, {1});", head, arg);
                }
                else sb.AppendLineFormat("{0}LuaDLL.lua_pushinteger(L, {1});", head, arg);
            }
            else
            {
                if (isByteBuffer && t == typeof(byte[]))
                {
                    sb.AppendLineFormat("{0}LuaDLL.tolua_pushlstring(L, {1}, {1}.Length);", head, arg);
                }
                else
                {
                    string str = GetPushFunction(t);
                    sb.AppendLineFormat("{0}ToLua.{1}(L, {2});", head, str, arg);
                }
            }
        }

        static bool CompareParmsCount(_MethodBase l, _MethodBase r)
        {
            if (l == r)
            {
                return false;
            }

            int c1 = l.IsStatic ? 0 : 1;
            int c2 = r.IsStatic ? 0 : 1;

            c1 += l.GetParameters().Length;
            c2 += r.GetParameters().Length;

            return c1 == c2;
        }

        //decimal 类型扔掉了
        static Dictionary<Type, int> typeSize = new Dictionary<Type, int>()
        {
            { typeof(char), 2 },
            { typeof(byte), 3 },
            { typeof(sbyte), 4 },
            { typeof(ushort),5 },
            { typeof(short), 6 },
            { typeof(uint), 7 },
            { typeof(int), 8 },
            //{ typeof(ulong), 9 },
            //{ typeof(long), 10 },
            { typeof(decimal), 11 },
            { typeof(float), 12 },
            { typeof(double), 13 },
        };

        //-1 不存在替换, 1 保留左面， 2 保留右面
        static int CompareMethod(BindType bType, _MethodBase l, _MethodBase r)
        {
            int s = 0;

            var type = bType.type;
            if (!CompareParmsCount(l, r))
            {
                return -1;
            }
            else
            {
                ParameterInfo[] lp = l.GetParameters();
                ParameterInfo[] rp = r.GetParameters();

                List<Type> ll = new List<Type>();
                List<Type> lr = new List<Type>();

                if (!l.IsStatic)
                {
                    ll.Add(type);
                }

                if (!r.IsStatic)
                {
                    lr.Add(type);
                }

                for (int i = 0; i < lp.Length; i++)
                {
                    ll.Add(GetParameterType(bType, lp[i]));
                }

                for (int i = 0; i < rp.Length; i++)
                {
                    lr.Add(GetParameterType(bType, rp[i]));
                }

                for (int i = 0; i < ll.Count; i++)
                {
                    if (!typeSize.ContainsKey(ll[i]) || !typeSize.ContainsKey(lr[i]))
                    {
                        if (ll[i] == lr[i])
                        {
                            continue;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (ll[i].IsPrimitive && lr[i].IsPrimitive && s == 0)
                    {
                        s = typeSize[ll[i]] >= typeSize[lr[i]] ? 1 : 2;
                    }
                    else if (ll[i] != lr[i] && !ll[i].IsPrimitive && !lr[i].IsPrimitive)
                    {
                        return -1;
                    }
                }

                if (s == 0 && l.IsStatic)
                {
                    s = 2;
                }
            }

            return s;
        }

        static void Push(BindType bType, List<_MethodBase> list, _MethodBase r)
        {
            var className = bType.name;
            string name = GetMethodName(r.Method);
            int index = list.FindIndex((p) => { return GetMethodName(p.Method) == name && CompareMethod(bType, p, r) >= 0; });

            if (index >= 0)
            {
                if (CompareMethod(bType, list[index], r) == 2)
                {
                    Debugger.LogWarning("{0}.{1} has been dropped as function {2} more match lua", className, list[index].GetTotalName(bType), r.GetTotalName(bType));
                    list.RemoveAt(index);
                    list.Add(r);
                    return;
                }
                else
                {
                    Debugger.LogWarning("{0}.{1} has been dropped as function {2} more match lua", className, r.GetTotalName(bType), list[index].GetTotalName(bType));
                    return;
                }
            }

            list.Add(r);
        }

        static void GenOverrideFuncBody(BindType bType, _MethodBase md, bool beIf, int checkTypeOffset)
        {
            int offset = md.IsStatic ? 0 : 1;
            int ret = md.GetReturnType() == typeof(void) ? 0 : 1;
            string strIf = beIf ? "if " : "else if ";

            if (HasOptionalParam(md.GetParameters()))
            {
                ParameterInfo[] paramInfos = md.GetParameters();
                ParameterInfo param = paramInfos[paramInfos.Length - 1];
                string str = GetTypeStr(bType, param.ParameterType.GetElementType());

                if (paramInfos.Length + offset > 1)
                {
                    string strParams = md.GenParamTypes(bType, 0);
                    sb.AppendLineFormat("\t\t\t{0}(TypeChecker.CheckTypes<{1}>(L, 1) && TypeChecker.CheckParamsType<{2}>(L, {3}, {4}))", strIf, strParams, str, paramInfos.Length + offset, GetCountStr(paramInfos.Length + offset - 1));
                }
                else
                {
                    sb.AppendLineFormat("\t\t\t{0}(TypeChecker.CheckParamsType<{1}>(L, {2}, {3}))", strIf, str, paramInfos.Length + offset, GetCountStr(paramInfos.Length + offset - 1));
                }
            }
            else
            {
                ParameterInfo[] paramInfos = md.GetParameters();

                if (paramInfos.Length + offset > checkTypeOffset)
                {
                    string strParams = md.GenParamTypes(bType, checkTypeOffset);
                    sb.AppendLineFormat("\t\t\t{0}(count == {1} && TypeChecker.CheckTypes<{2}>(L, {3}))", strIf, paramInfos.Length + offset, strParams, checkTypeOffset + 1);
                }
                else
                {
                    sb.AppendLineFormat("\t\t\t{0}(count == {1})", strIf, paramInfos.Length + offset);
                }
            }

            sb.AppendLineEx("\t\t\t{");
            int count = md.ProcessParams(bType, 4, false, checkTypeOffset);
            sb.AppendLineFormat("\t\t\t\treturn {0};", ret + count);
            sb.AppendLineEx("\t\t\t}");
        }

        static int[] CheckCheckTypePos<T>(BindType bType, List<T> list) where T : _MethodBase
        {
            int[] map = new int[list.Count];

            for (int i = 0; i < list.Count;)
            {
                if (HasOptionalParam(list[i].GetParameters()))
                {
                    if (list[0].IsConstructor)
                    {
                        for (int k = 0; k < map.Length; k++)
                        {
                            map[k] = 1;
                        }
                    }
                    else
                    {
                        Array.Clear(map, 0, map.Length);
                    }

                    return map;
                }

                int c1 = list[i].GetParamsCount();
                int count = c1;
                map[i] = count;
                int j = i + 1;

                for (; j < list.Count; j++)
                {
                    int c2 = list[j].GetParamsCount();

                    if (c1 == c2)
                    {
                        count = Mathf.Min(count, list[i].GetEqualParamsCount(bType, list[j]));
                    }
                    else
                    {
                        map[j] = c2;
                        break;
                    }

                    for (int m = i; m <= j; m++)
                    {
                        map[m] = count;
                    }
                }

                i = j;
            }

            return map;
        }

        static void GenOverrideDefinedFunc(MethodBase method)
        {
            string name = GetMethodName(method);
            FieldInfo field = extendType.GetField(name + "Defined");
            string strfun = field.GetValue(null) as string;
            sb.AppendLineEx(strfun);
            return;
        }

        static _MethodBase GenOverrideFunc(BindType bType, string name)
        {
            List<_MethodBase> list = new List<_MethodBase>();

            for (int i = 0; i < methods.Count; i++)
            {
                string curName = GetMethodName(methods[i].Method);

                if (curName == name && !IsGenericMethod(methods[i].Method))
                {
                    Push(bType, list, methods[i]);
                }
            }

            if (list.Count == 1)
            {
                return list[0];
            }
            else if (list.Count == 0)
            {
                return null;
            }

            list.Sort(Compare);
            int[] checkTypeMap = CheckCheckTypePos(bType, list);

            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int {0}(IntPtr L)", name == "Register" ? "_Register" : name);
            sb.AppendLineEx("\t{");

            BeginTry();
            sb.AppendLineEx("\t\t\tint count = LuaDLL.lua_gettop(L);");
            sb.AppendLineEx();

            for (int i = 0; i < list.Count; i++)
            {
                if (HasAttribute(list[i].Method, typeof(OverrideDefinedAttribute)))
                {
                    GenOverrideDefinedFunc(list[i].Method);
                }
                else
                {
                    GenOverrideFuncBody(bType, list[i], i == 0, checkTypeMap[i]);
                }
            }

            var className = bType.name;
            sb.AppendLineEx("\t\t\telse");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineFormat("\t\t\t\treturn LuaDLL.luaL_throw(L, \"invalid arguments to method: {0}.{1}\");", className, name);
            sb.AppendLineEx("\t\t\t}");

            EndTry();
            sb.AppendLineEx("\t}");
            return null;
        }

        public static string CombineTypeStr(string space, string name)
        {
            if (string.IsNullOrEmpty(space))
            {
                return name;
            }
            else
            {
                return space + "." + name;
            }
        }

        public static string GetBaseTypeStr(Type t)
        {
            if (t.IsGenericType)
            {
                return LuaMisc.GetTypeName(t);
            }
            else
            {
                return t.FullName.Replace("+", ".");
            }
        }

        //获取类型名字
        public static string GetTypeStr(BindType bType, Type t)
        {
            if (t.IsByRef)
            {
                t = t.GetElementType();
                return GetTypeStr(bType, t);
            }
            else if (t.IsArray)
            {
                string str = GetTypeStr(bType, t.GetElementType());
                str += LuaMisc.GetArrayRank(t);
                return str;
            }
            else if (t == extendType)
            {
                if (bType != null) {
                    var type = bType.type;
                    return GetTypeStr(bType, type);
                }
            }
            else if (IsIEnumerator(t))
            {
                return LuaMisc.GetTypeName(typeof(IEnumerator));
            }

            return LuaMisc.GetTypeName(t);
        }

        //获取 typeof(string) 这样的名字
        static string GetTypeOf(BindType bType, Type t, string sep)
        {
            string str;

            if (t.IsByRef)
            {
                t = t.GetElementType();
            }

            if (IsNumberEnum(t))
            {
                str = string.Format("uint{0}", sep);
            }
            else if (IsIEnumerator(t))
            {
                str = string.Format("{0}{1}", GetTypeStr(bType, typeof(IEnumerator)), sep);
            }
            else
            {
                str = string.Format("{0}{1}", GetTypeStr(bType, t), sep);
            }

            return str;
        }

        static void GenGetFieldStr(BindType bType, string varName, Type varType, bool isStatic, bool isByteBuffer, bool beOverride = false)
        {
            var className = bType.name;
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int {0}_{1}(IntPtr L)", beOverride ? "_get" : "get", varName);
            sb.AppendLineEx("\t{");

            if (isStatic)
            {
                string arg = string.Format("{0}.{1}", className, varName);
                BeginTry();
                GenPushStr(varType, arg, "\t\t\t", isByteBuffer);
                sb.AppendLineEx("\t\t\treturn 1;");
                EndTry();
            }
            else
            {
                sb.AppendLineEx("\t\tobject o = null;\r\n");
                BeginTry();
                sb.AppendLineEx("\t\t\to = ToLua.ToObject(L, 1);");
                sb.AppendLineFormat("\t\t\t{0} obj = ({0})o;", className);
                sb.AppendLineFormat("\t\t\t{0} ret = obj.{1};", GetTypeStr(bType, varType), varName);
                GenPushStr(varType, "ret", "\t\t\t", isByteBuffer);
                sb.AppendLineEx("\t\t\treturn 1;");

                sb.AppendLineEx("\t\t}");
                sb.AppendLineEx("\t\tcatch(Exception e)");
                sb.AppendLineEx("\t\t{");

                sb.AppendLineFormat("\t\t\treturn LuaDLL.toluaL_exception(L, e, o, \"attempt to index {0} on a nil value\");", varName);
                sb.AppendLineEx("\t\t}");
            }

            sb.AppendLineEx("\t}");
        }

        static void GenGetEventStr(BindType bType, string varName, Type varType)
        {
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int get_{0}(IntPtr L)", varName);
            sb.AppendLineEx("\t{");
            sb.AppendLineFormat("\t\tToLua.Push(L, new EventObject(typeof({0})));", GetTypeStr(bType, varType));
            sb.AppendLineEx("\t\treturn 1;");
            sb.AppendLineEx("\t}");
        }

        static void GenIndexFunc(BindType bType)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsLiteral && fields[i].FieldType.IsPrimitive && !fields[i].FieldType.IsEnum)
                {
                    continue;
                }

                bool beBuffer = IsByteBuffer(fields[i]);
                GenGetFieldStr(bType, fields[i].Name, fields[i].FieldType, fields[i].IsStatic, beBuffer);
            }

            for (int i = 0; i < props.Length; i++)
            {
                if (!props[i].CanRead)
                {
                    continue;
                }

                bool isStatic = true;
                int index = propList.IndexOf(props[i]);

                if (index >= 0)
                {
                    isStatic = false;
                }

                _MethodBase md = methods.Find((p) => { return p.Name == "get_" + props[i].Name; });
                bool beBuffer = IsByteBuffer(props[i]);

                GenGetFieldStr(bType, props[i].Name, props[i].PropertyType, isStatic, beBuffer, md != null);
            }

            for (int i = 0; i < events.Length; i++)
            {
                GenGetEventStr(bType, events[i].Name, events[i].EventHandlerType);
            }
        }

        static void GenSetFieldStr(BindType bType, string varName, Type varType, bool isStatic, bool beOverride = false)
        {
            var className = bType.name;
            var type = bType.type;
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int {0}_{1}(IntPtr L)", beOverride ? "_set" : "set", varName);
            sb.AppendLineEx("\t{");

            if (!isStatic)
            {
                sb.AppendLineEx("\t\tobject o = null;\r\n");
                BeginTry();
                sb.AppendLineEx("\t\t\to = ToLua.ToObject(L, 1);");
                sb.AppendLineFormat("\t\t\t{0} obj = ({0})o;", className);
                ProcessArg(bType, varType, "\t\t\t", "arg0", 2);

                if (typeof(System.MulticastDelegate).IsAssignableFrom(varType))
                {
                    sb.AppendLineEx("");
                    sb.AppendLineFormat("\t\t\tif (!object.ReferenceEquals(obj.{0}, arg0))", varName);
                    sb.AppendLineEx("\t\t\t{");
                    sb.AppendLineFormat("\t\t\t\tif (obj.{0} != null) obj.{0}.SubRef();", varName);
                    sb.AppendLineFormat("\t\t\t\tobj.{0} = arg0;", varName);
                    sb.AppendLineEx("\t\t\t}\r\n");
                }
                else
                {
                    sb.AppendLineFormat("\t\t\tobj.{0} = arg0;", varName);
                }

                if (type.IsValueType)
                {
                    sb.AppendLineEx("\t\t\tToLua.SetBack(L, 1, obj);");
                }
                sb.AppendLineEx("\t\t\treturn 0;");
                sb.AppendLineEx("\t\t}");
                sb.AppendLineEx("\t\tcatch(Exception e)");
                sb.AppendLineEx("\t\t{");
                sb.AppendLineFormat("\t\t\treturn LuaDLL.toluaL_exception(L, e, o, \"attempt to index {0} on a nil value\");", varName);
                sb.AppendLineEx("\t\t}");
            }
            else
            {
                BeginTry();
                ProcessArg(bType, varType, "\t\t\t", "arg0", 2);
                sb.AppendLineFormat("\t\t\t{0}.{1} = arg0;", className, varName);

                if (typeof(System.MulticastDelegate).IsAssignableFrom(varType))
                {
                    sb.AppendLineEx("");
                    sb.AppendLineFormat("\t\t\tif (!object.ReferenceEquals({0}.{1}, arg0))", className, varName);
                    sb.AppendLineEx("\t\t\t{");
                    sb.AppendLineFormat("\t\t\t\tif ({0}.{1} != null) {0}.{1}.SubRef();", className, varName);
                    sb.AppendLineFormat("\t\t\t\t{0}.{1} = arg0;", className, varName);
                    sb.AppendLineEx("\t\t\t}\r\n");
                }
                else
                {
                    sb.AppendLineFormat("\t\t\t{0}.{1} = arg0;", className, varName);
                }

                sb.AppendLineEx("\t\t\treturn 0;");
                EndTry();
            }

            sb.AppendLineEx("\t}");
        }

        static void GenSetEventStr(BindType bType, string varName, Type varType, bool isStatic)
        {
            var className = bType.name;
            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int set_{0}(IntPtr L)", varName);
            sb.AppendLineEx("\t{");
            BeginTry();

            if (!isStatic)
            {
                sb.AppendLineFormat("\t\t\t{0} obj = ({0})ToLua.CheckObject<{0}>(L, 1);", className);
            }

            string strVarType = GetTypeStr(bType, varType);
            string objStr = isStatic ? className : "obj";

            sb.AppendLineEx("\t\t\tEventObject arg0 = null;\r\n");
            sb.AppendLineEx("\t\t\tif (LuaDLL.lua_isuserdata(L, 2) != 0)");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineEx("\t\t\t\targ0 = (EventObject)ToLua.ToObject(L, 2);");
            sb.AppendLineEx("\t\t\t}");
            sb.AppendLineEx("\t\t\telse");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineFormat("\t\t\t\treturn LuaDLL.luaL_throw(L, \"The event '{0}.{1}' can only appear on the left hand side of += or -= when used outside of the type '{0}'\");", className, varName);
            sb.AppendLineEx("\t\t\t}\r\n");

            sb.AppendLineEx("\t\t\tif (arg0.op == EventOp.Add)");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineFormat("\t\t\t\t{0} ev = ({0})arg0.func;", strVarType);
            sb.AppendLineFormat("\t\t\t\t{0}.{1} += ev;", objStr, varName);
            sb.AppendLineEx("\t\t\t}");
            sb.AppendLineEx("\t\t\telse if (arg0.op == EventOp.Sub)");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineFormat("\t\t\t\t{0} ev = ({0})arg0.func;", strVarType);
            sb.AppendLineFormat("\t\t\t\t{0}.{1} -= ev;", objStr, varName);
            sb.AppendLineEx("\t\t\t}\r\n");

            sb.AppendLineEx("\t\t\treturn 0;");
            EndTry();

            sb.AppendLineEx("\t}");
        }

        static void GenNewIndexFunc(BindType bType)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsLiteral || fields[i].IsInitOnly || fields[i].IsPrivate)
                {
                    continue;
                }

                GenSetFieldStr(bType, fields[i].Name, fields[i].FieldType, fields[i].IsStatic);
            }

            for (int i = 0; i < props.Length; i++)
            {
                if (!props[i].CanWrite || !props[i].GetSetMethod(true).IsPublic)
                {
                    continue;
                }

                bool isStatic = true;
                int index = propList.IndexOf(props[i]);

                if (index >= 0)
                {
                    isStatic = false;
                }

                _MethodBase md = methods.Find((p) => { return p.Name == "set_" + props[i].Name; });
                GenSetFieldStr(bType, props[i].Name, props[i].PropertyType, isStatic, md != null);
            }

            for (int i = 0; i < events.Length; i++)
            {
                bool isStatic = eventList.IndexOf(events[i]) < 0;
                GenSetEventStr(bType, events[i].Name, events[i].EventHandlerType, isStatic);
            }
        }

        static void GenLuaFunctionRetValue(BindType bType, StringBuilder sb, Type t, string head, string name, bool beDefined = false)
        {
            if (t == typeof(bool))
            {
                name = beDefined ? name : "bool " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckBoolean();", head, name);
            }
            else if (t == typeof(long))
            {
                name = beDefined ? name : "long " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckLong();", head, name);
            }
            else if (t == typeof(ulong))
            {
                name = beDefined ? name : "ulong " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckULong();", head, name);
            }
            else if (t.IsPrimitive || IsNumberEnum(t))
            {
                string type = GetTypeStr(bType, t);
                name = beDefined ? name : type + " " + name;

                if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                {
                    sb.AppendLineFormat("{0}{1} = ({2})func.CheckNumber();", head, name, type);
                }
                else
                {
                    sb.AppendLineFormat("{0}{1} = ({2})func.CheckInteger();", head, name, type);
                }
            }
            else if (t == typeof(string))
            {
                name = beDefined ? name : "string " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckString();", head, name);
            }
            else if (typeof(System.MulticastDelegate).IsAssignableFrom(t))
            {
                name = beDefined ? name : GetTypeStr(bType, t) + " " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckDelegate();", head, name);
            }
            else if (t == typeof(Vector3))
            {
                name = beDefined ? name : "UnityEngine.Vector3 " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckVector3();", head, name);
            }
            else if (t == typeof(Quaternion))
            {
                name = beDefined ? name : "UnityEngine.Quaternion " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckQuaternion();", head, name);
            }
            else if (t == typeof(Vector2))
            {
                name = beDefined ? name : "UnityEngine.Vector2 " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckVector2();", head, name);
            }
            else if (t == typeof(Vector4))
            {
                name = beDefined ? name : "UnityEngine.Vector4 " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckVector4();", head, name);
            }
            else if (t == typeof(Color))
            {
                name = beDefined ? name : "UnityEngine.Color " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckColor();", head, name);
            }
            else if (t == typeof(Ray))
            {
                name = beDefined ? name : "UnityEngine.Ray " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckRay();", head, name);
            }
            else if (t == typeof(Bounds))
            {
                name = beDefined ? name : "UnityEngine.Bounds " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckBounds();", head, name);
            }
            else if (t == typeof(LayerMask))
            {
                name = beDefined ? name : "UnityEngine.LayerMask " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckLayerMask();", head, name);
            }
            else if (t == typeof(object))
            {
                name = beDefined ? name : "object " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckVariant();", head, name);
            }
            else if (t == typeof(byte[]))
            {
                name = beDefined ? name : "byte[] " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckByteBuffer();", head, name);
            }
            else if (t == typeof(char[]))
            {
                name = beDefined ? name : "char[] " + name;
                sb.AppendLineFormat("{0}{1} = func.CheckCharBuffer();", head, name);
            }
            else
            {
                string type = GetTypeStr(bType, t);
                name = beDefined ? name : type + " " + name;
                sb.AppendLineFormat("{0}{1} = ({2})func.CheckObject(TypeTraits<{2}>.type);", head, name, type);

                //Debugger.LogError("GenLuaFunctionCheckValue undefined type:" + t.FullName);
            }
        }

        public static bool IsByteBuffer(Type type)
        {
            object[] attrs = type.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(LuaByteBufferAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsByteBuffer(MemberInfo mb)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(LuaByteBufferAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        static void GenDelegateBody(BindType bType, StringBuilder sb, Type t, string head, bool hasSelf = false)
        {
            MethodInfo mi = t.GetMethod("Invoke");
            ParameterInfo[] pi = mi.GetParameters();
            int n = pi.Length;

            if (n == 0)
            {
                if (mi.ReturnType == typeof(void))
                {
                    if (!hasSelf)
                    {
                        sb.AppendLineFormat("{0}{{\r\n{0}\tfunc.Call();\r\n{0}}}", head);
                    }
                    else
                    {
                        sb.AppendLineFormat("{0}{{\r\n{0}\tfunc.BeginPCall();", head);
                        sb.AppendLineFormat("{0}\tfunc.Push(self);", head);
                        sb.AppendLineFormat("{0}\tfunc.PCall();", head);
                        sb.AppendLineFormat("{0}\tfunc.EndPCall();", head);
                        sb.AppendLineFormat("{0}}}", head);
                    }
                }
                else
                {
                    sb.AppendLineFormat("{0}{{\r\n{0}\tfunc.BeginPCall();", head);
                    if (hasSelf) sb.AppendLineFormat("{0}\tfunc.Push(self);", head);
                    sb.AppendLineFormat("{0}\tfunc.PCall();", head);
                    GenLuaFunctionRetValue(bType, sb, mi.ReturnType, head + "\t", "ret");
                    sb.AppendLineFormat("{0}\tfunc.EndPCall();", head);
                    sb.AppendLineEx(head + "\treturn ret;");
                    sb.AppendLineFormat("{0}}}", head);
                }

                return;
            }

            sb.AppendLineFormat("{0}{{{0}", head);
            sb.AppendLineEx("\tfunc.BeginPCall();");
            if (hasSelf) sb.AppendLineFormat("{0}\tfunc.Push(self);", head);

            for (int i = 0; i < n; i++)
            {
                string push = GetPushFunction(pi[i].ParameterType);

                if (!IsParams(pi[i]))
                {
                    if (pi[i].ParameterType == typeof(byte[]) && IsByteBuffer(t))
                    {
                        sb.AppendLineFormat("{2}\tfunc.PushByteBuffer(param{1});", push, i, head);
                    }
                    else if (pi[i].Attributes != ParameterAttributes.Out)
                    {
                        sb.AppendLineFormat("{2}\tfunc.{0}(param{1});", push, i, head);
                    }
                }
                else
                {
                    sb.AppendLineEx();
                    sb.AppendLineFormat("{0}\tfor (int i = 0; i < param{1}.Length; i++)", head, i);
                    sb.AppendLineEx(head + "\t{");
                    sb.AppendLineFormat("{2}\t\tfunc.{0}(param{1}[i]);", push, i, head);
                    sb.AppendLineEx(head + "\t}\r\n");
                }
            }

            sb.AppendLineFormat("{0}\tfunc.PCall();", head);

            if (mi.ReturnType == typeof(void))
            {
                for (int i = 0; i < pi.Length; i++)
                {
                    if ((pi[i].Attributes & ParameterAttributes.Out) != ParameterAttributes.None)
                    {
                        GenLuaFunctionRetValue(bType, sb, pi[i].ParameterType.GetElementType(), head + "\t", "param" + i, true);
                    }
                }

                sb.AppendLineFormat("{0}\tfunc.EndPCall();", head);
            }
            else
            {
                GenLuaFunctionRetValue(bType, sb, mi.ReturnType, head + "\t", "ret");

                for (int i = 0; i < pi.Length; i++)
                {
                    if ((pi[i].Attributes & ParameterAttributes.Out) != ParameterAttributes.None)
                    {
                        GenLuaFunctionRetValue(bType, sb, pi[i].ParameterType.GetElementType(), head + "\t", "param" + i, true);
                    }
                }

                sb.AppendLineFormat("{0}\tfunc.EndPCall();", head);
                sb.AppendLineEx(head + "\treturn ret;");
            }

            sb.AppendLineFormat("{0}}}", head);
        }

        //static void GenToStringFunction()
        //{
        //    if ((op & MetaOp.ToStr) == 0)
        //    {
        //        return;
        //    }

        //    sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
        //    sb.AppendLineEx("\tstatic int Lua_ToString(IntPtr L)");
        //    sb.AppendLineEx("\t{");
        //    sb.AppendLineEx("\t\tobject obj = ToLua.ToObject(L, 1);\r\n");

        //    sb.AppendLineEx("\t\tif (obj != null)");
        //    sb.AppendLineEx("\t\t{");
        //    sb.AppendLineEx("\t\t\tLuaDLL.lua_pushstring(L, obj.ToString());");
        //    sb.AppendLineEx("\t\t}");
        //    sb.AppendLineEx("\t\telse");
        //    sb.AppendLineEx("\t\t{");
        //    sb.AppendLineEx("\t\t\tLuaDLL.lua_pushnil(L);");
        //    sb.AppendLineEx("\t\t}");
        //    sb.AppendLineEx();
        //    sb.AppendLineEx("\t\treturn 1;");
        //    sb.AppendLineEx("\t}");
        //}

        static bool IsNeedOp(BindType bType, string name)
        {
            if (name == "op_Addition")
            {
                op |= MetaOp.Add;
            }
            else if (name == "op_Subtraction")
            {
                op |= MetaOp.Sub;
            }
            else if (name == "op_Equality")
            {
                op |= MetaOp.Eq;
            }
            else if (name == "op_Multiply")
            {
                op |= MetaOp.Mul;
            }
            else if (name == "op_Division")
            {
                op |= MetaOp.Div;
            }
            else if (name == "op_UnaryNegation")
            {
                op |= MetaOp.Neg;
            }
            else if (name == "ToString" && !bType.IsStatic)
            {
                op |= MetaOp.ToStr;
            }
            else if(name == "op_LessThanOrEqual")
            {
                op |= MetaOp.Le;
            }
            else if(name == "op_GreaterThanOrEqual")
            {
                op |= MetaOp.Lt;
            }
            else
            {
                return false;
            }


            return true;
        }

        static void CallOpFunction(string name, int count, string ret)
        {
            string head = string.Empty;

            for (int i = 0; i < count; i++)
            {
                head += "\t";
            }

            if (name == "op_Addition")
            {
                sb.AppendLineFormat("{0}{1} o = arg0 + arg1;", head, ret);
            }
            else if (name == "op_Subtraction")
            {
                sb.AppendLineFormat("{0}{1} o = arg0 - arg1;", head, ret);
            }
            else if (name == "op_Equality")
            {
                sb.AppendLineFormat("{0}{1} o = arg0 == arg1;", head, ret);
            }
            else if (name == "op_Multiply")
            {
                sb.AppendLineFormat("{0}{1} o = arg0 * arg1;", head, ret);
            }
            else if (name == "op_Division")
            {
                sb.AppendLineFormat("{0}{1} o = arg0 / arg1;", head, ret);
            }
            else if (name == "op_UnaryNegation")
            {
                sb.AppendLineFormat("{0}{1} o = -arg0;", head, ret);
            }
            else if (name == "op_LessThanOrEqual")
            {
                sb.AppendLineFormat("{0}{1} o = arg0 >= arg1;", head, ret);
            }
            else if (name == "op_GreaterThanOrEqual")
            {
                sb.AppendLineFormat("{0}{1} o = arg0 >= arg1 ? false : true;", head, ret);
            }
        }

        public static bool IsObsolete(BindType bType, MemberInfo mb)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(System.ObsoleteAttribute) || t == typeof(NoToLuaAttribute) || t == typeof(MonoPInvokeCallbackAttribute) ||
                    t.Name == "MonoNotSupportedAttribute" || t.Name == "MonoTODOAttribute") // || t.ToString() == "UnityEngine.WrapperlessIcall")
                {
                    return true;
                }
            }

            if (IsMemberFilter(bType, mb))
            {
                return true;
            }

            return false;
        }

        public static bool HasAttribute(MemberInfo mb, Type atrtype)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == atrtype)
                {
                    return true;
                }
            }

            return false;
        }

        static void GenEnum(BindType bType)
        {
            var type = bType.type;
            var className = bType.name;
            var wrapClassName = bType.wrapName;
            fields = type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
            List<FieldInfo> list = new List<FieldInfo>(fields);

            for (int i = list.Count - 1; i > 0; i--)
            {
                if (IsObsolete(bType, list[i]))
                {
                    list.RemoveAt(i);
                }
            }

            fields = list.ToArray();

            sb.AppendLineEx("\tpublic static void Register(LuaState L)");
            sb.AppendLineEx("\t{");
            sb.AppendLineFormat("\t\tL.BeginEnum(typeof({0}));", className);

            for (int i = 0; i < fields.Length; i++)
            {
                sb.AppendLineFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction(get_{0}), null);", fields[i].Name);
            }

            sb.AppendLineFormat("\t\tL.RegFunction(\"IntToEnum\", new LuaCSFunction(IntToEnum));");
            sb.AppendLineFormat("\t\tL.EndEnum();");
            sb.AppendLineFormat("\t\tTypeTraits<{0}>.Check = CheckType;", className);
            sb.AppendLineFormat("\t\tStackTraits<{0}>.Push = Push;", className);
            sb.AppendLineEx("\t}");
            sb.AppendLineEx();

            sb.AppendLineFormat("\tstatic void Push(IntPtr L, {0} arg)", className);
            sb.AppendLineEx("\t{");
            sb.AppendLineEx("\t\tToLua.Push(L, arg);");
            sb.AppendLineEx("\t}");
            sb.AppendLineEx();

            sb.AppendLineFormat("\tstatic Type TypeOf_{0} = typeof({1});", wrapClassName, className);
            sb.AppendLineEx();

            sb.AppendLineEx("\tstatic bool CheckType(IntPtr L, int pos)");
            sb.AppendLineEx("\t{");
            sb.AppendLineFormat("\t\treturn TypeChecker.CheckEnumType(TypeOf_{0}, L, pos);", wrapClassName);
            sb.AppendLineEx("\t}");

            for (int i = 0; i < fields.Length; i++)
            {
                sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
                sb.AppendLineFormat("\tstatic int get_{0}(IntPtr L)", fields[i].Name);
                sb.AppendLineEx("\t{");
                sb.AppendLineFormat("\t\tToLua.Push(L, {0}.{1});", className, fields[i].Name);
                sb.AppendLineEx("\t\treturn 1;");
                sb.AppendLineEx("\t}");
            }

            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineEx("\tstatic int IntToEnum(IntPtr L)");
            sb.AppendLineEx("\t{");
            sb.AppendLineEx("\t\tint arg0 = (int)LuaDLL.lua_tointeger(L, 1);");
            sb.AppendLineFormat("\t\t{0} o = ({0})arg0;", className);
            sb.AppendLineEx("\t\tToLua.Push(L, o);");
            sb.AppendLineEx("\t\treturn 1;");
            sb.AppendLineEx("\t}");
        }



        static string GetDelegateParams(BindType bType, MethodInfo mi)
        {
            ParameterInfo[] infos = mi.GetParameters();
            List<string> list = new List<string>();

            for (int i = 0; i < infos.Length; i++)
            {
                string s2 = GetTypeStr(bType, infos[i].ParameterType) + " param" + i;

                if (infos[i].ParameterType.IsByRef)
                {
                    if (infos[i].Attributes == ParameterAttributes.Out)
                    {
                        s2 = "out " + s2;
                    }
                    else
                    {
                        s2 = "ref " + s2;
                    }
                }

                list.Add(s2);
            }

            return string.Join(", ", list.ToArray());
        }

        static string GetReturnValue(BindType bType, Type t)
        {
            if (t.IsPrimitive)
            {
                if (t == typeof(bool))
                {
                    return "false";
                }
                else if (t == typeof(char))
                {
                    return "'\\0'";
                }
                else
                {
                    return "0";
                }
            }
            else if (!t.IsValueType)
            {
                return "null";
            }
            else
            {
                return string.Format("default({0})", GetTypeStr(bType, t));
            }
        }

        static string GetDefaultDelegateBody(BindType bType, MethodInfo md)
        {
            string str = "\r\n\t\t\t{\r\n";
            bool flag = false;
            ParameterInfo[] pis = md.GetParameters();

            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i].Attributes == ParameterAttributes.Out)
                {
                    str += string.Format("\t\t\t\tparam{0} = {1};\r\n", i, GetReturnValue(bType, pis[i].ParameterType.GetElementType()));
                    flag = true;
                }
            }

            if (flag)
            {
                if (md.ReturnType != typeof(void))
                {
                    str += "\t\t\treturn ";
                    str += GetReturnValue(bType, md.ReturnType);
                    str += ";";
                }

                str += "\t\t\t};\r\n\r\n";
                return str;
            }

            if (md.ReturnType == typeof(void))
            {
                return "{ };\r\n";
            }
            else
            {
                return string.Format("{{ return {0}; }};\r\n", GetReturnValue(bType, md.ReturnType));
            }
        }

        public static void GenDelegates(BindType bType, DelegateType[] list, string saveDir, string fileName = "DelegateGenFactory")
        {
            usingList.Add("System");

            for (int i = 0; i < list.Length; i++)
            {
                Type t = list[i].type;

                if (!typeof(System.Delegate).IsAssignableFrom(t))
                {
                    Debug.LogError(t.FullName + " not a delegate type");
                    return;
                }
            }

            sb.AppendLineEx("namespace LuaInterface.ObjectWrap");
            sb.AppendLineEx("{");

            sb.Append("public class DelegateGenFactory\r\n");
            sb.Append("{\r\n");
            sb.Append("\tstatic DelegateGenFactory factory = new DelegateGenFactory();\r\n");
            sb.AppendLineEx();
            sb.Append("\tpublic static void Init()\r\n");
            sb.Append("\t{\r\n");
            sb.Append("\t\tRegister();\r\n");
            sb.AppendLineEx("\t}\r\n");

            sb.Append("\tpublic static void Register()\r\n");
            sb.Append("\t{\r\n");
            sb.Append("\t\tDelegateFactory.ClearDelegateCreate();\r\n");

            for (int i = 0; i < list.Length; i++)
            {
                string type = list[i].strType;
                string name = list[i].name;
                sb.AppendLineFormat("\t\tDelegateFactory.AddDelegateCreate(typeof({0}), factory.{1});", type, name);
            }

            sb.AppendLineEx();

            for (int i = 0; i < list.Length; i++)
            {
                string type = list[i].strType;
                string name = list[i].name;
                sb.AppendLineFormat("\t\tDelegateTraits<{0}>.Init(factory.{1});", type, name);
            }

            sb.AppendLineEx();

            for (int i = 0; i < list.Length; i++)
            {
                string type = list[i].strType;
                string name = list[i].name;
                sb.AppendLineFormat("\t\tTypeTraits<{0}>.Init(factory.Check_{1});", type, name);
            }

            sb.AppendLineEx();

            for (int i = 0; i < list.Length; i++)
            {
                string type = list[i].strType;
                string name = list[i].name;
                sb.AppendLineFormat("\t\tStackTraits<{0}>.Push = factory.Push_{1};", type, name);
            }

            sb.Append("\t}\r\n");

            for (int i = 0; i < list.Length; i++)
            {
                Type t = list[i].type;
                string strType = list[i].strType;
                string name = list[i].name;
                MethodInfo mi = t.GetMethod("Invoke");
                string args = GetDelegateParams(bType, mi);

                //生成委托类
                sb.AppendLineFormat("\tclass {0}_Event : LuaDelegate", name);
                sb.AppendLineEx("\t{");
                sb.AppendLineFormat("\t\tpublic {0}_Event(LuaFunction func) : base(func) {{ }}", name);
                sb.AppendLineFormat("\t\tpublic {0}_Event(LuaFunction func, LuaTable self) : base(func, self) {{ }}", name);
                sb.AppendLineEx();
                sb.AppendLineFormat("\t\tpublic {0} Call({1})", GetTypeStr(bType, mi.ReturnType), args);
                GenDelegateBody(bType, sb, t, "\t\t");
                sb.AppendLineEx();
                sb.AppendLineFormat("\t\tpublic {0} CallWithSelf({1})", GetTypeStr(bType, mi.ReturnType), args);
                GenDelegateBody(bType, sb, t, "\t\t", true);
                sb.AppendLineEx("\t}\r\n");

                //生成转换函数1
                sb.AppendLineFormat("\tpublic {0} {1}(LuaFunction func, LuaTable self, bool flag)", strType, name);
                sb.AppendLineEx("\t{");
                sb.AppendLineEx("\t\tif (func == null)");
                sb.AppendLineEx("\t\t{");
                sb.AppendFormat("\t\t\t{0} fn = delegate({1}) {2}", strType, args, GetDefaultDelegateBody(bType, mi));
                sb.AppendLineEx("\t\t\treturn fn;");
                sb.AppendLineEx("\t\t}\r\n");
                sb.AppendLineEx("\t\tif(!flag)");
                sb.AppendLineEx("\t\t{");
                sb.AppendLineFormat("\t\t\t{0}_Event target = new {0}_Event(func);", name);
                sb.AppendLineFormat("\t\t\t{0} d = target.Call;", strType);
                sb.AppendLineEx("\t\t\ttarget.method = d.Method;");
                sb.AppendLineEx("\t\t\treturn d;");
                sb.AppendLineEx("\t\t}");
                sb.AppendLineEx("\t\telse");
                sb.AppendLineEx("\t\t{");
                sb.AppendLineFormat("\t\t\t{0}_Event target = new {0}_Event(func, self);", name);
                sb.AppendLineFormat("\t\t\t{0} d = target.CallWithSelf;", strType);
                sb.AppendLineEx("\t\t\ttarget.method = d.Method;");
                sb.AppendLineEx("\t\t\treturn d;");
                sb.AppendLineEx("\t\t}");
                sb.AppendLineEx("\t}\r\n");

                sb.AppendLineFormat("\tbool Check_{0}(IntPtr L, int pos)", name);
                sb.AppendLineEx("\t{");
                sb.AppendLineFormat("\t\treturn TypeChecker.CheckDelegateType<{0}>(L, pos);", strType);
                sb.AppendLineEx("\t}\r\n");

                sb.AppendLineFormat("\tvoid Push_{0}(IntPtr L, {1} o)", name, strType);
                sb.AppendLineEx("\t{");
                sb.AppendLineEx("\t\tToLua.Push(L, o);");
                sb.AppendLineEx("\t}\r\n");
            }

            sb.AppendLineEx("} //end class");
            sb.AppendLineEx("} //end namespace LuaInterface");

            SaveFile($"{saveDir}/{fileName}.cs");

            Clear();
        }

        static bool IsUseDefinedAttributee(MemberInfo mb)
        {
            object[] attrs = mb.GetCustomAttributes(false);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(UseDefinedAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsMethodEqualExtend(BindType bType, MethodBase a, MethodBase b)
        {
            var type = bType.type;
            if (a.Name != b.Name)
            {
                return false;
            }

            int c1 = a.IsStatic ? 0 : 1;
            int c2 = b.IsStatic ? 0 : 1;

            c1 += a.GetParameters().Length;
            c2 += b.GetParameters().Length;

            if (c1 != c2) return false;

            ParameterInfo[] lp = a.GetParameters();
            ParameterInfo[] rp = b.GetParameters();

            List<Type> ll = new List<Type>();
            List<Type> lr = new List<Type>();

            if (!a.IsStatic)
            {
                ll.Add(type);
            }

            if (!b.IsStatic)
            {
                lr.Add(type);
            }

            for (int i = 0; i < lp.Length; i++)
            {
                ll.Add(GetParameterType(bType, lp[i]));
            }

            for (int i = 0; i < rp.Length; i++)
            {
                lr.Add(GetParameterType(bType, rp[i]));
            }

            for (int i = 0; i < ll.Count; i++)
            {
                if (ll[i] != lr[i])
                {
                    return false;
                }
            }

            return true;
        }

        static void ProcessEditorExtend(BindType bType, Type extendType, List<_MethodBase> list)
        {
            if (extendType != null)
            {
                var list2 = extendType.GetMethods(BindingFlags.Instance | binding | BindingFlags.DeclaredOnly)
                        //去掉操作符函数
                        .Where(m => {
                            var name = m.Name;
                            return !name.StartsWith("op_") && !name.StartsWith("add_") && !name.StartsWith("remove_") || IsNeedOp(bType, name);
                        })
                        .ToList();

                for (int i = list2.Count - 1; i >= 0; i--)
                {
                    if (IsUseDefinedAttributee(list2[i]))
                    {
                        list.RemoveAll((md) => { return md.Name == list2[i].Name; });
                    }
                    else
                    {
                        int index = list.FindIndex((md) => { return IsMethodEqualExtend(bType, md.Method, list2[i]); });

                        if (index >= 0)
                        {
                            list.RemoveAt(index);
                        }
                    }
                }

                list.AddRange(list2.Where(m => !IsObsolete(bType, m)).Select(m => new _MethodBase(m)).ToList());

                FieldInfo field = extendType.GetField("AdditionNameSpace");
                if (field != null)
                {
                    string str = field.GetValue(null) as string;
                    usingList.AddRange(str.Split(new char[] { ';' }).ToList());
                }
            }
        }

        static bool IsGenericType(MethodInfo md, Type t)
        {
            Type[] list = md.GetGenericArguments();

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == t)
                {
                    return true;
                }
            }

            return false;
        }

        static void ProcessExtendType(BindType bType, Type extendType, List<_MethodBase> list)
        {
            var type = bType.type;
            if (extendType != null)
            {
                List<MethodInfo> list2 = new List<MethodInfo>();
                list2.AddRange(extendType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

                for (int i = list2.Count - 1; i >= 0; i--)
                {
                    MethodInfo md = list2[i];

                    if (!md.IsDefined(typeof(ExtensionAttribute), false))
                    {
                        continue;
                    }

                    ParameterInfo[] plist = md.GetParameters();
                    Type t = plist[0].ParameterType;

                    if (t == type || t.IsAssignableFrom(type) || (IsGenericType(md, t) && (type == t.BaseType || type.IsSubclassOf(t.BaseType))))
                    {
                        if (!IsObsolete(bType, list2[i]))
                        {
                            _MethodBase mb = new _MethodBase(md);
                            mb.BeExtend = true;
                            list.Add(mb);
                        }
                    }
                }
            }
        }

        static void ProcessExtends(BindType bType, List<_MethodBase> list)
        {
            var className = bType.name;
            var extendList = bType.extendList;
            extendName = "ToLua_" + className.Replace(".", "_");
            extendType = Type.GetType(extendName + ", Assembly-CSharp-Editor");
            ProcessEditorExtend(bType, extendType, list);
            string temp = null;

            for (int i = 0; i < extendList.Count; i++)
            {
                ProcessExtendType(bType, extendList[i], list);
                string nameSpace = GetNameSpace(extendList[i], out temp);

                if (!string.IsNullOrEmpty(nameSpace))
                {
                    usingList.Add(nameSpace);
                }
            }

        }

        public static void GenEventFunction(BindType bType, Type t, StringBuilder sb)
        {
            string funcName;
            string space = GetNameSpace(t, out funcName);
            funcName = CombineTypeStr(space, funcName);
            funcName = ConvertToLibSign(funcName);

            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int {0}(IntPtr L)", funcName);
            sb.AppendLineEx("\t{");
            sb.AppendLineEx("\t\ttry");
            sb.AppendLineEx("\t\t{");
            sb.AppendLineEx("\t\t\tint count = LuaDLL.lua_gettop(L);");
            sb.AppendLineEx("\t\t\tLuaFunction func = ToLua.CheckLuaFunction(L, 1);");
            sb.AppendLineEx();
            sb.AppendLineEx("\t\t\tif (count == 1)");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineFormat("\t\t\t\tDelegate arg1 = DelegateTraits<{0}>.Create(func);", GetTypeStr(bType, t));
            sb.AppendLineEx("\t\t\t\tToLua.Push(L, arg1);");
            sb.AppendLineEx("\t\t\t\tfunc.Dispose();");
            sb.AppendLineEx("\t\t\t}");
            sb.AppendLineEx("\t\t\telse");
            sb.AppendLineEx("\t\t\t{");
            sb.AppendLineEx("\t\t\t\tLuaTable self = ToLua.CheckLuaTable(L, 2);");
            sb.AppendLineFormat("\t\t\t\tDelegate arg1 = DelegateTraits<{0}>.Create(func, self);", GetTypeStr(bType, t));
            sb.AppendLineFormat("\t\t\t\tToLua.Push(L, arg1);");
            sb.AppendLineEx("\t\t\t\tfunc.Dispose();");
            sb.AppendLineEx("\t\t\t\tself.Dispose();");
            sb.AppendLineEx("\t\t\t}");

            sb.AppendLineEx("\t\t\treturn 1;");
            sb.AppendLineEx("\t\t}");
            sb.AppendLineEx("\t\tcatch(Exception e)");
            sb.AppendLineEx("\t\t{");
            sb.AppendLineEx("\t\t\treturn LuaDLL.toluaL_exception(L, e);");
            sb.AppendLineEx("\t\t}");
            sb.AppendLineEx("\t}");

        }

        static void GenEventFunctions(BindType bType)
        {
            foreach (Type t in eventSet)
            {
                GenEventFunction(bType, t, sb);
            }
        }

        static string RemoveChar(string str, char c)
        {
            int index = str.IndexOf(c);

            while (index > 0)
            {
                str = str.Remove(index, 1);
                index = str.IndexOf(c);
            }

            return str;
        }

        public static string ConvertToLibSign(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            str = str.Replace('<', '_');
            str = RemoveChar(str, '>');
            str = str.Replace('[', 's');
            str = RemoveChar(str, ']');
            str = str.Replace('.', '_');
            return str.Replace(',', '_');
        }

        public static string GetNameSpace(Type t, out string libName)
        {
            if (t.IsGenericType)
            {
                return GetGenericNameSpace(t, out libName);
            }
            else
            {
                string space = t.FullName;

                if (space.Contains("+"))
                {
                    space = space.Replace('+', '.');
                    int index = space.LastIndexOf('.');
                    libName = space.Substring(index + 1);
                    return space.Substring(0, index);
                }
                else
                {
                    libName = t.Namespace == null ? space : space.Substring(t.Namespace.Length + 1);
                    return t.Namespace;
                }
            }
        }

        static string GetGenericNameSpace(Type t, out string libName)
        {
            Type[] gArgs = t.GetGenericArguments();
            string typeName = t.FullName;
            int count = gArgs.Length;
            int pos = typeName.IndexOf("[");
            if (pos > 0) {
                typeName = typeName.Substring(0, pos);
            }

            string str = null;
            string name = null;
            int offset = 0;
            pos = typeName.IndexOf("+");

            while (pos > 0)
            {
                str = typeName.Substring(0, pos);
                typeName = typeName.Substring(pos + 1);
                pos = str.IndexOf('`');

                if (pos > 0)
                {
                    count = (int)(str[pos + 1] - '0');
                    str = str.Substring(0, pos);
                    str += "<" + string.Join(",", LuaMisc.GetGenericName(gArgs, offset, count)) + ">";
                    offset += count;
                }

                name = CombineTypeStr(name, str);
                pos = typeName.IndexOf("+");
            }

            string space = name;
            str = typeName;

            if (offset < gArgs.Length)
            {
                pos = str.IndexOf('`');
                count = (int)(str[pos + 1] - '0');
                str = str.Substring(0, pos);
                str += "<" + string.Join(",", LuaMisc.GetGenericName(gArgs, offset, count)) + ">";
            }

            libName = str;

            if (string.IsNullOrEmpty(space))
            {
                space = t.Namespace;

                if (space != null)
                {
                    libName = str.Substring(space.Length + 1);
                }
            }

            return space;
        }

        static Type GetParameterType(BindType bType, ParameterInfo info)
        {
            var type = bType.type;
            if (info.ParameterType == extendType)
            {
                return type;
            }

            return info.ParameterType;
        }

        public static void GenAutoRegister(string saveDir, string fileName = "ToLuaAutoRegister")
        {

            StringBuilder sb = new StringBuilder();
            string strAutoRegister = @"//this source code was auto-generated by tolua#, do not modify it
using System;
using UnityEngine;

namespace LuaInterface.ObjectWrap
{
    public class ToLua_Gen_Initer_Register__
    {
        public static void Init(LuaState L)
        {
            LuaBinder.Bind(L);
        }

        public ToLua_Gen_Initer_Register__()
        {
            DelegateGenFactory.Init();
            LuaState.AddIniter(Init);
        }

    } //end class ToLua_Gen_Initer_Register__
} //end namespace LuaInterface.ObjectWrap

namespace LuaInterface
{
    public partial class ObjectTranslator
    {
        static LuaInterface.ObjectWrap.ToLua_Gen_Initer_Register__ s_gen_reg_dumb_obj = new LuaInterface.ObjectWrap.ToLua_Gen_Initer_Register__();
        static LuaInterface.ObjectWrap.ToLua_Gen_Initer_Register__ gen_reg_dumb_obj {get{return s_gen_reg_dumb_obj;}}
    } //end class ObjectTranslator
} //end namespace LuaInterface";
            sb.Append(strAutoRegister);

            var file = $"{saveDir}/{fileName}.cs";
            using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
            {
                textWriter.Write(sb.ToString());
                textWriter.Flush();
                textWriter.Close();
            }

        }

        public static void GenAutoGenConfig(string saveDir, string fileName = "ToLuaAutoGenConfig")
        {
            //ToLua相对路径
            var toLuaRootPath = EditorTools.AbsolutePathToAssetPath(ToLuaPathConfig.RootPath, false);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//this source code was auto-generated by tolua#, do not modify it");
            sb.AppendLine("using System.IO;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("namespace LuaInterface.ObjectWrap.Config");
            sb.AppendLine("{");

            sb.AppendLine("public static class ToLuaAutoGenConfig");
            sb.AppendLine("{");
            sb.AppendLineFormat("\tprivate static string mToLuaRootPath = Path.Combine(Application.dataPath, \"{0}\");", toLuaRootPath);
            sb.AppendLine("\tpublic static string ToLuaRootPath");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\tget {");
            sb.AppendLine("\t\t\treturn mToLuaRootPath;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("} //end class ToLuaConfig");

            sb.AppendLine("} //end namespace LuaInterface.ObjectWrap.Config");

            var file = $"{saveDir}/{fileName}.cs";
            using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
            {
                textWriter.Write(sb.ToString());
                textWriter.Flush();
                textWriter.Close();
            }

        }


        //=================================== 生成委托 ==================================
        static void AutoAddBaseType(BindType bt, bool beDropBaseType)
        {
            Type t = bt.baseType;

            if (t == null)
            {
                return;
            }

            if (t.IsInterface)
            {
                Debugger.LogWarning("{0} has a base type {1} is Interface, use SetBaseType to jump it", bt.name, t.FullName);
                bt.baseType = t.BaseType;
            }
            else if (Generator.IsTypeInBlackList(t))
            {
                Debugger.LogWarning("{0} has a base type {1} is a drop type", bt.name, t.FullName);
                bt.baseType = t.BaseType;
            }
            else
            {
                return;
            }

            AutoAddBaseType(bt, beDropBaseType);
        }

        public static void GenRegisterInfo(string nameSpace, StringBuilder sb, List<DelegateType> delegateList, List<DelegateType> wrappedDelegatesCache)
        {
            var notDynamicList = Generator.LuaCallCSharp
                .Where(bt => !bt.IsDynamic )
                .Where(bt => bt.nameSpace == nameSpace)
                .ToList();
            for (int i = 0; i < notDynamicList.Count; i++)
            {
                BindType dt = notDynamicList[i];
                sb.AppendLineFormat("\t\t{0}Wrap.Register(L);", dt.wrapName);
            }

            string funcName = null;
            for (int i = 0; i < delegateList.Count; i++)
            {
                DelegateType dt = delegateList[i];
                Type type = dt.type;
                if (ToLuaExport.GetNameSpace(type, out funcName) == nameSpace)
                {
                    string abr = ToLuaExport.ConvertToLibSign(funcName);
                    sb.AppendLineFormat("\t\tL.RegFunction(\"{0}\", new LuaCSFunction({1}));", abr, dt.name);
                    wrappedDelegatesCache.Add(dt);
                }
            }
        }

        static void GenPreLoadFunction(BindType bt, StringBuilder sb)
        {
            string funcName = "LuaOpen_" + bt.wrapName;

            sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
            sb.AppendLineFormat("\tstatic int {0}(IntPtr L)", funcName);
            sb.AppendLineEx("\t{");
            sb.AppendLineEx("\t\ttry");
            sb.AppendLineEx("\t\t{");
            sb.AppendLineEx("\t\t\tLuaState state = LuaState.Get(L);");
            sb.AppendLineFormat("\t\t\tstate.BeginPreModule(\"{0}\");", bt.nameSpace);
            sb.AppendLineFormat("\t\t\t{0}Wrap.Register(state);", bt.wrapName);
            sb.AppendLineFormat("\t\t\tint reference = state.GetMetaReference(typeof({0}));", bt.name);
            sb.AppendLineEx("\t\t\tstate.EndPreModule(L, reference);");
            sb.AppendLineEx("\t\t\treturn 1;");
            sb.AppendLineEx("\t\t}");
            sb.AppendLineEx("\t\tcatch(Exception e)");
            sb.AppendLineEx("\t\t{");
            sb.AppendLineEx("\t\t\treturn LuaDLL.toluaL_exception(L, e);");
            sb.AppendLineEx("\t\t}");
            sb.AppendLineEx("\t}");
        }

        static ToLuaTree<string> InitTree()
        {
            ToLuaTree<string> tree = new ToLuaTree<string>();
            ToLuaNode<string> root = tree.GetRoot();
            BindType[] list = Generator.LuaCallCSharp.ToArray();

            for (int i = 0; i < list.Length; i++)
            {
                string space = list[i].nameSpace;
                AddSpaceNameToTree(tree, root, space);
            }

            string str = null;

            foreach (var exportDelegate in Generator.CSharpCallLua)
            {
                string space = ToLuaExport.GetNameSpace(exportDelegate, out str);
                AddSpaceNameToTree(tree, root, space);
            }

            return tree;
        }

        static void AddSpaceNameToTree(ToLuaTree<string> tree, ToLuaNode<string> parent, string space)
        {
            if (space == null || space == string.Empty)
            {
                return;
            }

            string[] ns = space.Split(new char[] { '.' });

            for (int j = 0; j < ns.Length; j++)
            {
                List<ToLuaNode<string>> nodes = tree.Find((_t) => { return _t == ns[j]; }, j);

                if (nodes.Count == 0)
                {
                    ToLuaNode<string> node = new ToLuaNode<string>();
                    node.value = ns[j];
                    parent.childs.Add(node);
                    node.parent = parent;
                    node.layer = j;
                    parent = node;
                }
                else
                {
                    bool flag = false;
                    int index = 0;

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        int count = j;
                        int size = j;
                        ToLuaNode<string> nodecopy = nodes[i];

                        while (nodecopy.parent != null)
                        {
                            nodecopy = nodecopy.parent;
                            if (nodecopy.value != null && nodecopy.value == ns[--count])
                            {
                                size--;
                            }
                        }

                        if (size == 0)
                        {
                            index = i;
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        ToLuaNode<string> nnode = new ToLuaNode<string>();
                        nnode.value = ns[j];
                        nnode.layer = j;
                        nnode.parent = parent;
                        parent.childs.Add(nnode);
                        parent = nnode;
                    }
                    else
                    {
                        parent = nodes[index];
                    }
                }
            }
        }

        static string GetSpaceNameFromTree(ToLuaNode<string> node)
        {
            string name = node.value;

            while (node.parent != null && node.parent.value != null)
            {
                node = node.parent;
                name = node.value + "." + name;
            }

            return name;
        }

        public static Type[] GetCustomTypeDelegates(List<BindType> LuaCallCSharp)
        {
            BindingFlags binding = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.Instance;
            List<Type> delegateList = new();
            BindType[] list = LuaCallCSharp.ToArray();

            for (int i = 0; i < list.Length; i++)
            {
                Type type = list[i].type;
                List<Type> delegateInType = new();
                delegateInType.AddRange(type.GetFields(BindingFlags.GetField | BindingFlags.SetField | binding)
                        .Select(f => f.FieldType));

                delegateInType.AddRange(type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | binding)
                        .Select(f => f.PropertyType));

                delegateInType.AddRange((type.IsInterface ? type.GetMethods() : type.GetMethods(BindingFlags.Instance | binding))
                        .Where(m => !m.IsGenericMethod)
                        .SelectMany(m => m.GetParameters())
                        .Select(pi => pi.ParameterType)
                        .Select(pt => pt.IsByRef ? pt.GetElementType() : pt));

                delegateInType = delegateInType
                        .Where(t => ToLuaExport.IsDelegateType(t))
                        .ToList();

                delegateList.AddRange(delegateInType);
            }

            return delegateList.Distinct().ToArray();
        }

        public static void GenLuaBinder(string saveDir, string fileName = "LuaBinder", string namespacePrefix = null)
        {

            ToLuaTree<string> tree = InitTree();
            StringBuilder sb = new StringBuilder();
            List<DelegateType> dtList = new List<DelegateType>();

            List<DelegateType> list = Generator.CSharpCallLua
                .Select(t => new DelegateType(t))
                .ToList();

            ToLuaNode<string> root = tree.GetRoot();

            sb.AppendLineEx("//this source code was auto-generated by tolua#, do not modify it");
            sb.AppendLineEx("using System;");
            sb.AppendLineEx("using UnityEngine;");
            sb.AppendLineEx("using LuaInterface.ObjectWrap;");
            sb.AppendLineEx();

            sb.AppendLineEx("namespace LuaInterface");
            sb.AppendLineEx("{");
            sb.AppendLineEx("public static class LuaBinder");
            sb.AppendLineEx("{");
            sb.AppendLineEx("\tpublic static void Bind(LuaState L)");
            sb.AppendLineEx("\t{");
            sb.AppendLineEx("\t\tfloat t = Time.realtimeSinceStartup;");
            sb.AppendLineEx("\t\tL.BeginModule(null);");
            if (namespacePrefix != null && namespacePrefix != "") {
                sb.AppendLineFormat("\t\tL.BeginModule(\"{0}\");", namespacePrefix);
            }


            GenRegisterInfo(null, sb, list, dtList);

            Action<ToLuaNode<string>> begin = (node) =>
            {
                if (node.value != null)
                {
                    sb.AppendLineFormat("\t\tL.BeginModule(\"{0}\");", node.value);
                    string space = GetSpaceNameFromTree(node);

                    GenRegisterInfo(space, sb, list, dtList);
                }
            };

            Action<ToLuaNode<string>> end = (node) =>
            {
                if (node.value != null)
                {
                    sb.AppendLineEx("\t\tL.EndModule();");
                }
            };

            tree.DepthFirstTraversal(begin, end, tree.GetRoot());
            sb.AppendLineEx("\t\tL.EndModule();");
            if (namespacePrefix != null && namespacePrefix != "") {
                sb.AppendLineEx("\t\tL.EndModule();");
            }

            var dynamicList = (from bTypeItem in Generator.LuaCallCSharp
                        where bTypeItem.IsDynamic
                        select bTypeItem).Distinct().ToArray();
            if (dynamicList.Length > 0)
            {
                sb.AppendLineEx("\t\tL.BeginPreLoad();");

                for (int i = 0; i < dynamicList.Length; i++)
                {
                    BindType bt = dynamicList[i];
                    sb.AppendLineFormat("\t\tL.AddPreLoad(\"{0}\", new LuaCSFunction(LuaOpen_{1}), typeof({0}));", bt.name, bt.wrapName);
                }

                sb.AppendLineEx("\t\tL.EndPreLoad();");
            }

            sb.AppendLineEx("\t\tDebugger.Log(\"Register lua type cost time: {0}\", Time.realtimeSinceStartup - t);");
            sb.AppendLineEx("\t}");

            for (int i = 0; i < dtList.Count; i++)
            {
                ToLuaExport.GenEventFunction(null, dtList[i].type, sb);
            }

            if (dynamicList.Length > 0)
            {
                for (int i = 0; i < dynamicList.Length; i++)
                {
                    BindType bt = dynamicList[i];
                    GenPreLoadFunction(bt, sb);
                }
            }

            sb.AppendLineEx("} //end class LuaBinder");
            sb.AppendLineEx("} //end namespace LuaInterface.ObjectWrap");

            string file = $"{saveDir}/{fileName}.cs";

            using (StreamWriter textWriter = new StreamWriter(file, false, Encoding.UTF8))
            {
                textWriter.Write(sb.ToString());
                textWriter.Flush();
                textWriter.Close();
            }

        }
    }
}
