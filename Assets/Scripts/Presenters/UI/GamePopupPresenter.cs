using Solitaire.Helpers;
using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Presenters
{
    public class GamePopupPresenter : StateReactor<Game.Popup>
    {
        [Inject] readonly GamePopup _gamePopup;

        Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        protected override StateModel<Game.Popup> Model => _gamePopup;

        protected override void SetVisibility(bool isVisible)
        {
            _canvas.enabled = isVisible;
        }
    }
}
