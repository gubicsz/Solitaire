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
                        topCard.Type == card.Type - 1 && card.Pile.TopCard() == card)
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

            card.SetPile(this);
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
                card.SetPile(null);
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

        private void UpdateCardPosition(Card card)
        {
            int index = Cards.IndexOf(card);

            switch (Arrangement)
            {
                case CardArrangement.Stack:
                    card.Position.Value = Position +
                        _offsetDepth * (index + 1) * Vector3.back;
                    break;

                case CardArrangement.Waterfall:
                    float verticalOffset = 0f;

                    if (index > 0)
                    {
                        Card prevCard = Cards[index - 1];
                        verticalOffset = Mathf.Abs(prevCard.Position.Value.y - Position.y) +
                            (prevCard.IsFaceUp.Value ? _offsetVertFaceUp : _offsetVertFaceDown);
                    }

                    card.Position.Value = Position +
                        _offsetDepth * (index + 1) * Vector3.back +
                        verticalOffset * Vector3.down;
                    break;

                case CardArrangement.TopThree:
                    float horizontalOffset;

                    if (Cards.Count > 3)
                    {
                        horizontalOffset = (3 - Mathf.Min(3, Cards.Count - index)) * _offsetHorizontal;
                    }
                    else
                    {
                        horizontalOffset = index * _offsetHorizontal;
                    }

                    card.Position.Value = Position +
                        _offsetDepth * (index + 1) * Vector3.back +
                        horizontalOffset * Vector3.right;
                    break;

                default:
                    break;
            }
        }
    }
}
