using System.IO;
using UniFramework.Event;

public class PatchEventDefine
{
    /// <summary>
    /// 更新失败
    /// </summary>
    public class PatchdownloadFailed : IEventMessage
    {
        public string file;
        public string error;
        public static void SendEventMessage(string file, string error)
        {
            var msg = new PatchdownloadFailed();
            msg.file = file;
            msg.error = error;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 补丁流程步骤改变
    /// </summary>
    public class PatchStatesChange : IEventMessage
    {
        public string Tips;

        public static void SendEventMessage(string tips)
        {
            var msg = new PatchStatesChange();
            msg.Tips = tips;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 下载进度更新
    /// </summary>
    public class DownloadProgressUpdate : IEventMessage
    {
        public string file;
        public float progress;

        public static void SendEventMessage(string file, float progress)
        {
            var msg = new DownloadProgressUpdate();
            msg.file = file;
            msg.progress = progress;
            UniEvent.SendMessage(msg);
        }
    }
}
