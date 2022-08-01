using Solitaire.Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class PopupOptionsPresenter : OrientationAwarePresenter
    {
        [SerializeField] Button _buttonClose;
        [SerializeField] Toggle _toggleDraw;
        [SerializeField] Toggle _toggleAudio;
        [SerializeField] TextMeshProUGUI _labelRestart;
        [SerializeField] RectTransform _panelRect;

        [Inject] readonly Options _options;

        protected override void Start()
        {
            base.Start();

            _options.CloseCommand.BindTo(_buttonClose).AddTo(this);
            _options.DrawThree.BindTo(_toggleDraw).AddTo(this);
            _options.AudioEnabled.BindTo(_toggleAudio).AddTo(this);
            _options.RestartNeeded.Subscribe(restartNeeded => 
                _labelRestart.gameObject.SetActive(restartNeeded)).AddTo(this);
        }

        protected override void OnOrientationChanged(bool isLandscape)
        {
            _panelRect.offsetMin = isLandscape ? new Vector2(250, 200) : new Vector2(150, 250);
            _panelRect.offsetMax = isLandscape ? new Vector2(-250, -200) : new Vector2(-150, -250);
        }
    }
}
