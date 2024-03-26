using System.Collections.Generic;
using Solitaire.Models;
using Solitaire.Presenters;

namespace Solitaire.Services
{
    public interface ICardSpawner
    {
        IList<CardPresenter> Cards { get; }

        void Despawn(CardPresenter card);
        void DespawnAll();
        IList<CardPresenter> MakeCopies(IList<Card> cards);
        CardPresenter MakeCopy(Card card);
        CardPresenter Spawn(Card.Suits suit, Card.Types type);
        void SpawnAll();
    }
}
