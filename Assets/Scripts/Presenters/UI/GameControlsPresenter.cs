using Solitaire.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class GameControlsPresenter : MonoBehaviour
    {
        [SerializeField] Button _buttonHome;
        [SerializeField] Button _buttonPause;
        [SerializeField] Button _buttonUndo;
        [SerializeField] Button _buttonHint;
        [SerializeField] HorizontalLayoutGroup _horizontalLayout;

        [Inject] GameControls _gameControls;
        [Inject] OrientationState _orientation;

        private void Start()
        {
            _gameControls.HomeCommand.BindTo(_buttonHome).AddTo(this);
            _gameControls.PauseCommand.BindTo(_buttonPause).AddTo(this);
            _gameControls.UndoCommand.BindTo(_buttonUndo).AddTo(this);
            _gameControls.HintCommand.BindTo(_buttonHint).AddTo(this);

            _orientation.State.Subscribe(orientation => AdjustSpacing(orientation)).AddTo(this);
        }

        private void AdjustSpacing(Orientation orientation)
        {
            _horizontalLayout.spacing = (orientation == Orientation.Portrait ? 10 : 30);
        }
    }
}
