using System;
using System.Collections.Generic;
using Solitaire.Models;
using Solitaire.Presenters;

namespace Solitaire.Services
{
    public class CardSpawner : ICardSpawner
    {
        private readonly CardPresenter.Factory _factory;

        private readonly IList<CardPresenter> _tempCopies;

        public CardSpawner(CardPresenter.Factory factory)
        {
            _factory = factory;
            _tempCopies = new List<CardPresenter>();
        }

        public IList<CardPresenter> Cards { get; } = new List<CardPresenter>(52);

        public void SpawnAll()
        {
            // Spawn cards
            foreach (Card.Suits suit in Enum.GetValues(typeof(Card.Suits)))
            foreach (Card.Types type in Enum.GetValues(typeof(Card.Types)))
                Cards.Add(Spawn(suit, type));
        }

        public void DespawnAll()
        {
            // Despawn cards
            foreach (var card in Cards)
                card.Dispose();

            // Clear list
            Cards.Clear();
        }

        public CardPresenter Spawn(Card.Suits suit, Card.Types type)
        {
            return _factory.Create(suit, type);
        }

        public void Despawn(CardPresenter card)
        {
            // Handle error
            if (card == null)
                return;

            // Despawn card
            card.Dispose();

            if (Cards.Contains(card))
                Cards.Remove(card);
        }

        public IList<CardPresenter> MakeCopies(IList<Card> cards)
        {
            _tempCopies.Clear();

            for (var i = 0; i < cards.Count; i++)
                _tempCopies.Add(MakeCopy(cards[i]));

            return _tempCopies;
        }

        public CardPresenter MakeCopy(Card card)
        {
            var copy = Spawn(card.Suit, card.Type);
            copy.transform.position = card.Position.Value;
            copy.Flip(card.IsFaceUp.Value);

            copy.Card.IsFaceUp.Value = card.IsFaceUp.Value;
            copy.Card.Position.Value = card.Position.Value;
            copy.Card.Order.Value = card.Order.Value;
            copy.Card.Alpha.Value = 0.5f;

            return copy;
        }
    }
}
