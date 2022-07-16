using Solitaire.Helpers;
using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Presenters
{
    [RequireComponent(typeof(Canvas))]
    public class OrientationStatePresenter : StateReactor<Orientation>
    {
        [Inject] OrientationState _orientationState;

        Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        protected override StateModel<Orientation> Model => _orientationState;

        protected override void SetVisibility(bool isVisible)
        {
            _canvas.enabled = isVisible;
        }
    }
}
