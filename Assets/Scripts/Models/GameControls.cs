using Solitaire.Helpers;
using Solitaire.Services;
using System;
using UniRx;
using Cysharp.Threading.Tasks;

namespace Solitaire.Models
{
    public class GameControls : DisposableEntity
    {
        public ReactiveCommand HomeCommand { get; private set; }
        public ReactiveCommand UndoCommand { get; private set; }
        public AsyncReactiveCommand HintCommand { get; private set; }

        public GameControls(Game game, GameState gameState, ICommandService commandService, IMovesService movesService)
        {
            IObservable<bool> isPlayingSource = gameState.State.Select(s => s == Game.State.Playing);

            HomeCommand = new ReactiveCommand(gameState.State.Select(s => s == Game.State.Playing || s == Game.State.Win));
            HomeCommand.Subscribe(_ => gameState.State.Value = Game.State.Home).AddTo(this);

            UndoCommand = new ReactiveCommand(isPlayingSource.CombineLatest(commandService.CanUndo, (isPlaying, canUndo) => isPlaying && canUndo));
            UndoCommand.Subscribe(_ =>
            {
                commandService.Undo();
                movesService.Increment();
            }).AddTo(this);
            
            HintCommand = new AsyncReactiveCommand(isPlayingSource);
            HintCommand.Subscribe(_ => game.TryShowHintAsync().ToObservable().AsUnitObservable()).AddTo(this);
        }
    }
}
