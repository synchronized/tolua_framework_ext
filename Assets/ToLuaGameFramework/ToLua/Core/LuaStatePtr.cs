using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.CompilerServices;

namespace LuaInterface
{
    public class LuaStatePtr
    {
        public IntPtr L;

        public int LuaUpValueIndex(int i)
        {
            return LuaDLL.lua_upvalueindex(i);
        }

        public IntPtr LuaNewState()
        {
            return LuaDLL.luaL_newstate();            
        }

        public void LuaClose()
        {
            LuaDLL.lua_close(L);
            L = IntPtr.Zero;
        }

        public IntPtr LuaNewThread()
        {
            return LuaDLL.lua_newthread(L);
        }        

        public IntPtr LuaAtPanic(IntPtr panic)
        {
            return LuaDLL.lua_atpanic(L, panic);
        }

        public int LuaGetTop()
        {
            return LuaDLL.lua_gettop(L);
        }

        public void LuaSetTop(int newTop)
        {
            LuaDLL.lua_settop(L, newTop);
        }

        public void LuaPushValue(int idx)
        {
            LuaDLL.lua_pushvalue(L, idx);
        }

        public void LuaRemove(int index)
        {
            LuaDLL.lua_remove(L, index);
        }

        public void LuaInsert(int idx)
        {
            LuaDLL.lua_insert(L, idx);
        }

        public bool LuaIsThread(int inx)
        {
            return LuaDLL.lua_isthread(L, inx);
        }

        public void LuaReplace(int idx)
        {
            LuaDLL.lua_replace(L, idx);
        }

        public bool LuaCheckStack(int args)
        {
            return LuaDLL.lua_checkstack(L, args) != 0;
        }

        public void LuaXMove(IntPtr to, int n)
        {
            LuaDLL.lua_xmove(L, to, n);
        }

        public bool LuaIsNumber(int idx)
        {
            return LuaDLL.lua_isnumber(L, idx) != 0;
        }

        public bool LuaIsInteger(int idx)
        {
            return LuaDLL.lua_isinteger(L, idx) != 0;
        }

        public bool LuaIsString(int index)
        {
            return LuaDLL.lua_isstring(L, index) != 0;
        }

        public bool LuaIsCFunction(int index)
        {
            return LuaDLL.lua_iscfunction(L, index) != 0;
        }

        public bool LuaIsUserData(int index)
        {
            return LuaDLL.lua_isuserdata(L, index) != 0;
        }

        public bool LuaIsNil(int n)
        {
            return LuaDLL.lua_isnil(L, n);
        }

        public LuaTypes LuaType(int index)
        {
            return LuaDLL.lua_type(L, index);
        }

        public string LuaTypeName(LuaTypes type)
        {
            return LuaDLL.lua_typename(L, type);
        }

        public string LuaTypeName(int idx)
        {
            return LuaDLL.luaL_typename(L, idx);
        }

        public bool LuaEqual(int idx1, int idx2)
        {
            return LuaDLL.lua_equal(L, idx1, idx2) != 0;
        }

        public bool LuaRawEqual(int idx1, int idx2)
        {
            return LuaDLL.lua_rawequal(L, idx1, idx2) != 0;
        }

        public bool LuaLessThan(int idx1, int idx2)
        {
            return LuaDLL.lua_lessthan(L, idx1, idx2) != 0;
        }

        public double LuaToNumber(int idx)
        {
            return LuaDLL.lua_tonumber(L, idx);
        }

        public long LuaToInteger(int idx)
        {
            return LuaDLL.lua_tointeger(L, idx);
        }

        public bool LuaToBoolean(int idx)
        {
            return LuaDLL.lua_toboolean(L, idx);
        }

        public string LuaToString(int index)
        {
            return LuaDLL.lua_tostring(L, index);
        }

        public IntPtr LuaToLString(int index, out int len)
        {
            return LuaDLL.tolua_tolstring(L, index, out len);
        }

        public IntPtr LuaToCFunction(int idx)
        {
            return LuaDLL.lua_tocfunction(L, idx);
        }

        public IntPtr LuaToUserData(int idx)
        {
            return LuaDLL.lua_touserdata(L, idx);
        }

        public IntPtr LuaToThread(int idx)
        {
            return LuaDLL.lua_tothread(L, idx);
        }

        public IntPtr LuaToPointer(int idx)
        {
            return LuaDLL.lua_topointer(L, idx);
        }

        public int LuaObjLen(int index)
        {
            return LuaDLL.tolua_objlen(L, index);
        }

        public void LuaPushNil()
        {
            LuaDLL.lua_pushnil(L);
        }

        public void LuaPushNumber(double number)
        {
            LuaDLL.lua_pushnumber(L, number);
        }

        public void LuaPushInteger(long n)
        {
            LuaDLL.lua_pushinteger(L, n);
        }

        public void LuaPushLString(byte[] str, int size)
        {
            LuaDLL.lua_pushlstring(L, str, size);
        }

        public void LuaPushString(string str)
        {
            LuaDLL.lua_pushstring(L, str);
        }

        public void LuaPushCClosure(IntPtr fn, int n)
        {
            LuaDLL.lua_pushcclosure(L, fn, n);
        }

        public void LuaPushBoolean(bool value)
        {
            LuaDLL.lua_pushboolean(L, value ? 1 : 0);
        }

        public void LuaPushLightUserData(IntPtr udata)
        {
            LuaDLL.lua_pushlightuserdata(L, udata);
        }

        public int LuaPushThread()
        {
            return LuaDLL.lua_pushthread(L);
        }

        public void LuaGetTable(int idx)
        {
            LuaDLL.lua_gettable(L, idx);
        }

        public void LuaGetField(int index, string key)
        {
            LuaDLL.lua_getfield(L, index, key);
        }

        public void LuaRawGet(int idx)
        {
            LuaDLL.lua_rawget(L, idx);
        }

        public void LuaRawGetI(int tableIndex, int index)
        {
            LuaDLL.lua_rawgeti(L, tableIndex, index);
        }

        public void LuaCreateTable(int narr = 0, int nec = 0)
        {
            LuaDLL.lua_createtable(L, narr, nec);
        }

        public IntPtr LuaNewUserData(int size)
        {
            return LuaDLL.tolua_newuserdata(L, size);
        }

        public int LuaGetMetaTable(int idx)
        {
            return LuaDLL.lua_getmetatable(L, idx);
        }

        public void LuaSetTable(int idx)
        {
            LuaDLL.lua_settable(L, idx);
        }

        public void LuaSetField(int idx, string key)
        {
            LuaDLL.lua_setfield(L, idx, key);
        }

        public void LuaRawSet(int idx)
        {
            LuaDLL.lua_rawset(L, idx);
        }

        public void LuaRawSetI(int tableIndex, int index)
        {
            LuaDLL.lua_rawseti(L, tableIndex, index);
        }

        public void LuaSetMetaTable(int objIndex)
        {
            LuaDLL.lua_setmetatable(L, objIndex);
        }

        public void LuaCall(int nArgs, int nResults)
        {
            LuaDLL.lua_call(L, nArgs, nResults);
        }

        public int LuaPCall(int nArgs, int nResults, int errfunc)
        {
            return LuaDLL.lua_pcall(L, nArgs, nResults, errfunc);
        }

		public void LuaCallK(int nArgs, int nResults, IntPtr ctx, LuaKFunction k)
		{
			LuaDLL.lua_callk(L, nArgs, nResults, ctx, k);
		}

		public void LuaPCallK(int nArgs, int nResults, int errfunc, IntPtr ctx, LuaKFunction k)
		{
			LuaDLL.lua_pcallk(L, nArgs, nResults, errfunc, ctx, k);
		}

		public int LuaYieldK(int nresults, IntPtr ctx, LuaKFunction k)
        {
            return LuaDLL.lua_yieldk(L, nresults, ctx, k);
        }

		public int LuaResumeThread(IntPtr thread, int narg)
        {
            return LuaDLL.lua_resume(thread, L, narg);
        }

        public int LuaStatus()
        {
            return LuaDLL.lua_status(L);
        }

        public int LuaGC(LuaGCOptions what, int data = 0)
        {
            return LuaDLL.lua_gc(L, what, data);
        }

        public bool LuaNext(int index)
        {
            return LuaDLL.lua_next(L, index) != 0;
        }

        public void LuaConcat(int n)
        {
            LuaDLL.lua_concat(L, n);
        }

        public void LuaPop(int amount)
        {
            LuaDLL.lua_pop(L, amount);
        }

        public void LuaNewTable()
        {
            LuaDLL.lua_createtable(L, 0 , 0);
        }

        public void LuaPushFunction(LuaCSFunction func)
        {
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            LuaDLL.lua_pushcclosure(L, fn, 0);
        }

        public bool lua_isfunction(int n)
        {
            return LuaDLL.lua_type(L, n) == LuaTypes.LUA_TFUNCTION;
        }

        public bool lua_istable(int n)
        {
            return LuaDLL.lua_type(L, n) == LuaTypes.LUA_TTABLE;
        }

        public bool lua_islightuserdata(int n)
        {
            return LuaDLL.lua_type(L, n) == LuaTypes.LUA_TLIGHTUSERDATA;
        }

        public bool lua_isnil(int n)
        {
            return LuaDLL.lua_type(L, n) == LuaTypes.LUA_TNIL;
        }

        public bool lua_isboolean(int n)
        {
            LuaTypes type = LuaDLL.lua_type(L, n);
            return type == LuaTypes.LUA_TBOOLEAN || type == LuaTypes.LUA_TNIL;
        }

        public bool lua_isthread(int n)
        {
            return LuaDLL.lua_type(L, n) == LuaTypes.LUA_TTHREAD;
        }

        public bool lua_isnone(int n)
        {
            return LuaDLL.lua_type(L, n) == LuaTypes.LUA_TNONE;
        }

        public bool lua_isnoneornil(int n)
        {
            return LuaDLL.lua_type(L, n) <= LuaTypes.LUA_TNIL;
        }

		public void lua_pushglobaltable()
		{
			LuaDLL.lua_pushglobaltable(L);
		}

		public void LuaRawGlobal(string name)
        {
            LuaDLL.lua_pushglobaltable(L);
            int top = LuaDLL.lua_gettop(L);
            LuaDLL.lua_pushstring(L, name);
            LuaDLL.lua_rawget(L, top);
            //弹出global table
            LuaDLL.lua_remove(L, top);	
        }

        public void LuaSetGlobal(string name)
        {
            LuaDLL.lua_setglobal(L, name);
        }

        public void LuaGetGlobal(string name)
        {
            LuaDLL.lua_getglobal(L, name);
        }

        public void LuaOpenLibs()
        {
            LuaDLL.luaL_openlibs(L);
        }

        public int AbsIndex(int i)
        {
            return (i > 0 || i <= LuaIndexes.LUA_REGISTRYINDEX) ? i : LuaDLL.lua_gettop(L) + i + 1;
        }

        public int LuaGetN(int i)
        {
            return LuaDLL.luaL_getn(L, i);
        }

        public double LuaCheckNumber(int stackPos)
        {
            return LuaDLL.luaL_checknumber(L, stackPos);
        }

        public long LuaCheckInteger(int idx)
        {
            return LuaDLL.luaL_checkinteger(L, idx);
        }

        public bool LuaCheckBoolean(int stackPos)
        {
            return LuaDLL.luaL_checkboolean(L, stackPos);
        }

        public string LuaCheckLString(int numArg, out int len)
        {
            return LuaDLL.luaL_checklstring(L, numArg, out len);
        }

        public int LuaLoadBuffer(byte[] buff, int size, string name)
        {
            return LuaDLL.luaL_loadbuffer(L, buff, size, name);
        }

        public IntPtr LuaFindTable(int idx, string fname, int szhint = 1)
        {
            return LuaDLL.luaL_findtable(L, idx, fname, szhint);
        }

        public int LuaTypeError(int stackPos, string tname, string t2 = null)
        {
            return LuaDLL.luaL_typerror(L, stackPos, tname, t2);
        }

        public bool LuaDoString(string chunk, string chunkName = "@LuaStatePtr.cs")
        {
            byte[] buffer = Encoding.UTF8.GetBytes(chunk);
            int status = LuaDLL.luaL_loadbuffer(L, buffer, buffer.Length, chunkName);

            if (status != 0)
            {
                return false;                
            }

            return LuaDLL.lua_pcall(L, 0, LuaDLL.LUA_MULTRET, 0) == 0;
            //return LuaDLL.luaL_dostring(L, chunk);
        }

        public bool LuaDoFile(string fileName)
        {
            int top = LuaGetTop();

            if (LuaDLL.luaL_dofile(L, fileName))
            {
                return true;
            }

            string err = LuaToString(-1);
            LuaSetTop(top);
            throw new LuaException(err, LuaException.GetLastError());
        }

        public void LuaGetMetaTable(string meta)
        {
            LuaDLL.luaL_getmetatable(L, meta);
        }

        public int LuaRef(int t)
        {
            return LuaDLL.luaL_ref(L, t);
        }

        public void LuaGetRef(int reference)
        {
            LuaDLL.lua_getref(L, reference);
        }

        public void LuaUnRef(int reference)
        {
            LuaDLL.lua_unref(L, reference);
        }

        public int LuaRequire(string fileName)
        {
#if UNITY_EDITOR
            string str = Path.GetExtension(fileName);

            if (str == ".lua")
            {
                throw new LuaException("Require not need file extension: " + str);
            }
#endif
            //fileName = fileName.Replace('/', '.');            
            return LuaDLL.tolua_require(L, fileName);
        }

        //适合Awake OnSendMsg使用
        public void ThrowLuaException(Exception e)
        {
            if (LuaException.InstantiateCount > 0 || LuaException.SendMsgCount > 0)
            {
                LuaDLL.toluaL_exception(LuaException.L, e);
            }
            else
            {
                throw e;
            }
        }

        public int ToLuaRef()
        {
            return LuaDLL.toluaL_ref(L);
        }

        public int LuaUpdate(float delta, float unscaled)
        {
            return LuaDLL.tolua_update(L, delta, unscaled);
        }

        public int LuaLateUpdate()
        {
            return LuaDLL.tolua_lateupdate(L);
        }

        public int LuaFixedUpdate(float fixedTime)
        {
            return LuaDLL.tolua_fixedupdate(L, fixedTime);
        }

        public void OpenToLuaLibs()
        {
            LuaDLL.tolua_openlibs(L);
        }

        public void ToLuaPushTraceback()
        {
            LuaDLL.tolua_pushtraceback(L);
        }

        public void ToLuaUnRef(int reference)
        {            
            LuaDLL.toluaL_unref(L, reference);
        }

        public int LuaGetStack(int level, ref Lua_Debug ar)
        {
            return LuaDLL.lua_getstack(L, level, ref ar);
        }   
           
        public int LuaGetInfo(string what, ref Lua_Debug ar)
        {
            return LuaDLL.lua_getinfo(L, what, ref ar);
        }
        
        public string LuaGetLocal(ref Lua_Debug ar, int n)
        {
            return LuaDLL.lua_getlocal(L, ref ar, n);
        }
        
        public string LuaSetLocal(ref Lua_Debug ar, int n)
        {
            return LuaDLL.lua_setlocal(L, ref ar, n);
        }
        
        public string LuaGetUpvalue(int funcindex, int n)
        {
            return LuaDLL.lua_getupvalue(L, funcindex, n);
        }
        
        public string LuaSetUpvalue(int funcindex, int n)
        {
            return LuaDLL.lua_setupvalue(L, funcindex, n);
        }
        
        public int LuaSetHook(LuaHookFunc func, int mask, int count)
        {
            return LuaDLL.lua_sethook(L, func, mask, count);
        }
        
        public LuaHookFunc LuaGetHook()
        {
            return LuaDLL.lua_gethook(L);
        }
        
        public  int LuaGetHookMask()
        {
            return LuaDLL.lua_gethookmask(L);
        }
        
        public int LuaGetHookCount()
        {
            return LuaDLL.lua_gethookcount(L);
        }
    }
}