using Solitaire.Models;
using Solitaire.Services;

namespace Solitaire.Commands
{
    public class RefillStockCommand : ICommand
    {
        readonly Pile _pileStock;
        readonly Pile _pileWaste;
        readonly PointsService _pointsService;
        readonly Game.Config _gameConfig;

        int _points;

        public RefillStockCommand(Pile pileStock, Pile pileWaste, PointsService pointsService, Game.Config gameConfig)
        {
            _pileStock = pileStock;
            _pileWaste = pileWaste;
            _pointsService = pointsService;
            _gameConfig = gameConfig;
        }

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
        }
    }
}
