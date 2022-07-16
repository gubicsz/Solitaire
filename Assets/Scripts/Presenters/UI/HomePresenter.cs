using Solitaire.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class HomePresenter : MonoBehaviour
    {
        [SerializeField] Button _buttonNewMatch;
        [SerializeField] Button _buttonContinue;

        [Inject] Game _game;

        private void Start()
        {
            _game.NewMatchCommand.BindTo(_buttonNewMatch).AddTo(this);
            _game.ContinueCommand.BindTo(_buttonContinue).AddTo(this);
        }
    }
}
