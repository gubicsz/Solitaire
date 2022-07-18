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

        public Pile PileStock { get; private set; }
        public Pile PileWaste { get; private set; }
        public IList<Pile> PileFoundations { get; private set; }
        public IList<Pile> PileTableaus { get; private set; }
        public IList<Card> Cards { get; private set; }

        [Inject] CardSpawner _cardSpawner;
        [Inject] CommandService _commandService;
        [Inject] MovesService _movesService;
        [Inject] PointsService _pointsService;
        [Inject] HintService _hintService;
        [Inject] AudioService _audioService;
        [Inject] Game.Config _gameConfig;
        [Inject] Card.Config _cardConfig;
        [Inject] GameState _gameState;

        BoolReactiveProperty _hasStarted = new BoolReactiveProperty();

        public Game()
        {
            RestartCommand = new ReactiveCommand();
            RestartCommand.Subscribe(_ => Restart()).AddTo(this);

            NewMatchCommand = new ReactiveCommand();
            NewMatchCommand.Subscribe(_ => NewMatch()).AddTo(this);

            ContinueCommand = new ReactiveCommand(_hasStarted);
            ContinueCommand.Subscribe(_ => Continue()).AddTo(this);
        }

        public void Init(Pile pileStock, Pile pileWaste, 
            IList<Pile> pileFoundations, IList<Pile> pileTableaus)
        {
            // Set references
            PileStock = pileStock;
            PileWaste = pileWaste;
            PileFoundations = pileFoundations;
            PileTableaus = pileTableaus;

            // Spawn cards
            _cardSpawner.SpawnAll();
            Cards = _cardSpawner.Cards.Select(c => c.Card).ToList();
        }

        public void RefillStock()
        {
            if (PileStock.HasCards || !PileWaste.HasCards)
            {
                IndicateError();
                return;
            }

            // Refill stock pile from waste pile
            var command = new RefillStockCommand(PileStock, PileWaste, _pointsService, _audioService, _gameConfig);
            command.Execute();
            _commandService.AddCommand(command);
            _movesService.Increment();
        }

        public void MoveCard(Card card, Pile pile)
        {
            if (card == null)
            {
                return;
            }

            // Try to find valid move for the card
            if (pile == null)
            {
                pile = _hintService.FindValidMove(card);
            }

            // Couldn't find move
            if (pile == null)
            {
                IndicateError();
                return;
            }

            // Move card to pile
            var command = new MoveCardCommand(card, pile, _pointsService, _audioService, _gameConfig);
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
            var command = new DrawCardCommand(card, PileStock, PileWaste, _audioService);
            command.Execute();
            _commandService.AddCommand(command);
            _movesService.Increment();
        }

        public void IndicateError()
        {
            _audioService.PlaySfx(Audio.SfxError, 0.5f);
        }

        public void DetectWinCondition()
        {
            // All cards in the tableau piles should be revelead
            for (int i = 0; i < PileTableaus.Count; i++)
            {
                Pile pileTableau = PileTableaus[i];

                for (int j = 0; j < pileTableau.Cards.Count; j++)
                {
                    if (!pileTableau.Cards[j].IsFaceUp.Value)
                    {
                        return;
                    }
                }
            }

            // The stock and waste piles should be empty
            if (PileStock.HasCards || PileWaste.HasCards)
            {
                return;
            }

            WinAsync().Forget();
        }

        public async UniTask TryShowHintAsync()
        {
            // Try to get hint
            Hint hint = _hintService.GetHint();

            if (hint == null)
            {
                return;
            }

            // Make copies of the original card and all cards above it
            var pile = hint.Pile;
            var cardsToCopy = hint.Card.Pile.SplitAt(hint.Card);
            var copies = _cardSpawner.MakeCopies(cardsToCopy);

            // Initialize copies without adding them to the pile
            for (int i = 0; i < copies.Count; i++)
            {
                // Calculate order
                var copy = copies[i];
                int index = pile.Cards.Count + i;
                copy.Card.Order.Value = index;

                // Calculate position
                int count = pile.Cards.Count + 1 + i;
                Card prevCard = i > 0 ? copies[i - 1].Card : (pile.HasCards ? pile.Cards[index - 1] : null);
                copy.Card.Position.Value = pile.CalculateCardPosition(index, count, prevCard);
            }

            _audioService.PlaySfx(Audio.SfxHint, 0.5f);

            // Wait until the animation completes
            int delayMs = (int)(_cardConfig.AnimationDuration * 1000) + 50;
            await UniTask.Delay(delayMs);

            // Despawn copies
            for (int i = 0; i < copies.Count; i++)
            {
                _cardSpawner.Despawn(copies[i]);
            }

            await UniTask.Delay(delayMs);
        }

        private void Reset()
        {
            // Reset piles
            PileStock.Reset();
            PileWaste.Reset();

            for (int i = 0; i < PileFoundations.Count; i++)
            {
                PileFoundations[i].Reset();
            }

            for (int i = 0; i < PileTableaus.Count; i++)
            {
                PileTableaus[i].Reset();
            }

            // Reset cards
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].Reset(PileStock.Position);
            }

            // Reset services
            _movesService.Reset();
            _pointsService.Reset();
            _commandService.Reset();

            _hasStarted.Value = true;
        }

        private void ShuffleCards()
        {
            Cards = Cards.OrderBy(a => Guid.NewGuid()).ToList();
        }

        private async UniTask DealAsync()
        {
            // Start delaing
            _gameState.State.Value = State.Dealing;

            // Play shuffle sfx
            _audioService.PlaySfx(Audio.SfxShuffle, 0.5f);
            int delayMs = (int)(_cardConfig.AnimationDuration * 1000) + 50;
            await UniTask.Delay(delayMs);

            // Add cards to the stock pile
            PileStock.AddCards(Cards);

            // Play deal sfx
            _audioService.PlaySfx(Audio.SfxDeal, 1.0f);
            await UniTask.Delay(delayMs);

            // Deal cards to the Tableau piles
            for (int i = 0; i < PileTableaus.Count; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    Card topCard = PileStock.TopCard();
                    PileTableaus[i].AddCard(topCard);

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
            _hasStarted.Value = false;

            int cardsInTableaus;

            do
            {
                cardsInTableaus = 0;

                // Check each tableau pile
                for (int i = 0; i < PileTableaus.Count; i++)
                {
                    Pile pileTableau = PileTableaus[i];
                    Card topCard = pileTableau.TopCard();
                    cardsInTableaus += pileTableau.Cards.Count;

                    // Skip empty pile
                    if (topCard == null)
                    {
                        continue;
                    }

                    // Skip card that cannot be moved to a foundation pile
                    Pile pileFoundation = _hintService.CheckPilesForMove(PileFoundations, topCard);

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
            _gameState.State.Value = State.Playing;
        }
    }
}
