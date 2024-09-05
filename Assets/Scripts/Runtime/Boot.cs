using UnityEngine;
using UniFramework.Event;
using YooAsset;
using ToLuaGameFramework;
using Cysharp.Threading.Tasks;

namespace GameLogic.BootLogic
{

    public class Boot : MonoBehaviour
    {
        /// <summary>
        /// 资源系统运行模式
        /// </summary>
        public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

        private EDefaultBuildPipeline BuildPipeline = EDefaultBuildPipeline.ScriptableBuildPipeline;

        void Awake()
        {
            GlobalManager.ResLoadMode = PlayMode == EPlayMode.EditorSimulateMode
                    ? ResLoadMode.SimulateMode : ResLoadMode.NormalMode;
            GlobalManager.Behaviour = this;
            GlobalManager.DefaultPackage = "DefaultPackage";


            Debug.Log($"资源加载模式运行模式：{PlayMode}");
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
        }
        async UniTask Start()
        {

            PatchManager.Instance.Behaviour = this;

            // 初始化事件系统
            UniEvent.Initalize();

            LuaManager.Instance.Initalize(this);

            // 初始化资源系统
            YooAssets.Initialize();

            // 加载更新页面
            var patchPrefabs = Resources.Load<GameObject>("PatchWindow");
            var pitchWnd = Instantiate(patchPrefabs);
            pitchWnd.AddComponent<PatchWindow>();

            // 开始补丁更新流程
            PatchOperation operation = new PatchOperation(GlobalManager.DefaultPackage, BuildPipeline.ToString(), PlayMode);
            YooAssets.StartOperation(operation);
            await operation;

            // 设置默认的资源包
            var gamePackage = YooAssets.GetPackage(GlobalManager.DefaultPackage);
            YooAssets.SetDefaultPackage(gamePackage);

            if (GlobalManager.ResLoadMode == ResLoadMode.NormalMode) {
                YooAssetLuaResLoader.AddLoader(); //增加LuaLoader
            }

            // 切换到主页面场景
            SceneEventDefine.ChangeToMainScene.SendEventMessage();
        }

        void Update()
        {
            GameClient.Network.NetManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        void OnDestroy()
        {
            GameClient.Network.NetManager.Shutdown();
        }
    }
}
