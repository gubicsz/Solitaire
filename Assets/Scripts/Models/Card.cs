using System;
using UniRx;
using UnityEngine;

namespace Solitaire.Models
{
    public class Card
    {
        [Serializable]
        public class Config
        {
            public float AnimationDuration = 0.5f;
            public Color[] Colors;
            public Sprite[] SuitSprites;
            public Sprite[] TypeSprites;
        }

        public enum Suits : byte
        {
            Spade,
            Club,
            Heart,
            Diamond,
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
            King,
        }

        public Suits Suit { get; private set; }
        public Types Type { get; private set; }
        public Pile Pile { get; private set; }
        public BoolReactiveProperty IsFaceUp { get; private set; } = new BoolReactiveProperty();
        public Vector3ReactiveProperty Position { get; private set; } = new Vector3ReactiveProperty();
        public IntReactiveProperty Order { get; private set; } = new IntReactiveProperty();
        public Vector3 DragOrigin { get; set; }
        public Vector3 DragOffset { get; set; }
        public int DragOrder { get; set; }
        public bool IsDragged { get; set; }
        public bool IsInPile => Pile != null;
        public bool IsDraggable => IsFaceUp.Value;

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
            DragOrigin = Vector3.zero;
            DragOffset = Vector3.zero;
            DragOrder = 0;
            IsDragged = false;
        }

        public int GetValue()
        {
            if (Type == Types.Jack ||
                Type == Types.Queen ||
                Type == Types.King)
            {
                return 10;
            }

            if (Type == Types.Ace)
            {
                return 11;
            }

            return (int)Type + 1;
        }

        public void Flip()
        {
            IsFaceUp.Value = !IsFaceUp.Value;
        }

        public void SetPile(Pile pile)
        {
            Pile = pile;
        }
    }
}
