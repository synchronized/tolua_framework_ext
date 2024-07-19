
using System;
using System.Collections.Generic;

namespace LuaInterface.Editor.Config
{

    public static class GenConfig {

        [ToLuaLuaCallCSharp]
        //在这里添加你要导出注册到lua的类型列表
        public static BindType[] customTypeList =
        {
            _GT(typeof(System.Type)),
            _GT(typeof(System.Delegate)),
            _GT(typeof(System.Enum)),
            _GT(typeof(System.Object)),
            _GT(typeof(System.String)),
            _GT(typeof(System.Collections.IEnumerator)),
            _GT(typeof(UnityEngine.Object)),
            _GT(typeof(LuaInterface.EventObject)),
            _GT(typeof(LuaInterface.LuaMethod)),
            _GT(typeof(LuaInterface.LuaProperty)),
            _GT(typeof(LuaInterface.LuaField)),
            _GT(typeof(LuaInterface.LuaConstructor)),
        };

        [ToLuaCSharpCallLua]
        public static List<Type> customDelegateList = new List<Type>()
        {
        };

        //黑名单
        [ToLuaBlackList]
        public static List<List<string>> BlackList = new List<List<string>>
        {
            new List<string>(){"System.Type", "MakeGenericSignatureType"},
            new List<string>(){"System.Type", "IsCollectible"},
            new List<string>(){"System.String", "Chars"},
        };

        //不需要导出或者无法导出的类型
        [ToLuaTypeBlackList]
        public static List<Type> dropType = new List<Type>()
        {
            typeof(System.ValueType),                                  //不需要
            typeof(System.Reflection.MemberInfo),
            typeof(System.Reflection.BindingFlags),

    #if UNITY_4_6 || UNITY_4_7
            typeof(UnityEngine.Motion),                         //很多平台只是空类
    #endif

    #if UNITY_5_3_OR_NEWER
            typeof(UnityEngine.CustomYieldInstruction),
    #endif
            typeof(UnityEngine.YieldInstruction),               //无需导出的类
            typeof(UnityEngine.WaitForEndOfFrame),              //内部支持
            typeof(UnityEngine.WaitForFixedUpdate),
            typeof(UnityEngine.WaitForSeconds),
            typeof(UnityEngine.Mathf),                          //lua层支持
            typeof(UnityEngine.Plane),
            typeof(UnityEngine.LayerMask),
            typeof(UnityEngine.Vector3),
            typeof(UnityEngine.Vector4),
            typeof(UnityEngine.Vector2),
            typeof(UnityEngine.Quaternion),
            typeof(UnityEngine.Ray),
            typeof(UnityEngine.Bounds),
            typeof(UnityEngine.Color),
            typeof(UnityEngine.Touch),
            typeof(UnityEngine.RaycastHit),
            typeof(UnityEngine.TouchPhase),

            //typeof(LuaInterface.LuaOutMetatable),               //手写支持
            typeof(LuaInterface.NullObject),
            typeof(LuaInterface.LuaFunction),
            typeof(LuaInterface.LuaTable),
            typeof(LuaInterface.LuaThread),
            typeof(LuaInterface.LuaByteBuffer),                 //只是类型标识符
        };

        internal static BindType _GT(Type t)
        {
            return new BindType(t);
        }
    }
}
