using Solitaire.Models;
using Solitaire.Presenters;
using System;
using System.Collections.Generic;

namespace Solitaire.Services
{
    public class CardSpawner
    {
        public List<CardPresenter> Cards { get; private set; } = new List<CardPresenter>(52);

        private CardPresenter.Factory _factory;

        public CardSpawner(CardPresenter.Factory factory)
        {
            // Set references
            _factory = factory;
        }

        public void SpawnAll()
        {
            // Spawn cards
            foreach (Card.Suits suit in Enum.GetValues(typeof(Card.Suits)))
            {
                foreach (Card.Types type in Enum.GetValues(typeof(Card.Types)))
                {
                    Cards.Add(_factory.Create(suit, type));
                }
            }
        }

        public void DespawnAll()
        {
            // Despawn cards
            foreach (var card in Cards)
            {
                card.Dispose();
            }

            // Clear list
            Cards.Clear();
        }

        public void Despawn(CardPresenter card)
        {
            // Handle error
            if (card == null || !Cards.Contains(card))
            {
                return;
            }

            // Despawn card
            card.Dispose();
            Cards.Remove(card);
        }
    }
}
