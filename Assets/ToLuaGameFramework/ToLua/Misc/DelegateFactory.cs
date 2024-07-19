using System;
using System.Collections.Generic;


namespace LuaInterface.ObjectWrap
{
}

namespace LuaInterface
{
	public class DelegateFactory
	{
		public delegate Delegate DelegateCreate(LuaFunction func, LuaTable self, bool flag);
		public static Dictionary<Type, DelegateCreate> dict = new Dictionary<Type, DelegateCreate>();

		public static void AddDelegateCreate(Type type, DelegateCreate create) {
			dict.Add(type, create);
		}

		public static bool TryGetDelegateCreate(Type type, out DelegateCreate create) {
			return dict.TryGetValue(type, out create);
		}

		public static void ClearDelegateCreate() {
			dict.Clear();
		}

        public static Delegate CreateDelegate(Type t, LuaFunction func = null)
        {
            DelegateCreate Create = null;

            if (!DelegateFactory.TryGetDelegateCreate(t, out Create))
            {
                throw new LuaException(string.Format("Delegate {0} not register", LuaMisc.GetTypeName(t)));
            }

            if (func != null)
            {
                LuaState state = func.GetLuaState();
                LuaDelegate target = state.GetLuaDelegate(func);

                if (target != null)
                {
                    return Delegate.CreateDelegate(t, target, target.method);
                }
                else
                {
                    Delegate d = Create(func, null, false);
                    target = d.Target as LuaDelegate;
                    state.AddLuaDelegate(target, func);
                    return d;
                }
            }

            return Create(null, null, false);
        }

        public static Delegate CreateDelegate(Type t, LuaFunction func, LuaTable self)
        {
            DelegateCreate Create = null;

            if (!DelegateFactory.TryGetDelegateCreate(t, out Create))
            {
                throw new LuaException(string.Format("Delegate {0} not register", LuaMisc.GetTypeName(t)));
            }

            if (func != null)
            {
                LuaState state = func.GetLuaState();
                LuaDelegate target = state.GetLuaDelegate(func, self);

                if (target != null)
                {
                    return Delegate.CreateDelegate(t, target, target.method);
                }
                else
                {
                    Delegate d = Create(func, self, true);
                    target = d.Target as LuaDelegate;
                    state.AddLuaDelegate(target, func, self);
                    return d;
                }
            }

            return Create(null, null, true);
        }
    
        public static Delegate RemoveDelegate(Delegate obj, LuaFunction func)
        {
            Delegate[] ds = obj.GetInvocationList();

            for (int i = 0; i < ds.Length; i++)
            {
                LuaDelegate ld = ds[i].Target as LuaDelegate;

                if (ld != null && ld.func == func)
                {
                    obj = Delegate.Remove(obj, ds[i]);
                    if (obj != null) obj.AddRef();
                    break;
                }
            }

            return obj;
        }

        public static Delegate RemoveDelegate(Delegate obj, Delegate dg)
        {
            LuaDelegate remove = dg.Target as LuaDelegate;

            if (remove == null)
            {
                obj = Delegate.Remove(obj, dg);
                return obj;
            }

            Delegate[] ds = obj.GetInvocationList();

            for (int i = 0; i < ds.Length; i++)
            {
                LuaDelegate ld = ds[i].Target as LuaDelegate;

                if (ld != null && ld == remove)
                {
                    obj = Delegate.Remove(obj, ds[i]);
                    if (obj != null) obj.AddRef();
                    break;
                }
            }

            return obj;
        }
	}
	//DelegateFactory.TryGetDelegateCreate
}