using QFramework;

using Cricket.Common;

namespace Cricket.Game 
{

    public class LoginCommand : AbstractCommand
    {

        private string m_Username;
        private string m_Password;

        public LoginCommand(string username, string password) {
            this.m_Username = username;
            this.m_Password = password;
        }

        protected override void OnExecute()
        {
            LogKit.I("LoginCommand username:{0}, password:{1}", m_Username, m_Password);

            IPlayerModel playerModel = this.GetModel<IPlayerModel>();
            playerModel.Username = this.m_Username;
            playerModel.Password = this.m_Password;

            this.GetUtility<INetManager<byte[]>>().SendConnect(AppConst.ipaddress, AppConst.port);
        }
    }

}
