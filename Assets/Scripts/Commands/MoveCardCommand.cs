using Solitaire.Models;
using Solitaire.Services;

namespace Solitaire.Commands
{
    public class MoveCardCommand : ICommand
    {
        readonly Card _card;
        readonly Pile _pileTarget;
        readonly Pile _pileSource;
        readonly PointsService _pointsService;
        readonly Game.Config _gameConfig;

        private bool _wasTopCardFlipped;

        public MoveCardCommand(Card card, Pile pile, PointsService pointsService, Game.Config gameConfig)
        {
            _card = card;
            _pileTarget = pile;
            _pileSource = card.Pile;
            _pointsService = pointsService;
            _gameConfig = gameConfig;
        }

        public void Execute()
        {
            if (_pileSource.TopCard() == _card)
            {
                // Single card
                _pileTarget.AddCard(_card);
            }
            else
            {
                // Multiple cards
                var cards = _pileSource.SplitAt(_card);
                _pileSource.RemoveCards(cards);
                _pileTarget.AddCards(cards);
            }

            // Scoring
            if (_pileSource.Type == Pile.PileType.Waste)
            {
                if (_pileTarget.Type == Pile.PileType.Tableau)
                {
                    _pointsService.Add(_gameConfig.PointsWasteToTableau);
                }
                else if (_pileTarget.Type == Pile.PileType.Foundation)
                {
                    _pointsService.Add(_gameConfig.PointsWasteToFoundation);
                }
            }
            else if (_pileSource.Type == Pile.PileType.Tableau && _pileTarget.Type == Pile.PileType.Foundation)
            {
                _pointsService.Add(_gameConfig.PointsTableauToFoundation);
            }
            else if (_pileSource.Type == Pile.PileType.Foundation && _pileTarget.Type == Pile.PileType.Tableau)
            {
                _pointsService.Add(_gameConfig.PointsFoundationToTableau);
            }

            // Reveal card below if needed
            Card cardBelow = _pileSource.TopCard();

            if (_pileSource.Type == Pile.PileType.Tableau &&
                cardBelow != null && !cardBelow.IsFaceUp.Value)
            {
                cardBelow.Flip();
                _wasTopCardFlipped = true;
                _pointsService.Add(_gameConfig.PointsTurnOverTableauCard);
            }
        }

        public void Undo()
        {
            // Hide top card of the source tableau pile
            Card cardTop = _pileSource.TopCard();

            if (_pileSource.Type == Pile.PileType.Tableau && _wasTopCardFlipped &&
                cardTop != null && cardTop.IsFaceUp.Value)
            {
                cardTop.Flip();
                _pointsService.Add(-_gameConfig.PointsTurnOverTableauCard);
            }

            // Scoring
            if (_pileSource.Type == Pile.PileType.Waste)
            {
                if (_pileTarget.Type == Pile.PileType.Tableau)
                {
                    _pointsService.Add(-_gameConfig.PointsWasteToTableau);
                }
                else if (_pileTarget.Type == Pile.PileType.Foundation)
                {
                    _pointsService.Add(-_gameConfig.PointsWasteToFoundation);
                }
            }
            else if (_pileSource.Type == Pile.PileType.Tableau && _pileTarget.Type == Pile.PileType.Foundation)
            {
                _pointsService.Add(-_gameConfig.PointsTableauToFoundation);
            }
            else if (_pileSource.Type == Pile.PileType.Foundation && _pileTarget.Type == Pile.PileType.Tableau)
            {
                _pointsService.Add(-_gameConfig.PointsFoundationToTableau);
            }

            if (_pileTarget.TopCard() == _card)
            {
                // Single card
                _pileSource.AddCard(_card);
            }
            else
            {
                // Multiple cards
                var cards = _pileTarget.SplitAt(_card);
                _pileTarget.RemoveCards(cards);
                _pileSource.AddCards(cards);
            }
        }
    }
}
