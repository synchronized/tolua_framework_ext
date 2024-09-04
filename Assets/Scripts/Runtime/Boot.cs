using UnityEngine;
using UniFramework.Event;
using ToLuaGameFramework;

namespace GameLogic.BootLogic
{

    public class Boot : MonoBehaviour
    {
        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public ResLoadMode resLoadMode = ResLoadMode.SimulateMode;

        void Awake()
        {
            GlobalManager.ResLoadMode = resLoadMode;
            GlobalManager.Behaviour = this;

            Debug.Log($"资源加载模式运行模式：{resLoadMode}");
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
        }
        void Start()
        {
            // 初始化事件系统
            UniEvent.Initalize();

            LuaManager.Instance.Initalize(this);

            // 加载更新页面
            var patchPrefabs = Resources.Load<GameObject>("PatchWindow");
            var pitchWnd = Instantiate(patchPrefabs);
            pitchWnd.AddComponent<PatchWindow>();

            PatchManager.Instance.StartCheckUpdate();
        }

        void Update()
        {
            GameClient.Network.NetManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
            ResManager.Instance.DoUpdate();
        }

        void OnDestroy()
        {
            GameClient.Network.NetManager.Shutdown();
        }
    }
}
