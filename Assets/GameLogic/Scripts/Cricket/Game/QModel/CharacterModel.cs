using QFramework;

namespace Cricket.Game {

    public interface ICharacterModel : IModel {

    }

    public class CharacterModel : AbstractModel, ICharacterModel
    {
        protected override void OnInit()
        {
            //TODO
        }
    }

}
