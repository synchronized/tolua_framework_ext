/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace LuaInterface
{
	public enum LazyMemberTypes
	{
		Method,
		FieldGet,
		FieldSet,
		PropertyGet,
		PropertySet,
		Event,
	}

	public static partial class Utils
	{

#if (UNITY_WSA && !ENABLE_IL2CPP) && !UNITY_EDITOR
        public static List<Assembly> _assemblies;
        public static List<Assembly> GetAssemblies()
        {
            if (_assemblies == null)
            {
                System.Threading.Tasks.Task t = new System.Threading.Tasks.Task(() =>
                {
                    _assemblies = GetAssemblyList().Result;
                });
                t.Start();
                t.Wait();
            }
            return _assemblies;
            
        }
        public static async System.Threading.Tasks.Task<List<Assembly>> GetAssemblyList()
        {
            List<Assembly> assemblies = new List<Assembly>();
            //return assemblies;
            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            if (files == null)
                return assemblies;

            foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe"))
            {
                try
                {
                    assemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            }
            return assemblies;
        }
        public static IEnumerable<Type> GetAllTypes(bool exclude_generic_definition = true)
        {
            var assemblies = GetAssemblies();
            return from assembly in assemblies
                   where !(assembly.IsDynamic)
                   from type in assembly.GetTypes()
                   where exclude_generic_definition ? !type.GetTypeInfo().IsGenericTypeDefinition : true
                   select type;
        }
#else
		public static List<Type> GetAllTypes(bool exclude_generic_definition = true)
		{
			List<Type> allTypes = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				try
				{
#if UNITY_EDITOR && !NET_STANDARD_2_0
					if (!(assemblies[i].ManifestModule is System.Reflection.Emit.ModuleBuilder))
					{
#endif
						allTypes.AddRange(assemblies[i].GetTypes()
						.Where(type => exclude_generic_definition ? !type.IsGenericTypeDefinition() : true)
						);
#if UNITY_EDITOR && !NET_STANDARD_2_0
					}
#endif
				}
				catch (Exception)
				{
				}
			}

			return allTypes;
		}
#endif

		public static bool IsParamsMatch(MethodInfo delegateMethod, MethodInfo bridgeMethod)
		{
			if (delegateMethod == null || bridgeMethod == null)
			{
				return false;
			}
			if (delegateMethod.ReturnType != bridgeMethod.ReturnType)
			{
				return false;
			}
			ParameterInfo[] delegateParams = delegateMethod.GetParameters();
			ParameterInfo[] bridgeParams = bridgeMethod.GetParameters();
			if (delegateParams.Length != bridgeParams.Length)
			{
				return false;
			}

			for (int i = 0; i < delegateParams.Length; i++)
			{
				if (delegateParams[i].ParameterType != bridgeParams[i].ParameterType || delegateParams[i].IsOut != bridgeParams[i].IsOut)
				{
					return false;
				}
			}

            var lastPos = delegateParams.Length - 1;
            return lastPos < 0 || delegateParams[lastPos].IsDefined(typeof(ParamArrayAttribute), false) == bridgeParams[lastPos].IsDefined(typeof(ParamArrayAttribute), false);
		}

		public static bool IsSupportedMethod(MethodInfo method)
		{
			if (!method.ContainsGenericParameters)
				return true;
			var methodParameters = method.GetParameters();
			var returnType = method.ReturnType;
			var hasValidGenericParameter = false;
			var returnTypeValid = !returnType.IsGenericParameter;
			for (var i = 0; i < methodParameters.Length; i++)
			{
				var parameterType = methodParameters[i].ParameterType;
				if (parameterType.IsGenericParameter)
				{
					var parameterConstraints = parameterType.GetGenericParameterConstraints();
					if (parameterConstraints.Length == 0) return false;
					foreach (var parameterConstraint in parameterConstraints)
					{
						if (!parameterConstraint.IsClass() || (parameterConstraint == typeof(ValueType)))
							return false;
					}
					hasValidGenericParameter = true;
					if (!returnTypeValid)
					{
						if (parameterType == returnType)
						{
							returnTypeValid = true;
						}
					}
				}
			}
			return hasValidGenericParameter && returnTypeValid;
		}

		public static MethodInfo MakeGenericMethodWithConstraints(MethodInfo method)
		{
			try
			{
				var genericArguments = method.GetGenericArguments();
				var constraintedArgumentTypes = new Type[genericArguments.Length];
				for (var i = 0; i < genericArguments.Length; i++)
				{
					var argumentType = genericArguments[i];
					var parameterConstraints = argumentType.GetGenericParameterConstraints();
					constraintedArgumentTypes[i] = parameterConstraints[0];
				}
				return method.MakeGenericMethod(constraintedArgumentTypes);
			}
			catch (Exception)
			{
				return null;
			}
		}

		private static Type getExtendedType(MethodInfo method)
		{
			var type = method.GetParameters()[0].ParameterType;
			if (!type.IsGenericParameter)
				return type;
			var parameterConstraints = type.GetGenericParameterConstraints();
			if (parameterConstraints.Length == 0)
				throw new InvalidOperationException();

			var firstParameterConstraint = parameterConstraints[0];
			if (!firstParameterConstraint.IsClass())
				throw new InvalidOperationException();
			return firstParameterConstraint;
		}

		public static bool IsStaticPInvokeCSFunction(LuaCSFunction csFunction)
		{
#if UNITY_WSA && !UNITY_EDITOR
            return csFunction.GetMethodInfo().IsStatic && csFunction.GetMethodInfo().GetCustomAttribute<MonoPInvokeCallbackAttribute>() != null;
#else
			return csFunction.Method.IsStatic && Attribute.IsDefined(csFunction.Method, typeof(MonoPInvokeCallbackAttribute));
#endif
		}

		public static bool IsPublic(Type type)
		{
			if (type.IsNested)
			{
				if (!type.IsNestedPublic()) return false;
				return IsPublic(type.DeclaringType);
			}
			if (type.IsGenericType())
			{
				var gas = type.GetGenericArguments();
				for (int i = 0; i < gas.Length; i++)
				{
					if (!IsPublic(gas[i]))
					{
						return false;
					}
				}
			}
			return type.IsPublic();
		}
	}
}
