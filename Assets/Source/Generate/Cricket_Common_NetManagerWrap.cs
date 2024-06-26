﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class Cricket_Common_NetManagerWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(Cricket.Common.NetManager), typeof(System.Object));
		L.RegFunction("DoUpdate", new LuaCSFunction(DoUpdate));
		L.RegFunction("DoClose", new LuaCSFunction(DoClose));
		L.RegFunction("SendConnect", new LuaCSFunction(SendConnect));
		L.RegFunction("CloseSocket", new LuaCSFunction(CloseSocket));
		L.RegFunction("IsConnected", new LuaCSFunction(IsConnected));
		L.RegFunction("SendMessage", new LuaCSFunction(SendMessage));
		L.RegFunction("AddEvent", new LuaCSFunction(AddEvent));
		L.RegFunction("New", new LuaCSFunction(_CreateCricket_Common_NetManager));
		L.RegFunction("__tostring", new LuaCSFunction(ToLua.op_ToString));
		L.RegVar("MsgDispatcher", new LuaCSFunction(get_MsgDispatcher), new LuaCSFunction(set_MsgDispatcher));
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateCricket_Common_NetManager(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				Cricket.Common.NetManager obj = new Cricket.Common.NetManager();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: Cricket.Common.NetManager.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DoUpdate(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
			obj.DoUpdate();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DoClose(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
			obj.DoClose();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SendConnect(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			int arg1 = (int)LuaDLL.luaL_checkinteger(L, 3);
			obj.SendConnect(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CloseSocket(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
			obj.CloseSocket();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int IsConnected(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
			bool o = obj.IsConnected();
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SendMessage(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<Cricket.Common.ByteBuffer>(L, 2))
			{
				Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
				Cricket.Common.ByteBuffer arg0 = (Cricket.Common.ByteBuffer)ToLua.ToObject(L, 2);
				obj.SendMessage(arg0);
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes<byte[]>(L, 2))
			{
				Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
				byte[] arg0 = ToLua.CheckByteBuffer(L, 2);
				obj.SendMessage(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: Cricket.Common.NetManager.SendMessage");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddEvent(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)ToLua.CheckObject<Cricket.Common.NetManager>(L, 1);
			byte[] arg0 = ToLua.CheckByteBuffer(L, 2);
			obj.AddEvent(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_MsgDispatcher(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)o;
			Cricket.Common.IMsgDispatcher<byte[]> ret = obj.MsgDispatcher;
			ToLua.PushObject(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index MsgDispatcher on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int set_MsgDispatcher(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			Cricket.Common.NetManager obj = (Cricket.Common.NetManager)o;
			Cricket.Common.IMsgDispatcher<byte[]> arg0 = (Cricket.Common.IMsgDispatcher<byte[]>)ToLua.CheckObject<Cricket.Common.IMsgDispatcher<byte[]>>(L, 2);
			obj.MsgDispatcher = arg0;
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index MsgDispatcher on a nil value");
		}
	}
}

