using System.Collections;
using System.Collections.Generic;

using QFramework;

using Cricket.Common;

namespace Cricket.Game
{
    public class GameArch : Architecture<GameArch>
    {

        protected override void Init()
        {
            //this.RegisterSystem<ILoginSystem>(new LoginSystem());
            //this.RegisterSystem<ICharacterSystem>(new CharacterSystem());

            //this.RegisterModel<IPlayerModel>(new PlayerModel());
            //this.RegisterModel<ICharacterModel>(new CharacterModel());

            var dispatcher = new MsgDispatcherPB();
            var netManager = new NetManager
            {
                MsgDispatcher = dispatcher
            };
            var msgSender = new MsgSenderPB
            {
                NetManager = netManager,
                MsgDispatcher = dispatcher
            };
            this.RegisterUtility<IMsgDispatcher<byte[]>>(dispatcher);
            this.RegisterUtility<INetManager<byte[]>>(netManager);
            this.RegisterUtility<IMsgSender<byte[], byte[]>>(msgSender);
        }

    }
}

