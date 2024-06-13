using QFramework;

namespace Cricket.Game {

    public interface IPlayerModel : IModel {
        string Username { get; set; }
        string Password { get; set; }

        string Acknumber { get; set; }
        string Clientkey { get; set; }
        string Secret { get; set; }
	    int LoginSession { get; set; }
	    int LoginSessionExpire { get; set; }
	    string Token { get; set; }
    }

    public class PlayerModel : AbstractModel, IPlayerModel
    {
        public string Username { get; set; } = "sunday";
        public string Password { get; set; } = "123456";

        public string Acknumber { get; set; } = "";
        public string Clientkey { get; set; } = "";
        public string Secret { get; set; } = "";
	    public int LoginSession { get; set; } = 0;
	    public int LoginSessionExpire { get; set; } = 0;
	    public string Token { get; set; } = "";

        protected override void OnInit()
        {
        }

    }

}
