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
        bool _hasPlayed;
        bool _hasWon;

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
            if (_pileStock.HasCards || !_pileWaste.HasCards)
            {
                return;
            }

            // Refill stock pile from waste pile
            var command = new RefillStockCommand(_pileStock, _pileWaste, _pointsService, _gameConfig);
            command.Execute();
            _commandService.AddCommand(command);
            _movesService.Increment();
        }

        public void MoveCard(Card card, Pile pile)
        {
            if (card == null || pile == null)
            {
                return;
            }

            // Move card to pile
            var command = new MoveCardCommand(card, pile, _pointsService, _gameConfig);
            command.Execute();
            _commandService.AddCommand(command);
            _movesService.Increment();
        }

        public void DrawCard(Card card)
        {
            if (card == null)
            {
                return;
            }

            // Draw card from stock
            var command = new DrawCardCommand(_pileStock, _pileWaste, card);
            command.Execute();
            _commandService.AddCommand(command);
            _movesService.Increment();
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

            _hasPlayed = true;
            _hasWon = false;
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

                    await UniTask.DelayFrame(3);
                }
            }
            
            // Start playing
            _gameState.State.Value = State.Playing;
        }

        private async UniTask WinAsync()
        {
            // Start win sequence
            _gameState.State.Value = State.Win;
            _hasWon = true;

            int cardsInTableaus;

            do
            {
                cardsInTableaus = 0;

                // Check each tableau pile
                for (int i = 0; i < _pileTableaus.Count; i++)
                {
                    Pile pileTableau = _pileTableaus[i];
                    Card topCard = pileTableau.TopCard();
                    cardsInTableaus += pileTableau.Cards.Count;

                    // Skip empty pile
                    if (topCard == null)
                    {
                        continue;
                    }

                    // Skip card that cannot be moved to a foundation pile
                    Pile pileFoundation = CheckPilesForMove(_pileFoundations, topCard);

                    if (pileFoundation == null)
                    {
                        continue;
                    }

                    // Move card to the foundation
                    MoveCard(topCard, pileFoundation);
                    await UniTask.DelayFrame(3);
                }

            }
            while (cardsInTableaus > 0);
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
            if (_hasPlayed && !_hasWon)
            {
                _gameState.State.Value = State.Playing;
            }
            else
            {
                NewMatch();
            }
        }
    }
}
