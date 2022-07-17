using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Models
{
    public class Pile
    {
        public enum PileType
        {
            Stock,
            Waste,
            Foundation,
            Tableau,
        }

        public enum CardArrangement
        {
            Stack,
            Waterfall,
            TopThree,
        }

        public PileType Type { get; private set; }
        public CardArrangement Arrangement { get; private set; }
        public Vector3 Position { get; private set; }
        public List<Card> Cards { get; private set; }

        public bool HasCards => Cards.Count > 0;
        public bool IsStock => Type == PileType.Stock;
        public bool IsWaste => Type == PileType.Waste;
        public bool IsFoundation => Type == PileType.Foundation;
        public bool IsTableau => Type == PileType.Tableau;

        List<Card> _splitCards;

        const float _offsetDepth = 0.005f;
        const float _offsetVertFaceUp = 0.5f;
        const float _offsetVertFaceDown = 0.2f;
        const float _offsetHorizontal = 0.3f;

        public Pile()
        {
            Cards = new List<Card>();
            _splitCards = new List<Card>();
        }

        public void Init(PileType type, CardArrangement arrangement, Vector3 position)
        {
            Type = type;
            Arrangement = arrangement;
            Position = position;
        }

        public void Reset()
        {
            Cards.Clear();
        }

        public Card BottomCard()
        {
            return HasCards ? Cards[0] : null;
        }

        public Card TopCard()
        {
            return HasCards ? Cards[Cards.Count - 1] : null;
        }

        public bool CanAddCard(Card card)
        {
            var topCard = TopCard();

            switch (Type)
            {
                case PileType.Stock:
                    return false;

                case PileType.Waste:
                    return false;

                case PileType.Foundation:
                    if (topCard == null && card.Type == Card.Types.Ace)
                    {
                        return true;
                    }

                    if (topCard != null && topCard.Suit == card.Suit &&
                        topCard.Type == card.Type - 1 && card.IsOnTop)
                    {
                        return true;
                    }

                    return false;

                case PileType.Tableau:
                    if (topCard == null && card.Type == Card.Types.King)
                    {
                        return true;
                    }

                    if (topCard != null && topCard.Type == card.Type + 1 &&
                        (((int)topCard.Suit / 2 == 0 && (int)card.Suit / 2 == 1) ||
                        ((int)topCard.Suit / 2 == 1 && (int)card.Suit / 2 == 0)))
                    {
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        public void AddCard(Card card)
        {
            if (card == null)
            {
                return;
            }

            if (card.IsInPile)
            {
                card.Pile.RemoveCard(card);
            }

            card.Pile = this;
            Cards.Add(card);

            card.Order.Value = Cards.Count - 1;

            if (Arrangement == CardArrangement.TopThree && Cards.Count > 3)
            {
                // Update the position of the last three cards
                for (int i = Cards.Count - 3; i < Cards.Count; i++)
                {
                    UpdateCardPosition(Cards[i]);
                }
            }
            else
            {
                UpdateCardPosition(card);
            }
        }

        public void RemoveCard(Card card)
        {
            if (card == null)
            {
                return;
            }

            if (Cards.Remove(card))
            {
                card.Pile = null;
            }

            if (Arrangement == CardArrangement.TopThree && Cards.Count > 2)
            {
                // Update the position of the last two cards
                for (int i = Cards.Count - 1; i >= Cards.Count - 2; i--)
                {
                    UpdateCardPosition(Cards[i]);
                }
            }
        }

        public void AddCards(IList<Card> cards)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                AddCard(cards[i]);
            }
        }

        public void RemoveCards(IList<Card> cards)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                RemoveCard(cards[i]);
            }
        }

        public IList<Card> SplitAt(Card card)
        {
            int index = Cards.IndexOf(card);

            if (index < 0 || index >= Cards.Count)
            {
                return null;
            }

            _splitCards.Clear();

            for (int i = index; i < Cards.Count; i++)
            {
                _splitCards.Add(Cards[i]);
            }

            return _splitCards;
        }

        public void UpdatePosition(Vector3 position)
        {
            Position = position;

            for (int i = 0; i < Cards.Count; i++)
            {
                UpdateCardPosition(Cards[i]);
            }
        }

        public Vector3 CalculateCardPosition(int index, int count, Card prevCard)
        {
            switch (Arrangement)
            {
                case CardArrangement.Stack:
                    return Position + _offsetDepth * (index + 1) * Vector3.back;

                case CardArrangement.Waterfall:
                    float verticalOffset = 0f;

                    if (prevCard != null)
                    {
                        verticalOffset = Mathf.Abs(prevCard.Position.Value.y - Position.y) +
                            (prevCard.IsFaceUp.Value ? _offsetVertFaceUp : _offsetVertFaceDown);
                    }

                    return Position + _offsetDepth * (index + 1) * Vector3.back + verticalOffset * Vector3.down;

                case CardArrangement.TopThree:
                    float horizontalOffset;

                    if (count > 3)
                    {
                        horizontalOffset = (3 - Mathf.Min(3, count - index)) * _offsetHorizontal;
                    }
                    else
                    {
                        horizontalOffset = index * _offsetHorizontal;
                    }

                    return Position + _offsetDepth * (index + 1) * Vector3.back + horizontalOffset * Vector3.right;

                default:
                    return Vector3.zero;
            }
        }

        private void UpdateCardPosition(Card card)
        {
            int count = Cards.Count;
            int index = Cards.IndexOf(card);
            Card prevCard = index > 0 ? Cards[index - 1] : null;
            card.Position.Value = CalculateCardPosition(index, count, prevCard);
        }

        public override string ToString()
        {
            return $"{Type}";
        }
    }
}
