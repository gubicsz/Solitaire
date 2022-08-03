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

        [Inject] readonly Game _game;
        [Inject] readonly Card _card;
        [Inject] readonly Card.Config _config;
        [Inject] readonly IDragAndDropHandler _dndHandler;

        Tweener _tweenScale;
        Tweener _tweenMove;
        BoxCollider2D _collider;
        Transform _transform;
        IMemoryPool _pool;
        float _lastClick;

        public Card Card => _card;

        const float DoubleClickInterval = 0.4f;
        const float MoveEpsilon = 0.00001f;
        const int AnimOrder = 100;

        void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
            _transform = transform;

            _card.Alpha.Subscribe(UpdateAlpha).AddTo(this);
            _card.Order.Subscribe(UpdateOrder).AddTo(this);
            _card.IsVisible.Subscribe(UpdateVisiblity).AddTo(this);
            _card.IsInteractable.Subscribe(UpdateInteractability).AddTo(this);
            _card.IsFaceUp.Where(CanFlip).Subscribe(AnimateFlip).AddTo(this);
            _card.Position.Where(CanMove).Subscribe(AnimateMove).AddTo(this);
        }

        bool CanFlip(bool isFaceUp)
        {
            return (isFaceUp && !_front.gameObject.activeSelf) || (!isFaceUp && !_back.gameObject.activeSelf);
        }

        void AnimateFlip(bool isFaceUp)
        {
            // Scale X from 1 to 0 then back to 1 again,
            // switching between front and back sprites in the middle.
            // This gives the illusion of flipping the card in 2D.
            if (_tweenScale == null)
            {
                _tweenScale = _transform.DOScaleX(0f, _config.AnimationDuration / 2f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.Linear)
                    .SetAutoKill(false)
                    .OnStepComplete(() => Flip(_card.IsFaceUp.Value))
                    .OnComplete(() => _transform.localScale = Vector3.one);
            }
            else
            {
                _tweenScale.Restart();
            }
        }

        bool CanMove(Vector3 position)
        {
            return Vector3.SqrMagnitude(position - _transform.position) > MoveEpsilon;
        }

        void AnimateMove(Vector3 position)
        {
            if (_card.IsDragged)
            {
                // Update position instantly while the card is being dragged
                _transform.position = position;
            }
            else
            {
                // Move card over time to the target position while changing
                // order at the start and end so the cards are overlaid correctly.
                if (_tweenMove == null)
                {
                    _tweenMove = _transform.DOLocalMove(position, _config.AnimationDuration)
                        .SetEase(Ease.OutQuad)
                        .SetAutoKill(false)
                        .OnRewind(() =>
                        {
                            _card.OrderToRestore = _card.IsInPile ? _card.Pile.Cards.IndexOf(_card) : _card.Order.Value;
                            _card.Order.Value = AnimOrder + _card.OrderToRestore;
                        })
                        .OnComplete(() =>
                        {
                            _card.Order.Value = _card.OrderToRestore;
                        });
                }
                else
                {
                    _tweenMove.ChangeEndValue(position, true).Restart();
                }
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
        
        void UpdateAlpha(float alpha)
        {
            Color color = _back.color;
            color.a = alpha;
            _back.color = color;

            color = _front.color;
            color.a = alpha;
            _front.color = color;

            color = _type.color;
            color.a = alpha;
            _type.color = color;

            color = _suit1.color;
            color.a = alpha;
            _suit1.color = color;

            color = _suit2.color;
            color.a = alpha;
            _suit2.color = color;
        }

        void UpdateVisiblity(bool isVisible)
        {
            _back.enabled = isVisible;
            _front.enabled = isVisible;
            _type.enabled = isVisible;
            _suit1.enabled = isVisible;
            _suit2.enabled = isVisible;
        }

        void UpdateInteractability(bool isInteractable)
        {
            _collider.enabled = isInteractable;
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

        public void Flip(bool isFaceUp)
        {
            _back.gameObject.SetActive(!isFaceUp);
            _front.gameObject.SetActive(isFaceUp);
        }

        #region IEventSystemHandlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_card.IsMoveable)
            {
                _dndHandler.BeginDrag(eventData, _card.Pile.SplitAt(_card));
                _card.IsInteractable.Value = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_card.IsMoveable)
            {
                _dndHandler.Drag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_card.IsMoveable)
            {
                _dndHandler.EndDrag();
                _card.IsInteractable.Value = true;
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
                _game.MoveCard(cardPresenter.Card, _card.Pile);
            }
            else
            {
                _game.PlayErrorSfx();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }

            if (_card.IsDrawable)
            {
                _game.DrawCard();
            }
            else if ((_lastClick + DoubleClickInterval) > Time.time)
            {
                if (_card.IsMoveable)
                {
                    _game.MoveCard(_card, null);
                }
                else
                {
                    _game.PlayErrorSfx();
                }
            }

            _lastClick = Time.time;
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
