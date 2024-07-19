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

namespace LuaInterface
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type type)
        {
        }
    }

    public class NoToLuaAttribute : System.Attribute
    {
        public NoToLuaAttribute()
        {

        }
    }

    public class UseDefinedAttribute : System.Attribute
    {
        public UseDefinedAttribute()
        {

        }
    }

    public class OverrideDefinedAttribute: System.Attribute
    {
        public OverrideDefinedAttribute()
        {

        }
    }

    public sealed class LuaByteBufferAttribute : Attribute
    {
        public LuaByteBufferAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LuaRenameAttribute : Attribute
    {
        public string Name;
        public LuaRenameAttribute()
        {
        }
    }

    //如果你要生成Lua调用CSharp的代码，加这个标签
    public sealed class ToLuaLuaCallCSharpAttribute : Attribute
    {
    }

    //生成CSharp调用Lua，加这标签
    //[AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface)]
    public class ToLuaCSharpCallLuaAttribute : Attribute
    {
    }

    //不生成某个成员，加这标签
    public class ToLuaBlackListAttribute : Attribute
    {
    }

    //不生成某个类型，加这标签
    public class ToLuaTypeBlackListAttribute : Attribute
    {
    }

    //只能标注Dictionary<Type, List<string>>的field或者property
    public class DoNotGenAttribute : Attribute
    {
    }

    public class AdditionalPropertiesAttribute : Attribute
    {
    }
}
