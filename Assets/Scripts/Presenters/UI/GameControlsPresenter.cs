using Solitaire.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class GameControlsPresenter : MonoBehaviour
    {
        [SerializeField] Button _buttonOptions;
        [SerializeField] Button _buttonHome;
        [SerializeField] Button _buttonMatch;
        [SerializeField] Button _buttonUndo;
        [SerializeField] Button _buttonHint;
        [SerializeField] HorizontalLayoutGroup _horizontalLayout;

        [Inject] GameControls _gameControls;
        [Inject] GamePopup _gamePopup;
        [Inject] OrientationState _orientation;

        private void Start()
        {
            _gamePopup.OptionsCommand.BindTo(_buttonOptions).AddTo(this);
            _gameControls.HomeCommand.BindTo(_buttonHome).AddTo(this);
            _gamePopup.MatchCommand.BindTo(_buttonMatch).AddTo(this);
            _gameControls.UndoCommand.BindTo(_buttonUndo).AddTo(this);
            _gameControls.HintCommand.BindTo(_buttonHint).AddTo(this);

            _orientation.State.Subscribe(orientation => AdjustSpacing(orientation)).AddTo(this);
        }

        private void AdjustSpacing(Orientation orientation)
        {
            // TODO: replace layout group with manual position calculation to optimize performance
            _horizontalLayout.spacing = (orientation == Orientation.Portrait ? 10 : 30);
        }
    }
}
