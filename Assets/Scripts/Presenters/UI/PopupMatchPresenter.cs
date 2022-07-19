using Solitaire.Models;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Zenject;

namespace Solitaire.Presenters
{
    public class PopupMatchPresenter : OrientationAwarePresenter
    {
        [SerializeField] Button _buttonRestart;
        [SerializeField] Button _buttonNewMatch;
        [SerializeField] Button _buttonContinue;
        [SerializeField] RectTransform _panelRect;
        [SerializeField] VerticalLayoutGroup _verticalLayout;

        [Inject] Game _game;

        RectTransform _rectRestart;
        RectTransform _rectNewMatch;
        RectTransform _rectContinue;

        private void Awake()
        {
            _rectRestart = _buttonRestart.GetComponent<RectTransform>();
            _rectNewMatch = _buttonNewMatch.GetComponent<RectTransform>();
            _rectContinue = _buttonContinue.GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            base.Start();

            // Bind commands
            _game.RestartCommand.BindTo(_buttonRestart).AddTo(this);
            _game.NewMatchCommand.BindTo(_buttonNewMatch).AddTo(this);
            _game.ContinueCommand.BindTo(_buttonContinue).AddTo(this);
        }

        protected override void OnOrientationChanged(bool isLandscape)
        {
            _panelRect.offsetMin = isLandscape ? new Vector2(250, 200) : new Vector2(150, 250);
            _panelRect.offsetMax = isLandscape ? new Vector2(-250, -200) : new Vector2(-150, -250);

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
