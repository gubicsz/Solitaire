using DG.Tweening;
using Solitaire.Models;
using Solitaire.Services;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Solitaire.Presenters
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CardPresenter : MonoBehaviour, 
        IPoolable<Card.Suits, Card.Types, IMemoryPool>, IDisposable, 
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler, IPointerClickHandler
    {
        [Header("Sprites")]
        [SerializeField] SpriteRenderer _back;
        [SerializeField] SpriteRenderer _front;
        [SerializeField] SpriteRenderer _type;
        [SerializeField] SpriteRenderer _suit1;
        [SerializeField] SpriteRenderer _suit2;

        [Inject] Card _card;
        [Inject] Card.Config _config;
        [Inject] Game _game;
        [Inject] DragAndDropHandler _dndHandler;

        BoxCollider2D _collider;
        IMemoryPool _pool;

        public Card Card => _card;

        const float moveEpsilon = 0.00001f;

        void Start()
        {
            _collider = GetComponent<BoxCollider2D>();

            // Handle order change 
            _card.Order.Subscribe(order => UpdateOrder(order)).AddTo(this);

            // Animate card flip
            _card.IsFaceUp.Where(isFaceUp => (isFaceUp && !_front.gameObject.activeSelf) || (!isFaceUp && !_back.gameObject.activeSelf))
                .Subscribe(isFaceUp => AnimateFlip(isFaceUp)).AddTo(this);

            // Animate card movement
            _card.Position.Where(position => Vector3.SqrMagnitude(position - transform.position) > moveEpsilon)
                .Subscribe(position => AnimateMove(position)).AddTo(this);
        }

        void AnimateFlip(bool isFaceUp)
        {
            transform.DOScaleX(0f, _config.AnimationDuration / 2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .OnStepComplete(() =>
                {
                    _back.gameObject.SetActive(!isFaceUp);
                    _front.gameObject.SetActive(isFaceUp);
                })
                .OnComplete(() => transform.localScale = Vector3.one);
        }

        void AnimateMove(Vector3 position)
        {
            if (_card.IsDragged)
            {
                transform.position = position;
            }
            else
            {
                transform.DOLocalMove(position, _config.AnimationDuration);
            }
        }

        void UpdateOrder(int order)
        {
            // Update the sorting order of each sprite
            int sortingOrder = order * 10;
            _back.sortingOrder = sortingOrder;
            _front.sortingOrder = sortingOrder;
            _type.sortingOrder = sortingOrder + 1;
            _suit1.sortingOrder = sortingOrder + 1;
            _suit2.sortingOrder = sortingOrder + 1;
        }

        void Initialize()
        {
            name = $"Card_{_card.Suit}_{_card.Type}";

            // Update suit sprites
            Sprite spriteSuit = _config.SuitSprites[(int)_card.Suit];
            _suit1.sprite = spriteSuit;
            _suit2.sprite = spriteSuit;

            // Update type color and sprite
            Color color = _config.Colors[(int)_card.Suit];
            Sprite spriteType = _config.TypeSprites[(int)_card.Type];
            _type.sprite = spriteType;
            _type.color = color;
        }

        #region IEventSystemHandlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_card.IsDraggable)
            {
                _dndHandler.BeginDrag(eventData, _card.Pile.SplitAt(_card));
                _collider.enabled = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_card.IsDraggable)
            {
                _dndHandler.Drag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_card.IsDraggable)
            {
                _dndHandler.EndDrag();
                _collider.enabled = true;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData == null || eventData.pointerDrag == null)
            {
                return;
            }

            if (eventData.pointerDrag.TryGetComponent(out CardPresenter cardPresenter) &&
                _card.Pile.CanAddCard(cardPresenter.Card))
            {
                _dndHandler.Drop();
                _game.InteractCard(cardPresenter.Card, _card.Pile);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }
            
            if ((_card.IsFaceUp.Value && eventData.clickCount == 2) ||
                (!_card.IsFaceUp.Value && eventData.clickCount == 1))
            {
                Pile pile = _game.FindValidPileForCard(_card);
                _game.InteractCard(_card, pile);
            }
        }

        #endregion IEventSystemHandlers

        #region IPoolable

        public void OnSpawned(Card.Suits suit, Card.Types type, IMemoryPool pool)
        {
            // Init model
            _pool = pool;
            _card.Init(suit, type);
            Initialize();
        }

        public void OnDespawned()
        {
            // Reset model
            _pool = null;
            _card.Reset(Vector3.zero);
        }

        #endregion IPoolable

        #region IDisposable

        public void Dispose()
        {
            _pool.Despawn(this);
        }

        #endregion IDisposable

        public class Factory : PlaceholderFactory<Card.Suits, Card.Types, CardPresenter>
        {
        }
    }
}
