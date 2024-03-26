using Cysharp.Threading.Tasks;
using Solitaire.Helpers;
using Solitaire.Services;
using UniRx;

namespace Solitaire.Models
{
    public class GameControls : DisposableEntity
    {
        public GameControls(
            Game game,
            GameState gameState,
            ICommandService commandService,
            IMovesService movesService
        )
        {
            var isPlayingSource = gameState.State.Select(s => s == Game.State.Playing);

            HomeCommand = new ReactiveCommand(
                gameState.State.Select(s => s == Game.State.Playing || s == Game.State.Win)
            );
            
            HomeCommand.Subscribe(_ => gameState.State.Value = Game.State.Home).AddTo(this);

            UndoCommand = new ReactiveCommand(
                isPlayingSource.CombineLatest(
                    commandService.CanUndo,
                    (isPlaying, canUndo) => isPlaying && canUndo
                )
            );
            
            UndoCommand
                .Subscribe(_ =>
                {
                    commandService.Undo();
                    movesService.Increment();
                })
                .AddTo(this);

            HintCommand = new AsyncReactiveCommand(isPlayingSource);
            
            HintCommand
                .Subscribe(_ => game.TryShowHintAsync().ToObservable().AsUnitObservable())
                .AddTo(this);
        }

        public ReactiveCommand HomeCommand { get; }
        public ReactiveCommand UndoCommand { get; }
        public AsyncReactiveCommand HintCommand { get; }
    }
}
