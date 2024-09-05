using System.Collections;
using Cysharp.Threading.Tasks;
using UniFramework.Event;
using UnityEngine;
using YooAsset;

public class PatchManager
{
    private static PatchManager _instance;
    public static PatchManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PatchManager();
            return _instance;
        }
    }

    private readonly EventGroup _eventGroup = new EventGroup();

    /// <summary>
    /// 协程启动器
    /// </summary>
    public MonoBehaviour Behaviour;

    private PatchManager()
    {
        // 注册监听事件
        _eventGroup.AddListener<SceneEventDefine.ChangeToMainScene>(OnHandleEventMessage);
    }

    /// <summary>
    /// 开启一个协程
    /// </summary>
    public void StartCoroutine(IEnumerator enumerator)
    {
        Behaviour.StartCoroutine(enumerator);
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is SceneEventDefine.ChangeToMainScene)
        {
            UnityEngine.Debug.Log($"ChangeToMainScene");
            OpenMainScene().Forget();
        }
    }

    private async UniTask OpenMainScene()
    {
        SceneHandle handle = YooAssets.LoadSceneAsync("SceneMain");
        await handle;
        UnityEngine.Debug.Log($"handle.LastError:{handle.LastError}");
    }
}
