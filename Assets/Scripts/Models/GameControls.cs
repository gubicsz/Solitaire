using Solitaire.Helpers;
using Solitaire.Services;
using System;
using UniRx;

namespace Solitaire.Models
{
    public class GameControls : DisposableEntity
    {
        public ReactiveCommand HomeCommand { get; private set; }
        public ReactiveCommand PauseCommand { get; private set; }
        public ReactiveCommand UndoCommand { get; private set; }
        public ReactiveCommand HintCommand { get; private set; }

        public GameControls(GameState gameState, CommandService commandService, MovesService movesService)
        {
            IObservable<bool> isPlayingSource = gameState.State.Select(s => s == Game.State.Playing);

            HomeCommand = new ReactiveCommand(gameState.State.Select(s => s == Game.State.Playing || s == Game.State.Win));
            HomeCommand.Subscribe(_ => gameState.State.Value = Game.State.Home).AddTo(this);

            PauseCommand = new ReactiveCommand(isPlayingSource);
            PauseCommand.Subscribe(_ => gameState.State.Value = Game.State.Paused).AddTo(this);

            UndoCommand = new ReactiveCommand(isPlayingSource.CombineLatest(commandService.CanUndo, (isPlaying, canUndo) => isPlaying && canUndo));
            UndoCommand.Subscribe(_ =>
            {
                commandService.UndoCommand();
                movesService.Increment();
            }).AddTo(this);

            HintCommand = new ReactiveCommand(isPlayingSource);
            HintCommand.Subscribe(_ => UnityEngine.Debug.Log("HINT")).AddTo(this);// todo
        }
    }
}
