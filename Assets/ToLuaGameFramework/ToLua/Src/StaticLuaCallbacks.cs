/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

namespace LuaInterface
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class StaticLuaCallbacks
    {
        static readonly Il2cppType il2cpp = new Il2cppType();
        static Type TypeOfList = typeof(List<>);

        public StaticLuaCallbacks()
        {
        }

        internal static bool Array_GetPrimitiveValue(IntPtr L, Il2cppType il, Array obj, Type t, int index)
        {
            bool flag = true;        

            if (t == il.TypeOfFloat)
            {            
                float[] array = (float[])obj;
                float ret = array[index];
                LuaDLL.lua_pushnumber(L, ret);
            }
            else if (t == il.TypeOfInt)
            {
                int[] array = (int[])obj;
                int ret = array[index];
                LuaDLL.lua_pushinteger(L, ret);
            }
            else if (t == il.TypeOfDouble)
            {
                double[] array = (double[])obj;
                double ret = array[index];
                LuaDLL.lua_pushnumber(L, ret);
            }
            else if (t == il.TypeOfBool)
            {
                bool[] array = (bool[])obj;
                bool ret = array[index];
                LuaDLL.lua_pushboolean(L, ret);
            }
            else if (t == il.TypeOfLong)
            {
                long[] array = (long[])obj;
                long ret = array[index];
                LuaDLL.tolua_pushint64(L, ret);
            }
            else if (t == il.TypeOfULong)
            {
                ulong[] array = (ulong[])obj;
                ulong ret = array[index];
                LuaDLL.tolua_pushuint64(L, ret);
            }
            else if (t == il.TypeOfSByte)
            {
                sbyte[] array = (sbyte[])obj;
                sbyte ret = array[index];
                LuaDLL.lua_pushinteger(L, ret);
            }
            else if (t == il.TypeOfByte)
            {
                byte[] array = (byte[])obj;
                byte ret = array[index];
                LuaDLL.lua_pushinteger(L, ret);
            }
            else if (t == il.TypeOfShort)
            {
                short[] array = (short[])obj;
                short ret = array[index];
                LuaDLL.lua_pushinteger(L, ret);
            }
            else if (t == il.TypeOfUShort)
            {
                ushort[] array = (ushort[])obj;
                ushort ret = array[index];
                LuaDLL.lua_pushinteger(L, ret);
            }
            else if (t == il.TypeOfChar)
            {
                char[] array = (char[])obj;
                char ret = array[index];
                LuaDLL.lua_pushinteger(L, ret);
            }
            else if (t == il.TypeOfUInt)
            {
                uint[] array = (uint[])obj;
                uint ret = array[index];
                LuaDLL.lua_pushinteger(L, ret);
            }
            else
            {
                flag = false;
            }

            return flag;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int Array_GetItem(IntPtr L)
        {
            try
            {
                Il2cppType il = il2cpp;
                Array obj = ToLua.ToObject(L, 1) as Array;            

                if (obj == null)
                {
                    throw new LuaException("trying to index an invalid object reference");
                }
                
                int index = (int)LuaDLL.lua_tointeger(L, 2);

                if (index >= obj.Length)
                {
                    throw new LuaException("array index out of bounds: " + index + " " + obj.Length);
                }

                Type t = obj.GetType().GetElementType();

                if (t.IsValueType)
                {
                    if (t.IsPrimitive)
                    {
                        if (Array_GetPrimitiveValue(L, il, obj, t, index))
                        {
                            return 1;
                        }
                    }
                    else if (t == il.TypeOfVector3)
                    {
                        Vector3[] array = (Vector3[])obj;
                        Vector3 ret = array[index];
                        ToLua.Push(L, ret);
                        return 1;
                    }
                    else if (t == il.TypeOfQuaternion)
                    {
                        Quaternion[] array = (Quaternion[])obj;
                        Quaternion ret = array[index];
                        ToLua.Push(L, ret);
                        return 1;
                    }
                    else if (t == il.TypeOfVector2)
                    {
                        Vector2[] array = (Vector2[])obj;
                        Vector2 ret = array[index];
                        ToLua.Push(L, ret);
                        return 1;
                    }
                    else if (t == il.TypeOfVector4)
                    {
                        Vector4[] array = (Vector4[])obj;
                        Vector4 ret = array[index];
                        ToLua.Push(L, ret);
                        return 1;
                    }
                    else if (t == il.TypeOfColor)
                    {
                        Color[] array = (Color[])obj;
                        Color ret = array[index];
                        ToLua.Push(L, ret);
                        return 1;
                    }
                }

                object val = obj.GetValue(index);
                ToLua.Push(L, val);
                return 1;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        internal static bool Array_SetPrimitiveValue(IntPtr L, Il2cppType il, object obj, Type t, int index)
        {
            bool flag = true;

            if (t == il.TypeOfFloat)
            {
                float[] array = (float[])obj;
                float val = (float)LuaDLL.luaL_checknumber(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfInt)
            {
                int[] array = (int[])obj;
                int val = (int)LuaDLL.luaL_checkinteger(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfDouble)
            {
                double[] array = (double[])obj;
                double val = LuaDLL.luaL_checknumber(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfBool)
            {
                bool[] array = (bool[])obj;
                bool val = LuaDLL.luaL_checkboolean(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfLong)
            {
                long[] array = (long[])obj;
                long val = LuaDLL.tolua_toint64(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfULong)
            {
                ulong[] array = (ulong[])obj;
                ulong val = LuaDLL.tolua_touint64(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfSByte)
            {
                sbyte[] array = (sbyte[])obj;
                sbyte val = (sbyte)LuaDLL.luaL_checkinteger(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfByte)
            {
                byte[] array = (byte[])obj;
                byte val = (byte)LuaDLL.luaL_checkinteger(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfShort)
            {
                short[] array = (short[])obj;
                short val = (short)LuaDLL.luaL_checkinteger(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfUShort)
            {
                ushort[] array = (ushort[])obj;
                ushort val = (ushort)LuaDLL.luaL_checkinteger(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfChar)
            {
                char[] array = (char[])obj;
                char val = (char)LuaDLL.luaL_checkinteger(L, 3);
                array[index] = val;
            }
            else if (t == il.TypeOfUInt)
            {
                uint[] array = (uint[])obj;
                uint val = (uint)LuaDLL.luaL_checkinteger(L, 3);
                array[index] = val;
            }
            else
            {
                flag = false;
            }

            return flag;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int Array_SetItem(IntPtr L)
        {
            try
            {
                Il2cppType il = il2cpp;
                Array obj = ToLua.ToObject(L, 1) as Array;

                if (obj == null)
                {
                    throw new LuaException("trying to index an invalid object reference");
                }

                int index = (int)LuaDLL.lua_tointeger(L, 2);            
                Type t = obj.GetType().GetElementType();

                if (t.IsValueType)
                {
                    if (t.IsPrimitive)
                    {
                        if (Array_SetPrimitiveValue(L, il, obj, t, index))
                        {
                            return 0;
                        }
                    }
                    else if (t == il.TypeOfVector3)
                    {
                        Vector3[] array = (Vector3[])obj;
                        Vector3 val = ToLua.ToVector3(L, 3);
                        array[index] = val;
                        return 0;
                    }
                    else if (t == il.TypeOfQuaternion)
                    {
                        Quaternion[] array = (Quaternion[])obj;
                        Quaternion val = ToLua.ToQuaternion(L, 3);
                        array[index] = val;
                        return 0;
                    }
                    else if (t == il.TypeOfVector2)
                    {
                        Vector2[] array = (Vector2[])obj;
                        Vector2 val = ToLua.ToVector2(L, 3);
                        array[index] = val;
                        return 0;
                    }
                    else if (t == il.TypeOfVector4)
                    {
                        Vector4[] array = (Vector4[])obj;
                        Vector4 val = ToLua.ToVector4(L, 3);
                        array[index] = val;
                        return 0;
                    }
                    else if (t == il.TypeOfColor)
                    {
                        Color[] array = (Color[])obj;
                        Color val = ToLua.ToColor(L, 3);
                        array[index] = val;
                        return 0;
                    }
                }

                if (!TypeChecker.CheckType(L, t, 3))
                {                                
                    return LuaDLL.luaL_typerror(L, 3, LuaMisc.GetTypeName(t));
                }

                object v = ToLua.CheckVarObject(L, 3, t);
                v = TypeChecker.ChangeType(v, t);
                obj.SetValue(v, index);
                return 0;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int Array_ToTable(IntPtr L)
        {
            try
            {
                Il2cppType il = il2cpp;
                Array obj = ToLua.ToObject(L, 1) as Array;

                if (obj == null)
                {
                    throw new LuaException("trying to index an invalid object reference");                
                }

                LuaDLL.lua_createtable(L, obj.Length, 0);
                Type t = obj.GetType().GetElementType();

                if (t.IsValueType)
                {
                    if (t.IsPrimitive)
                    {
                        if (t == il.TypeOfFloat)
                        {
                            float[] array = (float[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                float ret = array[i];
                                LuaDLL.lua_pushnumber(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfInt)
                        {
                            int[] array = (int[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                int ret = array[i];
                                LuaDLL.lua_pushinteger(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfDouble)
                        {
                            double[] array = (double[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                double ret = array[i];
                                LuaDLL.lua_pushnumber(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfBool)
                        {
                            bool[] array = (bool[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                bool ret = array[i];
                                LuaDLL.lua_pushboolean(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfLong)
                        {
                            long[] array = (long[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                long ret = array[i];
                                LuaDLL.tolua_pushint64(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfULong)
                        {
                            ulong[] array = (ulong[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                ulong ret = array[i];
                                LuaDLL.tolua_pushuint64(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfByte)
                        {
                            byte[] array = (byte[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                byte ret = array[i];
                                LuaDLL.lua_pushinteger(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfSByte)
                        {
                            sbyte[] array = (sbyte[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                sbyte ret = array[i];
                                LuaDLL.lua_pushinteger(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfChar)
                        {
                            char[] array = (char[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                char ret = array[i];
                                LuaDLL.lua_pushinteger(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfUInt)
                        {
                            uint[] array = (uint[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                uint ret = array[i];
                                LuaDLL.lua_pushinteger(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfShort)
                        {
                            short[] array = (short[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                short ret = array[i];
                                LuaDLL.lua_pushinteger(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                        else if (t == il.TypeOfUShort)
                        {
                            ushort[] array = (ushort[])obj;

                            for (int i = 0; i < array.Length; i++)
                            {
                                ushort ret = array[i];
                                LuaDLL.lua_pushinteger(L, ret);
                                LuaDLL.lua_rawseti(L, -2, i + 1);
                            }

                            return 1;
                        }
                    }
                    else if (t == il.TypeOfVector3)
                    {
                        Vector3[] array = (Vector3[])obj;

                        for (int i = 0; i < array.Length; i++)
                        {
                            Vector3 ret = array[i];
                            ToLua.Push(L, ret);
                            LuaDLL.lua_rawseti(L, -2, i + 1);
                        }

                        return 1;
                    }
                    else if (t == il.TypeOfQuaternion)
                    {
                        Quaternion[] array = (Quaternion[])obj;

                        for (int i = 0; i < array.Length; i++)
                        {
                            Quaternion ret = array[i];
                            ToLua.Push(L, ret);
                            LuaDLL.lua_rawseti(L, -2, i + 1);
                        }

                        return 1;
                    }
                    else if (t == il.TypeOfVector2)
                    {
                        Vector2[] array = (Vector2[])obj;

                        for (int i = 0; i < array.Length; i++)
                        {
                            Vector2 ret = array[i];
                            ToLua.Push(L, ret);
                            LuaDLL.lua_rawseti(L, -2, i + 1);
                        }

                        return 1;
                    }
                    else if (t == il.TypeOfVector4)
                    {
                        Vector4[] array = (Vector4[])obj;

                        for (int i = 0; i < array.Length; i++)
                        {
                            Vector4 ret = array[i];
                            ToLua.Push(L, ret);
                            LuaDLL.lua_rawseti(L, -2, i + 1);
                        }

                        return 1;
                    }
                    else if (t == il.TypeOfColor)
                    {
                        Color[] array = (Color[])obj;

                        for (int i = 0; i < array.Length; i++)
                        {
                            Color ret = array[i];
                            ToLua.Push(L, ret);
                            LuaDLL.lua_rawseti(L, -2, i + 1);
                        }

                        return 1;
                    }
                }

                for (int i = 0; i < obj.Length; i++)
                {
                    object val = obj.GetValue(i);
                    ToLua.Push(L, val);
                    LuaDLL.lua_rawseti(L, -2, i + 1);
                }

                return 1;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int List_GetItem(IntPtr L)
        {
            try
            {
                ToLua.CheckArgsCount(L, 2);
                IList obj = (IList)ToLua.CheckGenericObject(L, 1, TypeOfList);
                int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
                object o = obj[arg0];
                //object o = LuaMethodCache.CallSingleMethod("get_Item", obj, arg0);
                ToLua.Push(L, o);			
                return 1;
            }
            catch(Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int List_SetItem(IntPtr L)
        {
            try
            {
                ToLua.CheckArgsCount(L, 3);
                Type argType = null;
                IList obj = (IList)ToLua.CheckGenericObject(L, 1, TypeOfList, out argType);            
                int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
                object arg1 = ToLua.CheckObject(L, 3, argType);
                obj[arg0] = arg1;
                //LuaMethodCache.CallSingleMethod("set_Item", obj, arg0, arg1);
                return 0;
            }
            catch(Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

    }
}