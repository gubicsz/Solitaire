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
        [SerializeField] Pile.PileType Type;
        [SerializeField] Pile.CardArrangement Arrangement;
        [SerializeField] Vector3 PosPortrait;
        [SerializeField] Vector3 PosLandscape;

        [Inject] Pile _pile;
        [Inject] Game _game;
        [Inject] DragAndDropHandler _dndHandler;
        [Inject] OrientationState _orientation;
        [Inject] AudioService _audioService;

        public Pile Pile => _pile;

        void Awake()
        {
            _pile.Init(Type, Arrangement, transform.position);
        }

        private void Start()
        {
            // Update layout on orientation change
            _orientation.State.Subscribe(orientation => UpdateLayout(orientation)).AddTo(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData == null || eventData.pointerDrag == null)
            {
                return;
            }

            if (eventData.pointerDrag.TryGetComponent(out CardPresenter cardPresenter) &&
                _pile.CanAddCard(cardPresenter.Card))
            {
                _dndHandler.Drop();
                _game.MoveCard(cardPresenter.Card, _pile);
            }
            else
            {
                _game.IndicateError();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData?.clickCount == 1 && _pile.IsStock)
            {
                _game.RefillStock();
            }
        }

        private void UpdateLayout(Orientation orientation)
        {
            Vector3 position = orientation == Orientation.Landscape ?
                PosLandscape : PosPortrait;

            transform.position = position;
            _pile.UpdatePosition(position);
        }
    }
}
