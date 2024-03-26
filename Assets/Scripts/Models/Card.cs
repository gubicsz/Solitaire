using System;
using UniRx;
using UnityEngine;

namespace Solitaire.Models
{
    public class Card
    {
        public enum Suits : byte
        {
            Spade,
            Club,
            Heart,
            Diamond
        }

        public enum Types : byte
        {
            Ace,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten,
            Jack,
            Queen,
            King
        }

        public Card()
        {
            IsFaceUp = new BoolReactiveProperty();
            Position = new Vector3ReactiveProperty();
            Order = new IntReactiveProperty();
            Alpha = new FloatReactiveProperty(1);
            IsVisible = new BoolReactiveProperty(true);
            IsInteractable = new BoolReactiveProperty(true);
        }

        public Suits Suit { get; private set; }
        public Types Type { get; private set; }
        public BoolReactiveProperty IsFaceUp { get; }
        public Vector3ReactiveProperty Position { get; }
        public IntReactiveProperty Order { get; }
        public FloatReactiveProperty Alpha { get; }
        public BoolReactiveProperty IsVisible { get; }
        public BoolReactiveProperty IsInteractable { get; }

        public Pile Pile { get; set; }
        public Vector3 DragOrigin { get; set; }
        public Vector3 DragOffset { get; set; }
        public int OrderToRestore { get; set; }
        public bool IsDragged { get; set; }

        public bool IsInPile => Pile != null;
        public bool IsOnBottom => Pile.BottomCard() == this;
        public bool IsOnTop => Pile.TopCard() == this;

        public bool IsMoveable =>
            IsInPile
            && ((Pile.IsWaste && IsOnTop && IsFaceUp.Value) || (!Pile.IsWaste && IsFaceUp.Value));

        public bool IsDrawable => IsInPile && Pile.IsStock && IsOnTop && !IsFaceUp.Value;

        public void Init(Suits suit, Types type)
        {
            Suit = suit;
            Type = type;
        }

        public void Reset(Vector3 position)
        {
            Pile = null;
            IsFaceUp.Value = false;
            Position.Value = position;
            Order.Value = 0;
            Alpha.Value = 1f;
            IsVisible.Value = true;
            IsInteractable.Value = true;
            DragOrigin = Vector3.zero;
            DragOffset = Vector3.zero;
            OrderToRestore = 0;
            IsDragged = false;
        }

        public int GetValue()
        {
            if (Type == Types.Jack || Type == Types.Queen || Type == Types.King)
                return 10;

            if (Type == Types.Ace)
                return 11;

            return (int)Type + 1;
        }

        public void Flip()
        {
            IsFaceUp.Value = !IsFaceUp.Value;
        }

        public override string ToString()
        {
            return $"{Suit} {Type}";
        }

        [Serializable]
        public class Config
        {
            public float AnimationDuration = 0.5f;
            public Color[] Colors;
            public Sprite[] SuitSprites;
            public Sprite[] TypeSprites;
        }
    }
}
