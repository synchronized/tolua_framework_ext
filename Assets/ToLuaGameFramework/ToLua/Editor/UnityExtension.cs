using System.Collections.Generic;
using System.Text;

namespace LuaInterface.Editor
{
    public static class UnityExtension
    {
        public static StringBuilder AppendLineFormat(this StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(format, args).AppendLine();
            return builder;
        }

        public static HashSet<T> AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> e) {
            foreach (var i in e) {
                hashSet.Add(i);
            }
            return hashSet;
        }
    }
}