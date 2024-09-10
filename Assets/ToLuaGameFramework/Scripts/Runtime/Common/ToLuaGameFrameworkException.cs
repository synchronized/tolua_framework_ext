
namespace ToLuaGameFramework{

    [System.Serializable]
    public class ToLuaGameFrameworkException : System.Exception
    {
        public ToLuaGameFrameworkException() { }
        public ToLuaGameFrameworkException(string message) : base(message) { }
        public ToLuaGameFrameworkException(string message, System.Exception inner) : base(message, inner) { }
        protected ToLuaGameFrameworkException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
