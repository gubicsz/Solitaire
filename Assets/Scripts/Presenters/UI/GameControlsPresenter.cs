using Solitaire.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class GameControlsPresenter : OrientationAwarePresenter
    {
        [SerializeField] Button _buttonOptions;
        [SerializeField] Button _buttonHome;
        [SerializeField] Button _buttonMatch;
        [SerializeField] Button _buttonUndo;
        [SerializeField] Button _buttonHint;
        [SerializeField] HorizontalLayoutGroup _horizontalLayout;

        [Inject] GameControls _gameControls;
        [Inject] GamePopup _gamePopup;

        protected override void Start()
        {
            base.Start();

            _gamePopup.OptionsCommand.BindTo(_buttonOptions).AddTo(this);
            _gameControls.HomeCommand.BindTo(_buttonHome).AddTo(this);
            _gamePopup.MatchCommand.BindTo(_buttonMatch).AddTo(this);
            _gameControls.UndoCommand.BindTo(_buttonUndo).AddTo(this);
            _gameControls.HintCommand.BindTo(_buttonHint).AddTo(this);
        }

        protected override void OnOrientationChanged(bool isLandscape)
        {
            _horizontalLayout.spacing = isLandscape ? 30 : 10;
        }
    }
}
