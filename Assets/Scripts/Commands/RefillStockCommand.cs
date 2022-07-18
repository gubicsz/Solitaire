using Solitaire.Models;
using Solitaire.Services;
using System;
using Zenject;

namespace Solitaire.Commands
{
    public class RefillStockCommand : ICommand, IDisposable, IPoolable<Pile, Pile, IMemoryPool>
    {
        [Inject] PointsService _pointsService;
        [Inject] AudioService _audioService;
        [Inject] Game.Config _gameConfig;

        Pile _pileStock;
        Pile _pileWaste;
        IMemoryPool _pool;
        int _points;

        public void Execute()
        {
            Card topCard;

            while ((topCard = _pileWaste.TopCard()) != null)
            {
                topCard.Flip();
                _pileStock.AddCard(topCard);
            }

            _points = _pointsService.Points.Value;
            _pointsService.Add(_gameConfig.PointsRecycleWaste);

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }

        public void Undo()
        {
            Card topCard;

            while ((topCard = _pileStock.TopCard()) != null)
            {
                topCard.Flip();
                _pileWaste.AddCard(topCard);
            }

            _pointsService.Set(_points);

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
            _points = 0;
        }

        public class Factory : PlaceholderFactory<Pile, Pile, RefillStockCommand>
        {
        }
    }
}
