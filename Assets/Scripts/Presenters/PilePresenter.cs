using Solitaire.Models;
using Solitaire.Services;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Solitaire.Presenters
{
    public class PilePresenter : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [SerializeField]
        private Pile.PileType Type;

        [SerializeField]
        private Pile.CardArrangement Arrangement;

        [SerializeField]
        private Vector3 PosPortrait;

        [SerializeField]
        private Vector3 PosLandscape;

        [Inject]
        private readonly IDragAndDropHandler _dndHandler;

        [Inject]
        private readonly Game _game;

        [Inject]
        private readonly OrientationState _orientation;

        [Inject]
        private readonly Pile _pile;

        public Pile Pile => _pile;

        private void Awake()
        {
            _pile.Init(Type, Arrangement, transform.position);
        }

        private void Start()
        {
            // Update layout on orientation change
            _orientation.State.Subscribe(UpdateLayout).AddTo(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData == null || eventData.pointerDrag == null)
                return;

            if (
                eventData.pointerDrag.TryGetComponent(out CardPresenter cardPresenter)
                && _pile.CanAddCard(cardPresenter.Card)
            )
            {
                _dndHandler.Drop();
                _game.MoveCard(cardPresenter.Card, _pile);
            }
            else
            {
                _game.PlayErrorSfx();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData?.clickCount == 1 && _pile.IsStock)
                _game.RefillStock();
        }

        private void UpdateLayout(Orientation orientation)
        {
            var position = orientation == Orientation.Landscape ? PosLandscape : PosPortrait;

            transform.position = position;
            _pile.UpdatePosition(position);
        }
    }
}
