using UniFramework.Event;

public class UserEventDefine
{
    /// <summary>
    /// 用户尝试再次初始化资源包
    /// </summary>
    public class UserTryCheckPatchUpdate : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new UserTryCheckPatchUpdate();
            UniEvent.SendMessage(msg);
        }
    }
}
