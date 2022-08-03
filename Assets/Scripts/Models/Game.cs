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

        public enum Popup
        {
            None,
            Match,
            Options,
            Leaderboard,
        }

        public BoolReactiveProperty HasStarted { get; private set; }
        public ReactiveCommand RestartCommand { get; private set; }
        public ReactiveCommand NewMatchCommand { get; private set; }
        public ReactiveCommand ContinueCommand { get; private set; }

        public Pile PileStock { get; private set; }
        public Pile PileWaste { get; private set; }
        public IList<Pile> PileFoundations { get; private set; }
        public IList<Pile> PileTableaus { get; private set; }
        public IList<Card> Cards { get; private set; }

        [Inject] readonly ICardSpawner _cardSpawner;
        [Inject] readonly ICommandService _commandService;
        [Inject] readonly IMovesService _movesService;
        [Inject] readonly IPointsService _pointsService;
        [Inject] readonly IHintService _hintService;
        [Inject] readonly IAudioService _audioService;
        [Inject] readonly Card.Config _cardConfig;
        [Inject] readonly GameState _gameState;
        [Inject] readonly GamePopup _gamePopup;
        [Inject] readonly Leaderboard _leaderboard;
        [Inject] readonly DrawCardCommand.Factory _drawCardCommandFactory;
        [Inject] readonly MoveCardCommand.Factory _moveCardCommandFactory;
        [Inject] readonly RefillStockCommand.Factory _refillStockCommandFactory;

        public Game()
        {
            HasStarted = new BoolReactiveProperty(false);

            RestartCommand = new ReactiveCommand();
            RestartCommand.Subscribe(_ => Restart()).AddTo(this);

            NewMatchCommand = new ReactiveCommand();
            NewMatchCommand.Subscribe(_ => NewMatch()).AddTo(this);

            ContinueCommand = new ReactiveCommand(HasStarted);
            ContinueCommand.Subscribe(_ => Continue()).AddTo(this);
        }

        public void Init(Pile pileStock, Pile pileWaste, 
            IList<Pile> pileFoundations, IList<Pile> pileTableaus)
        {
            PileStock = pileStock;
            PileWaste = pileWaste;
            PileFoundations = pileFoundations;
            PileTableaus = pileTableaus;

            SpawnCards();
            LoadLeaderboard();
        }

        public void RefillStock()
        {
            if (PileStock.HasCards || !PileWaste.HasCards)
            {
                PlayErrorSfx();
                return;
            }

            // Refill stock pile from waste pile
            var command = _refillStockCommandFactory.Create(PileStock, PileWaste);
            command.Execute();
            _commandService.Add(command);
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
                PlayErrorSfx();
                return;
            }

            // Move card to pile
            var command = _moveCardCommandFactory.Create(card, card.Pile, pile);
            command.Execute();
            _commandService.Add(command);
            _movesService.Increment();
        }

        public void DrawCard()
        {
            // Draw card(s) from stock
            var command = _drawCardCommandFactory.Create(PileStock, PileWaste);
            command.Execute();
            _commandService.Add(command);
            _movesService.Increment();
        }

        public void PlayErrorSfx()
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

            HasStarted.Value = true;
            _gamePopup.State.Value = Popup.None;
        }

        private void ShuffleCards()
        {
            //// Shuffle by sorting
            //Cards = Cards.OrderBy(a => Guid.NewGuid()).ToList();

            // Fisher-Yates shuffle
            for (int i = Cards.Count - 1; i > 0; i--)
            {
                int n = UnityEngine.Random.Range(0, i + 1);
                Card temp = Cards[i];
                Cards[i] = Cards[n];
                Cards[n] = temp;
            }
        }

        private async UniTask DealAsync()
        {
            // Start dealing
            _gameState.State.Value = State.Dealing;
            int delayMs = (int)(_cardConfig.AnimationDuration * 1000) + 50;

            _audioService.PlaySfx(Audio.SfxShuffle, 0.5f);
            await UniTask.Delay(delayMs);

            // Add cards to the stock pile
            PileStock.AddCards(Cards);

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
            HasStarted.Value = false;

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

            AddPointsAndSaveLeaderboard();
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
            _gamePopup.State.Value = Popup.None;
        }

        private void SpawnCards()
        {
            _cardSpawner.SpawnAll();
            Cards = _cardSpawner.Cards.Select(c => c.Card).ToList();
        }

        private void AddPointsAndSaveLeaderboard()
        {
            var item = new Leaderboard.Item()
            {
                Points = _pointsService.Points.Value,
                Date = DateTime.Now.ToString("HH:mm MM/dd/yyyy"),
            };

            _leaderboard.Add(item);
            _leaderboard.Save();
        }

        private void LoadLeaderboard()
        {
            _leaderboard.Load();
        }
    }
}
