
using LuaInterface;
using UnityEngine;

namespace ToLuaGameFramework
{
    public enum ResLoadMode{
        SimulateMode, //模拟模式(仅在编辑器模式下可用)
        NormalMode, //正常模式
    }

    public static class GlobalManager
    {
        private static ResLoadMode m_ResLoadMode = ResLoadMode.SimulateMode;
        internal static ResLoadMode ResLoadMode {
            get {
                return m_ResLoadMode;
            }
            set {
                m_ResLoadMode = value;
                if (m_ResLoadMode == ResLoadMode.SimulateMode) {
                    LuaLoader.LoadMode = LuaLoadMode.SimulateMode;
                } else {
                    LuaLoader.LoadMode = LuaLoadMode.NormalMode;
                }
            }
        }

        public static MonoBehaviour Behaviour; //
        public static Transform MainCanvas; //主摄像机
        public static Transform MainCamera;

        public static string DefaultPackage = "";
    }
}
