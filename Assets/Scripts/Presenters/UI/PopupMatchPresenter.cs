using Solitaire.Models;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;

namespace Solitaire.Presenters
{
    public class PopupMatchPresenter : MonoBehaviour
    {
        [SerializeField] Button _buttonRestart;
        [SerializeField] Button _buttonNewMatch;
        [SerializeField] Button _buttonContinue;
        [SerializeField] RectTransform _panelRect;
        [SerializeField] VerticalLayoutGroup _verticalLayout;

        [Inject] Game _game;
        [Inject] OrientationState _orientation;

        RectTransform _rectRestart;
        RectTransform _rectNewMatch;
        RectTransform _rectContinue;

        private void Start()
        {
            _rectRestart = _buttonRestart.GetComponent<RectTransform>();
            _rectNewMatch = _buttonNewMatch.GetComponent<RectTransform>();
            _rectContinue = _buttonContinue.GetComponent<RectTransform>();

            // Bind commands
            _game.RestartCommand.BindTo(_buttonRestart).AddTo(this);
            _game.NewMatchCommand.BindTo(_buttonNewMatch).AddTo(this);
            _game.ContinueCommand.BindTo(_buttonContinue).AddTo(this);

            // Update layout on orientation change
            _orientation.State.Subscribe(orientation => AdjustLayout(orientation)).AddTo(this);
        }

        private void AdjustLayout(Orientation orientation)
        {
            bool isLandscape = orientation == Orientation.Landscape;

            _panelRect.offsetMin = isLandscape ? new Vector2(250, 150) : new Vector2(150, 250);
            _panelRect.offsetMax = isLandscape ? new Vector2(-250, -150) : new Vector2(-150, -250);

            // TODO: replace layout group with manual position calculation to optimize performance
            _verticalLayout.spacing = isLandscape ? 20 : 40;

            Vector2 size = _rectRestart.sizeDelta;
            size.y = isLandscape ? 70 : 140;
            _rectRestart.sizeDelta = size;
            _rectNewMatch.sizeDelta = size;
            _rectContinue.sizeDelta = size;
        }
    }
}
