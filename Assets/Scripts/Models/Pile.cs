using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Models
{
    public class Pile
    {
        public enum CardArrangement
        {
            Stack,
            Waterfall,
            TopThree
        }

        public enum PileType
        {
            Stock,
            Waste,
            Foundation,
            Tableau
        }

        private const float Depth = 0.005f;
        private const float VertFaceUp = 0.5f;
        private const float VertFaceDown = 0.2f;
        private const float Horizontal = 0.3f;

        private readonly List<Card> _splitCards;

        public Pile()
        {
            Cards = new List<Card>();
            _splitCards = new List<Card>();
        }

        public PileType Type { get; private set; }
        public CardArrangement Arrangement { get; private set; }
        public Vector3 Position { get; private set; }
        public List<Card> Cards { get; }

        public bool HasCards => Cards.Count > 0;
        public bool IsStock => Type == PileType.Stock;
        public bool IsWaste => Type == PileType.Waste;
        public bool IsFoundation => Type == PileType.Foundation;
        public bool IsTableau => Type == PileType.Tableau;
        public float OffsetDepth => Depth;
        public float OffsetVertFaceUp => VertFaceUp;
        public float OffsetVertFaceDown => VertFaceDown;
        public float OffsetHorizontal => Horizontal;

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
            return HasCards ? Cards[^1] : null;
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
                        return true;

                    if (
                        topCard != null
                        && topCard.Suit == card.Suit
                        && topCard.Type == card.Type - 1
                        && card.IsOnTop
                    )
                        return true;

                    return false;

                case PileType.Tableau:
                    if (topCard == null && card.Type == Card.Types.King)
                        return true;

                    if (
                        topCard != null
                        && topCard.Type == card.Type + 1
                        && (
                            ((int)topCard.Suit / 2 == 0 && (int)card.Suit / 2 == 1)
                            || ((int)topCard.Suit / 2 == 1 && (int)card.Suit / 2 == 0)
                        )
                    )
                        return true;

                    return false;

                default:
                    return false;
            }
        }

        public void AddCard(Card card)
        {
            if (card == null)
                return;

            if (card.IsInPile)
                card.Pile.RemoveCard(card);

            card.Pile = this;
            Cards.Add(card);

            card.Order.Value = Cards.Count - 1;

            if (Arrangement == CardArrangement.TopThree && Cards.Count > 3)
                // Update the position of the last three cards
                for (var i = Cards.Count - 3; i < Cards.Count; i++)
                    UpdateCardPosition(Cards[i]);
            else
                UpdateCardPosition(card);

            // Set visibility and interactability of previous card based on arrangement
            if (Arrangement == CardArrangement.Stack && Cards.Count >= 3)
            {
                var thirdFromTop = Cards[^3];
                thirdFromTop.IsVisible.Value = false;
                thirdFromTop.IsInteractable.Value = false;
            }
            else if (Arrangement == CardArrangement.TopThree && Cards.Count >= 7)
            {
                var fifthFromTop = Cards[^7];
                fifthFromTop.IsVisible.Value = false;
                fifthFromTop.IsInteractable.Value = false;
            }
        }

        public void RemoveCard(Card card)
        {
            if (card == null)
                return;

            if (Cards.Remove(card))
                card.Pile = null;

            if (Arrangement == CardArrangement.TopThree && Cards.Count > 2)
                // Update the position of the last two cards
                for (var i = Cards.Count - 1; i >= Cards.Count - 2; i--)
                    UpdateCardPosition(Cards[i]);

            // Set visibility and interactability of previous card based on arrangement
            if (Arrangement == CardArrangement.Stack && Cards.Count >= 2)
            {
                var secondFromTop = Cards[^2];
                secondFromTop.IsVisible.Value = true;
                secondFromTop.IsInteractable.Value = true;
            }
            else if (Arrangement == CardArrangement.TopThree && Cards.Count >= 3)
            {
                var thirdFromTop = Cards[^3];
                thirdFromTop.IsVisible.Value = true;
                thirdFromTop.IsInteractable.Value = true;
            }
        }

        public void AddCards(IList<Card> cards)
        {
            if (cards == null)
                return;

            for (var i = 0; i < cards.Count; i++)
                AddCard(cards[i]);
        }

        public void RemoveCards(IList<Card> cards)
        {
            if (cards == null)
                return;

            for (var i = 0; i < cards.Count; i++)
                RemoveCard(cards[i]);
        }

        public IList<Card> SplitAt(Card card)
        {
            var index = Cards.IndexOf(card);

            if (index < 0 || index >= Cards.Count)
                return null;

            _splitCards.Clear();

            for (var i = index; i < Cards.Count; i++)
                _splitCards.Add(Cards[i]);

            return _splitCards;
        }

        public void UpdatePosition(Vector3 position)
        {
            Position = position;

            for (var i = 0; i < Cards.Count; i++)
                UpdateCardPosition(Cards[i]);
        }

        public Vector3 CalculateCardPosition(int index, int count, Card prevCard)
        {
            switch (Arrangement)
            {
                case CardArrangement.Stack:
                    return Position + OffsetDepth * (index + 1) * Vector3.back;

                case CardArrangement.Waterfall:
                    var verticalOffset = 0f;

                    if (prevCard != null)
                        verticalOffset =
                            Mathf.Abs(prevCard.Position.Value.y - Position.y)
                            + (prevCard.IsFaceUp.Value ? OffsetVertFaceUp : OffsetVertFaceDown);

                    return Position
                        + OffsetDepth * (index + 1) * Vector3.back
                        + verticalOffset * Vector3.down;

                case CardArrangement.TopThree:
                    float horizontalOffset;

                    if (count > 3)
                        horizontalOffset = (3 - Mathf.Min(3, count - index)) * OffsetHorizontal;
                    else
                        horizontalOffset = index * OffsetHorizontal;

                    return Position
                        + OffsetDepth * (index + 1) * Vector3.back
                        + horizontalOffset * Vector3.right;

                default:
                    return Vector3.zero;
            }
        }

        private void UpdateCardPosition(Card card)
        {
            var count = Cards.Count;
            var index = Cards.IndexOf(card);
            var prevCard = index > 0 ? Cards[index - 1] : null;
            card.Position.Value = CalculateCardPosition(index, count, prevCard);
        }

        public override string ToString()
        {
            return $"{Type}";
        }
    }
}
