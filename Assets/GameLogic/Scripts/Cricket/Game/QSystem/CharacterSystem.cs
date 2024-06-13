using System;
using QFramework;

using Cricket.Common;

namespace Cricket.Game {

    public interface ICharacterSystem : ISystem {

    }

    public class CharacterSystem : AbstractSystem, ICharacterSystem
    {
        protected override void OnInit()
        {

            var dispatcher = this.GetUtility<IMsgDispatcher<byte[]>>();
        }

    }

}
