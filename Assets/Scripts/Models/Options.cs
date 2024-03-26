using Solitaire.Helpers;
using Solitaire.Services;
using UniRx;

namespace Solitaire.Models
{
    public class Options : DisposableEntity
    {
        private readonly Game _game;
        private readonly GamePopup _gamePopup;
        private readonly GameState _gameState;

        public Options(
            Game game,
            GameState gameState,
            GamePopup gamePopup,
            IAudioService audioService
        )
        {
            _game = game;
            _gameState = gameState;
            _gamePopup = gamePopup;

            CloseCommand = new ReactiveCommand(
                gamePopup.State.Select(s => s == Game.Popup.Options)
            );
            CloseCommand.Subscribe(_ => Close()).AddTo(this);

            // Detect changes in options that require restart if the game has already started
            DrawThree
                .Where(_ => _game.HasStarted.Value)
                .Subscribe(_ => RestartNeeded.Value = true)
                .AddTo(this);

            // Control music and sfx
            AudioEnabled
                .Subscribe(audioEnabled =>
                {
                    audioService.SetVolume(audioEnabled ? 1f : 0f);

                    if (audioEnabled)
                        audioService.PlayMusic(Audio.Music, 0.3333f);
                    else
                        audioService.StopMusic();
                })
                .AddTo(this);
        }

        public BoolReactiveProperty DrawThree { get; } = new(false);
        public BoolReactiveProperty AudioEnabled { get; } = new(true);
        public BoolReactiveProperty RestartNeeded { get; } = new(false);
        public ReactiveCommand CloseCommand { get; }

        private void Close()
        {
            if (RestartNeeded.Value)
            {
                // Restart the game based on game state
                if (_gameState.State.Value == Game.State.Paused)
                {
                    _game.RestartCommand.Execute();
                }
                else
                {
                    _game.HasStarted.Value = false;
                    _gamePopup.State.Value = Game.Popup.None;
                }

                RestartNeeded.Value = false;
            }
            else
            {
                // Continue the game based on game state
                if (_gameState.State.Value == Game.State.Paused)
                    _game.ContinueCommand.Execute();
                else
                    _gamePopup.State.Value = Game.Popup.None;
            }
        }
    }
}
