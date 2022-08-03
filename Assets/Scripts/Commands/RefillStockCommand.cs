using Solitaire.Models;
using Solitaire.Services;
using System;
using Zenject;

namespace Solitaire.Commands
{
    public class RefillStockCommand : ICommand, IDisposable, IPoolable<Pile, Pile, IMemoryPool>
    {
        [Inject] readonly IPointsService _pointsService;
        [Inject] readonly IAudioService _audioService;
        [Inject] readonly Game.Config _gameConfig;
        [Inject] readonly Options _options;

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

            if (!_options.DrawThree.Value)
            {
                _points = _pointsService.Points.Value;
                _pointsService.Add(_gameConfig.PointsRecycleWaste);
            }

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

            if (!_options.DrawThree.Value)
            {
                _pointsService.Set(_points);
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
            _points = 0;
        }

        public class Factory : PlaceholderFactory<Pile, Pile, RefillStockCommand>
        {
        }
    }
}
