using Solitaire.Models;
using Solitaire.Services;
using System;
using System.Collections.Generic;
using Zenject;

namespace Solitaire.Commands
{
    public class DrawCardCommand : ICommand, IDisposable, IPoolable<Pile, Pile, IMemoryPool> 
    {
        [Inject] readonly IAudioService _audioService;
        [Inject] readonly Options _options;

        readonly List<Card> _cards = new List<Card>(3);
        Pile _pileStock;
        Pile _pileWaste;
        IMemoryPool _pool;

        public void Execute()
        {
            int count = _options.DrawThree.Value ? 3 : 1;

            for (int i = 0; i < count; i++)
            {
                Card card = _pileStock.TopCard();

                if (card == null)
                {
                    break;
                }

                card.Flip();
                _pileWaste.AddCard(card);
                _cards.Add(card);
            }

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }

        public void Undo()
        {
            for (int i = _cards.Count - 1; i >= 0; i--)
            {
                Card card = _cards[i];
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

        public class Factory : PlaceholderFactory<Pile, Pile, DrawCardCommand>
        {
        }
    }
}
