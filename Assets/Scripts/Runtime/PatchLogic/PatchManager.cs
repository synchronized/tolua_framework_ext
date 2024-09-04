using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniFramework.Event;
using ToLuaGameFramework;

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

    private PatchManager()
    {
        // 注册监听事件
        _eventGroup.AddListener<UserEventDefine.UserTryCheckPatchUpdate>(OnHandleEventMessage);
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is UserEventDefine.UserTryCheckPatchUpdate)
        {
            this.StartCheckUpdate();
        }
    }

    public void StartCheckUpdate()
    {
        ResManager.Instance.StartUpdateABOnStartup(
            (string title, float progress, bool isComplete) => {
                if (isComplete)
                {
                    //LuaManager.Instance.StartLua();
                    //MessageCenter.Dispatch(MsgEnum.RunLuaMain);

                    // 切换到主页面场景
                    //SceneEventDefine.ChangeToLoginScene.SendEventMessage();
                    SceneManager.LoadScene("Main");
                } else {
                    PatchEventDefine.DownloadProgressUpdate.SendEventMessage(title, progress);
                }
            },
            (string title, string err) => {
                PatchEventDefine.PatchdownloadFailed.SendEventMessage(title, err);
            });
    }
}
