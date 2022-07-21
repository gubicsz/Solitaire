using Solitaire.Helpers;
using UniRx;

namespace Solitaire.Models
{
    public class GamePopup : StateModel<Game.Popup>
    {
        public ReactiveCommand OptionsCommand { get; private set; }
        public ReactiveCommand LeaderboardCommand { get; private set; }
        public ReactiveCommand MatchCommand { get; private set; }

        public GamePopup(GameState gameState) : base(Game.Popup.None)
        {
            // Options popup can be opened from home and while playing
            OptionsCommand = new ReactiveCommand(gameState.State.Select(s => s == Game.State.Home || s == Game.State.Playing));
            OptionsCommand.Subscribe(_ =>
            {
                if (gameState.State.Value == Game.State.Playing)
                {
                    gameState.State.Value = Game.State.Paused;
                }

                State.Value = Game.Popup.Options;
            }).AddTo(this);

            // Leaderboard popup can be opened from home and while playing
            LeaderboardCommand = new ReactiveCommand(gameState.State.Select(s => s == Game.State.Home || s == Game.State.Playing));
            LeaderboardCommand.Subscribe(_ =>
            {
                if (gameState.State.Value == Game.State.Playing)
                {
                    gameState.State.Value = Game.State.Paused;
                }

                State.Value = Game.Popup.Leaderboard;
            }).AddTo(this);

            // Match popup can only be opened while playing
            MatchCommand = new ReactiveCommand(gameState.State.Select(s => s == Game.State.Playing));
            MatchCommand.Subscribe(_ =>
            {
                gameState.State.Value = Game.State.Paused;
                State.Value = Game.Popup.Match;
            }).AddTo(this);
        }
    }
}
