using Cysharp.Threading.Tasks;
using Solitaire.Commands;
using Solitaire.Helpers;
using Solitaire.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Zenject;

namespace Solitaire.Models
{
    public class Game : DisposableEntity
    {
        [Serializable]
        public class Config
        {
            public int PointsWasteToTableau = 5;
            public int PointsWasteToFoundation = 10;
            public int PointsTableauToFoundation = 10;
            public int PointsTurnOverTableauCard = 5;
            public int PointsFoundationToTableau = -15;
            public int PointsRecycleWaste = -100;
        }

        public enum State
        {
            Home,
            Dealing,
            Playing,
            Paused,
            Win,
        }

        public ReactiveCommand RestartCommand { get; private set; }
        public ReactiveCommand NewMatchCommand { get; private set; }
        public ReactiveCommand ContinueCommand { get; private set; }

        [Inject] CardSpawner _cardSpawner;
        [Inject] CommandService _commandService;
        [Inject] MovesService _movesService;
        [Inject] PointsService _pointsService;
        [Inject] Game.Config _gameConfig;
        [Inject] GameState _gameState;

        Pile _pileStock;
        Pile _pileWaste;
        IList<Pile> _pileFoundations;
        IList<Pile> _pileTableaus;
        IList<Card> _cards;

        public Game()
        {
            RestartCommand = new ReactiveCommand();
            RestartCommand.Subscribe(_ => Restart()).AddTo(this);

            NewMatchCommand = new ReactiveCommand();
            NewMatchCommand.Subscribe(_ => NewMatch()).AddTo(this);

            ContinueCommand = new ReactiveCommand();
            ContinueCommand.Subscribe(_ => Continue()).AddTo(this);
        }

        public void Init(Pile pileStock, Pile pileWaste, 
            IList<Pile> pileFoundations, IList<Pile> pileTableaus)
        {
            // Set references
            _pileStock = pileStock;
            _pileWaste = pileWaste;
            _pileFoundations = pileFoundations;
            _pileTableaus = pileTableaus;

            // Spawn cards
            _cardSpawner.SpawnAll();
            _cards = _cardSpawner.Cards.Select(c => c.Card).ToList();
        }

        public void RefillStock()
        {
            // Refill stock pile from waste pile
            if (!_pileStock.HasCards && _pileWaste.HasCards)
            {
                var refillStockCommand = new RefillStockCommand(_pileStock, _pileWaste, _pointsService, _gameConfig);
                refillStockCommand.Execute();
                _commandService.AddCommand(refillStockCommand);
                _movesService.Increment();
            }
        }

        public void InteractCard(Card card, Pile pile)
        {
            if (card == null || !card.IsInPile)
            {
                return;
            }

            if (card.IsFaceUp.Value && pile != null)
            {
                // Move card to pile
                var moveCardCommand = new MoveCardCommand(card, pile, _pointsService, _gameConfig);
                moveCardCommand.Execute();
                _commandService.AddCommand(moveCardCommand);
                _movesService.Increment();
            }
            else if (!card.IsFaceUp.Value && card.Pile.Type == Pile.PileType.Stock && card.Pile.TopCard() == card)
            {
                // Draw card from stock
                var drawCardCommand = new DrawCardCommand(_pileStock, _pileWaste, card);
                drawCardCommand.Execute();
                _commandService.AddCommand(drawCardCommand);
                _movesService.Increment();
            }
        }

        public void DetectWinCondition()
        {
            // All cards in the tableau piles should be revelead
            for (int i = 0; i < _pileTableaus.Count; i++)
            {
                Pile pileTableau = _pileTableaus[i];

                for (int j = 0; j < pileTableau.Cards.Count; j++)
                {
                    if (!pileTableau.Cards[j].IsFaceUp.Value)
                    {
                        return;
                    }
                }
            }

            // The stock and waste should be empty
            if (_pileStock.HasCards || _pileWaste.HasCards)
            {
                return;
            }

            WinAsync().Forget();
        }

        public Pile FindValidPileForCard(Card card)
        {
            if (card == null)
            {
                return null;
            }

            // Check foundations
            Pile pileTarget = CheckPilesForMove(_pileFoundations, card);

            // Check tableaus
            if (pileTarget == null)
            {
                pileTarget = CheckPilesForMove(_pileTableaus, card);
            }

            return pileTarget;
        }

        private Pile CheckPilesForMove(IList<Pile> piles, Card card)
        {
            for (int i = 0; i < piles.Count; i++)
            {
                Pile pile = piles[i];

                if (pile.CanAddCard(card))
                {
                    return pile;
                }
            }

            return null;
        }

        private void Reset()
        {
            // Reset piles
            _pileStock.Reset();
            _pileWaste.Reset();

            for (int i = 0; i < _pileFoundations.Count; i++)
            {
                _pileFoundations[i].Reset();
            }

            for (int i = 0; i < _pileTableaus.Count; i++)
            {
                _pileTableaus[i].Reset();
            }

            // Reset cards
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].Reset(_pileStock.Position);
            }

            // Reset services
            _movesService.Reset();
            _pointsService.Reset();
            _commandService.Reset();
        }

        private void ShuffleCards()
        {
            _cards = _cards.OrderBy(a => Guid.NewGuid()).ToList();
        }

        private async UniTask DealAsync()
        {
            // Start delaing
            _gameState.State.Value = State.Dealing;

            // Add cards to the stock pile
            await UniTask.Delay(300);
            _pileStock.AddCards(_cards);
            await UniTask.Delay(300);

            // Deal cards to the Tableau piles
            for (int i = 0; i < _pileTableaus.Count; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    Card topCard = _pileStock.TopCard();
                    _pileTableaus[i].AddCard(topCard);

                    if (j == i)
                    {
                        topCard.Flip();
                    }

                    await UniTask.DelayFrame(5);
                }
            }
            
            // Start playing
            _gameState.State.Value = State.Playing;
        }

        private async UniTask WinAsync()
        {
            // Start win
            _gameState.State.Value = State.Win;
            int cardsInTableaus;

            do
            {
                cardsInTableaus = 0;

                for (int i = 0; i < _pileTableaus.Count; i++)
                {
                    Pile pileTableaus = _pileTableaus[i];
                    Card topCard = pileTableaus.TopCard();
                    cardsInTableaus += pileTableaus.Cards.Count;

                    if (topCard == null)
                    {
                        continue;
                    }

                    Pile pileTarget = CheckPilesForMove(_pileFoundations, topCard);

                    if (pileTarget == null)
                    {
                        continue;
                    }

                    InteractCard(topCard, pileTarget);
                    await UniTask.Delay(100);
                }

            }
            while (cardsInTableaus > 0);

            UnityEngine.Debug.Log("WON");//TODO: more test needed
        }

        private void Restart()
        {
            Reset();
            DealAsync().Forget();
        }

        private void NewMatch()
        {
            Reset();
            ShuffleCards();
            DealAsync().Forget();
        }

        private void Continue()
        {
            _gameState.State.Value = State.Playing;
        }
    }
}
