using Solitaire.Helpers;
using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Presenters
{
    [RequireComponent(typeof(Canvas))]
    public class GameStatePresenter : StateReactor<Game.State>
    {
        [Inject] readonly GameState _gameState;

        Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        protected override StateModel<Game.State> Model => _gameState;

        protected override void SetVisibility(bool isVisible)
        {
            _canvas.enabled = isVisible;
        }
    }
}
