using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Event;
using TMPro;

public class PatchWindow : MonoBehaviour
{
    /// <summary>
    /// 对话框封装类
    /// </summary>
    private class MessageBox
    {
        private GameObject _cloneObject;
        private TMP_Text _content;
        private Button _btnOK;
        private System.Action _clickOK;

        public bool ActiveSelf
        {
            get
            {
                return _cloneObject.activeSelf;
            }
        }

        public void Create(GameObject cloneObject)
        {
            _cloneObject = cloneObject;
            _content = cloneObject.transform.Find("txt_content").GetComponent<TMP_Text>();
            _btnOK = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
            _btnOK.onClick.AddListener(OnClickYes);
        }
        public void Show(string content, System.Action clickOK)
        {
            _content.text = content;
            _clickOK = clickOK;
            _cloneObject.SetActive(true);
            _cloneObject.transform.SetAsLastSibling();
        }
        public void Hide()
        {
            _content.text = string.Empty;
            _clickOK = null;
            _cloneObject.SetActive(false);
        }
        private void OnClickYes()
        {
            _clickOK?.Invoke();
            Hide();
        }
    }


    private readonly EventGroup _eventGroup = new EventGroup();
    private readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

    // UGUI相关
    private GameObject _messageBoxObj;
    private Slider _slider;
    private TMP_Text _tips;


    void Awake()
    {
        _slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
        _tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<TMP_Text>();
        _tips.text = "Initializing the game world !";
        _messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
        _messageBoxObj.SetActive(false);

        _eventGroup.AddListener<PatchEventDefine.PatchdownloadFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.PatchStatesChange>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.DownloadProgressUpdate>(OnHandleEventMessage);
    }
    void OnDestroy()
    {
        _eventGroup.RemoveAllListener();
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is PatchEventDefine.PatchdownloadFailed)
        {
            var msg = message as PatchEventDefine.PatchdownloadFailed;
            System.Action callback = () =>
            {
                UserEventDefine.UserTryCheckPatchUpdate.SendEventMessage();
            };
            ShowMessageBox($"Failed to download resource files! file:{msg.file} error:{msg.error}", callback);
        }
        else if (message is PatchEventDefine.PatchStatesChange)
        {
            var msg = message as PatchEventDefine.PatchStatesChange;
            _tips.text = msg.Tips;
        }
        else if (message is PatchEventDefine.DownloadProgressUpdate)
        {
            var msg = message as PatchEventDefine.DownloadProgressUpdate;
            var txtProgress = msg.progress.ToString("P0");
            _tips.text = $"{msg.file} {txtProgress}";
        }
        else
        {
            throw new System.NotImplementedException($"{message.GetType()}");
        }
    }

    /// <summary>
    /// 显示对话框
    /// </summary>
    private void ShowMessageBox(string content, System.Action ok)
    {
        // 尝试获取一个可用的对话框
        MessageBox msgBox = null;
        for (int i = 0; i < _msgBoxList.Count; i++)
        {
            var item = _msgBoxList[i];
            if (item.ActiveSelf == false)
            {
                msgBox = item;
                break;
            }
        }

        // 如果没有可用的对话框，则创建一个新的对话框
        if (msgBox == null)
        {
            msgBox = new MessageBox();
            var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
            msgBox.Create(cloneObject);
            _msgBoxList.Add(msgBox);
        }

        // 显示对话框
        msgBox.Show(content, ok);
    }
}
