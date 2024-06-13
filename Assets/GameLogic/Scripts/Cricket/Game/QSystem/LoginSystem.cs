using System;

using QFramework;
using Google.Protobuf;

using Cricket.Common;
using Proto;

namespace Cricket.Game {

    public interface ILoginSystem : ISystem {

    }

    public class LoginSystem : AbstractSystem, ILoginSystem
    {

        private IPlayerModel m_PlayerModel;
        private IMsgSender<byte[], IMessage> m_MsgSender;

        protected override void OnInit()
        {
            m_PlayerModel = this.GetModel<IPlayerModel>();
            m_MsgSender = this.GetUtility<IMsgSender<byte[], IMessage>>();

            var dispatcher = this.GetUtility<IMsgDispatcher<byte[]>>();
            dispatcher.Register("res_acknowledgment", OnResAcknowledgment);
            dispatcher.Register("res_handshake", OnResHandshake);
        }

        private void OnResAcknowledgment(byte[] bytearray)
        {
            res_acknowledgment resp = res_acknowledgment.Parser.ParseFrom(bytearray);
            m_PlayerModel.Acknumber = resp.Acknumber;
            m_PlayerModel.Clientkey = ""; //Crypt.RandomKey(8); //crypt.randomkey()

            req_handshake req = new req_handshake() {
                //crypt.dhexchange(user.clientkey)
                ClientPub = m_PlayerModel.Clientkey, 
            };

            LogKit.I("Acknumber: {0}", BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(resp.Acknumber)));

            m_MsgSender.SendMessage("req_handshake", req, (bool result, int errCode) => {
                LogKit.I("req_handshake callback result: {0}, errCode: {1}", result, errCode);
            });
        }

        private void OnResHandshake(byte[] bytearray)
        {
            res_handshake resp  = res_handshake.Parser.ParseFrom(bytearray);
	        //user.secret = crypt.dhsecret(resp.secret, user.clientkey)
            m_PlayerModel.Secret = resp.Secret;

            LogKit.I("Secret: {0}", BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(resp.Secret)));
        }
    }

}
