using Solitaire.Models;
using Solitaire.Services;
using System;
using Zenject;

namespace Solitaire.Commands
{
    public class DrawCardCommand : ICommand, IDisposable, IPoolable<Card, Pile, Pile, IMemoryPool> 
    {
        [Inject] AudioService _audioService;

        Card _card;
        Pile _pileStock;
        Pile _pileWaste;
        IMemoryPool _pool;

        public void Execute()
        {
            _card.Flip();
            _pileWaste.AddCard(_card);

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }

        public void Undo()
        {
            _card.Flip();
            _pileStock.AddCard(_card);

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }

        public void OnDespawned()
        {
            _card = null;
            _pileStock = null;
            _pileWaste = null;
            _pool = null;
        }

        public void OnSpawned(Card card, Pile pileStock, Pile pileWaste, IMemoryPool pool)
        {
            _card = card;
            _pileStock = pileStock;
            _pileWaste = pileWaste;
            _pool = pool;
        }

        public class Factory : PlaceholderFactory<Card, Pile, Pile, DrawCardCommand>
        {
        }
    }
}
