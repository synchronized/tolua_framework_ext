using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Event;
using TMPro;

public class PatchWindow : MonoBehaviour
{

    private class ProgressWindow
    {
        private Transform _myTransform;
        private Slider _slider; //进度条
        private TMP_Text _txtTitle; //标题
        private TMP_Text _txtTips; //进度条上方说明
        private TMP_Text _txtProgress; //进度条里面说明X%

        private bool _progressDone; //是否结束
        private float _targetProgress; //目标进度
        private string _targetProgressText; //目标进度提示信息
        private float _smoothSpeed; //当前进度到达目标进度的速度

        public Action OnComplete;

        public ProgressWindow(Transform progressTransform)
        {
            _myTransform = progressTransform;

            _slider = _myTransform.Find("UIWindow/Slider").GetComponent<Slider>();
            _txtTitle = _myTransform.Find("UIWindow/txtTitle").GetComponent<TMP_Text>();
            _txtTips = _myTransform.Find("UIWindow/txtTips").GetComponent<TMP_Text>();
            _txtProgress = _myTransform.Find("UIWindow/Slider/txtProgress").GetComponent<TMP_Text>();

            Reset();
        }

        internal void Reset()
        {
            _progressDone = false;
            _targetProgress = 0;
            _targetProgressText = null;
            _smoothSpeed = 1f;
            _internalSetProgress(0);
        }

        internal void SetTitle(string title)
        {
            _txtTitle.text = title;
        }

        internal void SetTips(string tips)
        {
            _txtTips.text = tips;
        }

        internal void SetSmoothSpeed(float speed)
        {
            _smoothSpeed = speed <= 0 ? 1 : speed;
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        internal void SetProgress(float progress, string progressText = null)
        {
            _targetProgress = progress;
            _targetProgressText = progressText;
        }

        internal void Update(float deltaTime)
        {
            if (!_progressDone)
            {
                if (_slider.value >= 0.99)
                {
                    _progressDone = true;
                    var onComplete = OnComplete;
                    OnComplete = null;
                    onComplete?.Invoke();
                }
                else if (_targetProgress > _slider.value)
                {
                    var progress = _targetProgress;
                    progress = _slider.value + progress*deltaTime/_smoothSpeed;
                    _internalSetProgress(progress);
                }
            }
        }

        private void _internalSetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            var progressText = _targetProgressText ?? string.Format("{0:0.0}%", progress*100);
            _slider.value = progress;
            _txtProgress.text = progressText;
        }
    }

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
            _content = cloneObject.transform.Find("txtContent").GetComponent<TMP_Text>();
            _btnOK = cloneObject.transform.Find("btnOk").GetComponent<Button>();
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

    private ProgressWindow _progressWindow;

    void Awake()
    {
        _progressWindow = new ProgressWindow(transform);
        _progressWindow.SetTitle("资源更新");
        _progressWindow.SetTips("Initializing the game world !");
        _progressWindow.OnComplete += PatchEventDefine.PatchProgressComplete.SendEventMessage;

        _messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
        _messageBoxObj.SetActive(false);

        _eventGroup.AddListener<PatchEventDefine.InitializeFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.PatchStatesChange>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.FoundUpdateFiles>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.DownloadProgressUpdate>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.PackageVersionUpdateFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.PatchManifestUpdateFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.WebFileDownloadFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<PatchEventDefine.PatchUpdaterDone>(OnHandleEventMessage);
    }

    void Update()
    {
        _progressWindow?.Update(Time.deltaTime);
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
        if (message is PatchEventDefine.InitializeFailed)
        {
            System.Action callback = () =>
            {
                UserEventDefine.UserTryInitialize.SendEventMessage();
            };
            ShowMessageBox($"Failed to initialize package !", callback);
        }
        else if (message is PatchEventDefine.PatchStatesChange)
        {
            var msg = message as PatchEventDefine.PatchStatesChange;
            _progressWindow.SetTips(msg.Tips);
        }
        else if (message is PatchEventDefine.FoundUpdateFiles)
        {
            var msg = message as PatchEventDefine.FoundUpdateFiles;
            System.Action callback = () =>
            {
                _progressWindow.SetTips("Begin download the update patch files");
                UserEventDefine.UserBeginDownloadWebFiles.SendEventMessage();
            };
            float sizeMB = msg.TotalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            string totalSizeMB = sizeMB.ToString("f1");
            ShowMessageBox($"Found update patch files, Total count {msg.TotalCount} Total szie {totalSizeMB}MB", callback);
        }
        else if (message is PatchEventDefine.DownloadProgressUpdate)
        {
            var msg = message as PatchEventDefine.DownloadProgressUpdate;
            var progress = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
            string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
            string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
            var progressText = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
            _progressWindow.SetProgress(progress, progressText);
        }
        else if (message is PatchEventDefine.PackageVersionUpdateFailed)
        {
            System.Action callback = () =>
            {
                UserEventDefine.UserTryUpdatePackageVersion.SendEventMessage();
            };
            ShowMessageBox($"Failed to update static version, please check the network status.", callback);
        }
        else if (message is PatchEventDefine.PatchManifestUpdateFailed)
        {
            System.Action callback = () =>
            {
                UserEventDefine.UserTryUpdatePatchManifest.SendEventMessage();
            };
            ShowMessageBox($"Failed to update patch manifest, please check the network status.", callback);
        }
        else if (message is PatchEventDefine.WebFileDownloadFailed)
        {
            var msg = message as PatchEventDefine.WebFileDownloadFailed;
            System.Action callback = () =>
            {
                UserEventDefine.UserTryDownloadWebFiles.SendEventMessage();
            };
            ShowMessageBox($"Failed to download file : {msg.FileName}", callback);
        }
        else if (message is PatchEventDefine.PatchUpdaterDone)
        {
            _progressWindow.SetProgress(1);
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
