using System;
using System.Collections.Generic;
using Solitaire.Models;
using Solitaire.Services;
using Zenject;

namespace Solitaire.Commands
{
    public class DrawCardCommand : ICommand, IDisposable, IPoolable<Pile, Pile, IMemoryPool>
    {
        [Inject]
        private readonly IAudioService _audioService;

        private readonly List<Card> _cards = new(3);

        [Inject]
        private readonly Options _options;
        private Pile _pileStock;
        private Pile _pileWaste;
        private IMemoryPool _pool;

        public void Execute()
        {
            var count = _options.DrawThree.Value ? 3 : 1;

            for (var i = 0; i < count; i++)
            {
                var card = _pileStock.TopCard();

                if (card == null)
                    break;

                card.Flip();
                _pileWaste.AddCard(card);
                _cards.Add(card);
            }

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }

        public void Undo()
        {
            for (var i = _cards.Count - 1; i >= 0; i--)
            {
                var card = _cards[i];
                card.Flip();
                _pileStock.AddCard(card);
                _cards.RemoveAt(i);
            }

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }

        public void Dispose()
        {
            _pool.Despawn(this);
        }

        public void OnDespawned()
        {
            _pileStock = null;
            _pileWaste = null;
            _pool = null;
        }

        public void OnSpawned(Pile pileStock, Pile pileWaste, IMemoryPool pool)
        {
            _pileStock = pileStock;
            _pileWaste = pileWaste;
            _pool = pool;
            _cards.Clear();
        }

        public class Factory : PlaceholderFactory<Pile, Pile, DrawCardCommand> { }
    }
}
