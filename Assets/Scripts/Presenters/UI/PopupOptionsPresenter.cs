using Solitaire.Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class PopupOptionsPresenter : MonoBehaviour
    {
        [SerializeField] Button _buttonClose;
        [SerializeField] Toggle _toggleDraw;
        [SerializeField] Toggle _toggleAudio;
        [SerializeField] TextMeshProUGUI _labelRestart;
        [SerializeField] RectTransform _panelRect;

        [Inject] Options _options;
        [Inject] OrientationState _orientation;

        private void Start()
        {
            _options.CloseCommand.BindTo(_buttonClose).AddTo(this);
            _options.DrawThree.BindTo(_toggleDraw).AddTo(this);
            _options.AudioEnabled.BindTo(_toggleAudio).AddTo(this);
            _options.RestartNeeded.Subscribe(restartNeeded => 
                _labelRestart.gameObject.SetActive(restartNeeded)).AddTo(this);

            // Update layout on orientation change
            _orientation.State.Subscribe(orientation => AdjustLayout(orientation)).AddTo(this);
        }

        private void AdjustLayout(Orientation orientation)
        {
            bool isLandscape = orientation == Orientation.Landscape;

            _panelRect.offsetMin = isLandscape ? new Vector2(250, 150) : new Vector2(150, 250);
            _panelRect.offsetMax = isLandscape ? new Vector2(-250, -150) : new Vector2(-150, -250);
        }
    }
}
