using Solitaire.Helpers;

namespace Solitaire.Models
{
    public class GameState : StateModel<Game.State>
    {
        public GameState()
            : base(Game.State.Home) { }
    }
}
