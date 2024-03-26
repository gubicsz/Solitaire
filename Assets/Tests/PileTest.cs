using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Tests
{
    [TestFixture]
    public class PileTest : ZenjectUnitTestFixture
    {
        [SetUp]
        public void CommonInstall()
        {
            Container.Bind<Pile>().AsSingle();
            Container.Bind<Card>().AsSingle();
            Container.Inject(this);
        }

        [Inject]
        readonly Pile _pile;

        [Inject]
        readonly Card _card;

        [Test]
        public void Should_InitCards_When_Created()
        {
            Assert.That(_pile.Cards != null && _pile.Cards.Count == 0);
        }

        [Test]
        public void Should_HaveCards_When_CardCountIsGreaterThanZero()
        {
            _pile.AddCard(new Card());

            Assert.That(_pile.HasCards);
        }

        [Test]
        public void Should_BeStock_When_TypeIsStock()
        {
            _pile.Init(Pile.PileType.Stock, Pile.CardArrangement.Stack, Vector3.zero);

            Assert.That(_pile.IsStock);
        }

        [Test]
        public void Should_BeWaste_When_TypeIsWaste()
        {
            _pile.Init(Pile.PileType.Waste, Pile.CardArrangement.TopThree, Vector3.zero);

            Assert.That(_pile.IsWaste);
        }

        [Test]
        public void Should_BeTableau_When_TypeIsTableau()
        {
            _pile.Init(Pile.PileType.Tableau, Pile.CardArrangement.Waterfall, Vector3.zero);

            Assert.That(_pile.IsTableau);
        }

        [Test]
        public void Should_BeFoundation_When_TypeIsFoundation()
        {
            _pile.Init(Pile.PileType.Foundation, Pile.CardArrangement.Stack, Vector3.zero);

            Assert.That(_pile.IsFoundation);
        }

        [Test]
        public void Should_SetCorrectValues_When_Initialized()
        {
            Pile.PileType type = Pile.PileType.Stock;
            Pile.CardArrangement arrangement = Pile.CardArrangement.Stack;
            Vector3 position = Vector3.zero;

            _pile.Init(type, arrangement, position);

            Assert.That(_pile.Type == type);
            Assert.That(_pile.Arrangement == arrangement);
            Assert.That(_pile.Position == position);
        }

        [Test]
        public void Should_ClearCards_When_Reseted()
        {
            _pile.AddCard(new Card());

            _pile.Reset();

            Assert.That(_pile.HasCards == false);
        }

        [Test]
        public void Should_ReturnFirstCard_When_BottomCardIsRequestedAndHasCards()
        {
            _pile.AddCard(_card);
            _pile.AddCard(new Card());

            var card = _pile.BottomCard();

            Assert.That(card == _card);
        }

        [Test]
        public void Should_ReturnNull_When_BottomCardIsRequestedAndHasNoCards()
        {
            var card = _pile.BottomCard();

            Assert.That(card == null);
        }

        [Test]
        public void Should_ReturnLastCard_When_TopCardIsRequestedAndHasCards()
        {
            _pile.AddCard(new Card());
            _pile.AddCard(_card);

            var card = _pile.TopCard();

            Assert.That(card == _card);
        }

        [Test]
        public void Should_ReturnNull_When_TopCardIsRequestedAndHasNoCards()
        {
            var card = _pile.TopCard();

            Assert.That(card == null);
        }

        [Test]
        [TestCase(Pile.PileType.Stock)]
        [TestCase(Pile.PileType.Waste)]
        [TestCase(Pile.PileType.Foundation)]
        [TestCase(Pile.PileType.Tableau)]
        public void Should_ReturnFalse_When_TryingToAddCardToTheWrongPileOrConditionsAreNotMet(
            Pile.PileType type
        )
        {
            _card.Init(Card.Suits.Spade, Card.Types.Two);
            _pile.Init(type, Pile.CardArrangement.Stack, Vector3.zero);

            bool result = _pile.CanAddCard(_card);

            Assert.That(result == false);
        }

        [Test]
        public void Should_ReturnTrue_When_TryingToAddAnAceToAnEmptyFoundationPile()
        {
            _card.Init(Card.Suits.Spade, Card.Types.Ace);
            _pile.Init(Pile.PileType.Foundation, Pile.CardArrangement.Stack, Vector3.zero);

            bool result = _pile.CanAddCard(_card);

            Assert.That(result == true);
        }

        [Test]
        public void Should_ReturnTrue_When_TryingToAddATopCardOfTheSameSuitAndHigherTypeToAFoundationPile()
        {
            var card1 = new Card();
            card1.Init(Card.Suits.Spade, Card.Types.Ace);

            var pileFoundation = new Pile();
            pileFoundation.Init(Pile.PileType.Foundation, Pile.CardArrangement.Stack, Vector3.zero);
            pileFoundation.AddCard(card1);

            var card2 = new Card();
            card2.Init(Card.Suits.Spade, Card.Types.Two);

            var pileTableau = new Pile();
            pileTableau.Init(Pile.PileType.Tableau, Pile.CardArrangement.Waterfall, Vector3.zero);
            pileTableau.AddCard(card2);

            bool result = pileFoundation.CanAddCard(card2);

            Assert.That(result == true);
        }

        [Test]
        public void Should_ReturnTrue_When_TryingToAddAKingToAnEmptyTableauPile()
        {
            _card.Init(Card.Suits.Spade, Card.Types.King);
            _pile.Init(Pile.PileType.Tableau, Pile.CardArrangement.Waterfall, Vector3.zero);

            bool result = _pile.CanAddCard(_card);

            Assert.That(result == true);
        }

        [Test]
        [TestCase(Card.Suits.Spade, Card.Suits.Diamond)]
        [TestCase(Card.Suits.Spade, Card.Suits.Heart)]
        [TestCase(Card.Suits.Club, Card.Suits.Diamond)]
        [TestCase(Card.Suits.Club, Card.Suits.Heart)]
        public void Should_ReturnTrue_When_TryingToAddACardOfTheOppositeColorAndLowerTypeToATableauPile(
            Card.Suits suitBlack,
            Card.Suits suitRed
        )
        {
            var card = new Card();
            card.Init(suitBlack, Card.Types.King);

            _pile.Init(Pile.PileType.Tableau, Pile.CardArrangement.Waterfall, Vector3.zero);
            _pile.AddCard(card);

            _card.Init(suitRed, Card.Types.Queen);

            bool result = _pile.CanAddCard(_card);

            Assert.That(result == true);
        }

        [Test]
        public void Should_NotAddCard_When_AddingNull()
        {
            _pile.AddCard(null);

            Assert.That(_pile.HasCards == false);
        }

        [Test]
        public void Should_RemoveCardFromOtherPile_When_AddingCard()
        {
            var pile = new Pile();
            pile.AddCard(_card);

            _pile.AddCard(_card);

            Assert.That(pile.HasCards == false);
        }

        [Test]
        public void Should_AddCardAndSetPile_When_AddingCard()
        {
            _pile.AddCard(_card);

            Assert.That(_pile.HasCards == true);
            Assert.That(_card.Pile == _pile);
        }

        [Test]
        public void Should_SetOrder_When_AddingCard()
        {
            _pile.Init(Pile.PileType.Foundation, Pile.CardArrangement.Stack, Vector3.zero);

            for (int i = 0; i < 3; i++)
            {
                _pile.AddCard(new Card());
            }

            for (int i = 0; i < 3; i++)
            {
                Assert.That(_pile.Cards[i].Order.Value == i);
            }
        }

        [Test]
        public void Should_SetPosition_When_AddingCard()
        {
            _card.Position.Value = Vector3.zero;
            _pile.Init(Pile.PileType.Foundation, Pile.CardArrangement.Stack, Vector3.up);
            _pile.AddCard(_card);

            Assert.That(_card.Position.Value != Vector3.zero);
        }

        [Test]
        public void Should_SetVisibilityOfThirdCardFromTop_When_AddingCardWithStackArrangement()
        {
            _pile.Init(Pile.PileType.Stock, Pile.CardArrangement.Stack, Vector3.up);

            for (int i = 0; i < 3; i++)
            {
                _pile.AddCard(new Card());
            }

            for (int i = 1; i <= 3; i++)
            {
                var card = _pile.Cards[^i];
                Assert.That(card.IsVisible.Value == i < 3);
                Assert.That(card.IsInteractable.Value == i < 3);
            }
        }

        [Test]
        public void Should_SetVisibilityOfSeventhCardFromTop_When_AddingCardWithTopThreeArrangement()
        {
            _pile.Init(Pile.PileType.Waste, Pile.CardArrangement.TopThree, Vector3.up);

            for (int i = 0; i < 7; i++)
            {
                _pile.AddCard(new Card());
            }

            for (int i = 1; i <= 7; i++)
            {
                var card = _pile.Cards[^i];
                Assert.That(card.IsVisible.Value == i < 7);
                Assert.That(card.IsInteractable.Value == i < 7);
            }
        }

        [Test]
        public void Should_KeepCard_When_RemovingNull()
        {
            _pile.AddCard(_card);

            _pile.RemoveCard(null);

            Assert.That(_card.Pile == _pile);
        }

        [Test]
        public void Should_RemoveCardAndSetPile_When_RemovingCard()
        {
            _pile.AddCard(_card);

            _pile.RemoveCard(_card);

            Assert.That(_pile.HasCards == false);
            Assert.That(_card.IsInPile == false);
        }

        [Test]
        public void Should_SetVisibilityOfSecondCardFromTop_When_RemovingCardWithStackArrangement()
        {
            _pile.Init(Pile.PileType.Stock, Pile.CardArrangement.Stack, Vector3.up);

            for (int i = 0; i < 3; i++)
            {
                _pile.AddCard(new Card());
            }

            _pile.RemoveCard(_pile.TopCard());

            for (int i = 1; i <= 2; i++)
            {
                var card = _pile.Cards[^i];
                Assert.That(card.IsVisible.Value == true);
                Assert.That(card.IsInteractable.Value == true);
            }
        }

        [Test]
        public void Should_SetVisibilityOfForthCardFromTop_When_RemovingCardWithTopThreeArrangement()
        {
            _pile.Init(Pile.PileType.Waste, Pile.CardArrangement.TopThree, Vector3.up);

            for (int i = 0; i < 4; i++)
            {
                _pile.AddCard(new Card());
            }

            _pile.RemoveCard(_pile.TopCard());

            for (int i = 1; i <= 3; i++)
            {
                var card = _pile.Cards[^i];
                Assert.That(card.IsVisible.Value == true);
                Assert.That(card.IsInteractable.Value == true);
            }
        }

        [Test]
        public void Should_ContainTheCards_When_AddingThem()
        {
            var cards = new List<Card>();

            for (int i = 0; i < 3; i++)
            {
                cards.Add(new Card());
            }

            _pile.AddCard(new Card());
            _pile.AddCards(cards);

            CollectionAssert.IsSupersetOf(_pile.Cards, cards);
        }

        [Test]
        public void Should_NotContainTheCards_When_RemovingThem()
        {
            var cards = new List<Card>();

            for (int i = 0; i < 3; i++)
            {
                cards.Add(new Card());
            }

            _pile.AddCard(new Card());
            _pile.AddCards(cards);

            _pile.RemoveCards(cards);

            CollectionAssert.DoesNotContain(_pile.Cards, cards);
        }

        [Test]
        public void Should_ReturnAllFollowingCards_When_SplittingAtACard()
        {
            int indexToSplitAt = 1;
            var cards = new List<Card>();

            for (int i = 0; i < 3; i++)
            {
                cards.Add(new Card());
            }

            _pile.AddCards(cards);

            var split = _pile.SplitAt(cards[indexToSplitAt]);

            CollectionAssert.AreEqual(split, _pile.Cards.Skip(indexToSplitAt));
        }

        [Test]
        public void Should_ChangePositionOfAllCards_When_UpdatingPosition()
        {
            var cards = new List<Card>();

            for (int i = 0; i < 3; i++)
            {
                cards.Add(new Card());
            }

            _pile.AddCards(cards);
            _pile.UpdatePosition(Vector3.up);

            foreach (Card card in _pile.Cards)
            {
                Assert.That(card.Position.Value != Vector3.zero);
            }
        }

        [Test]
        public void Should_ReturnCorrectValue_When_CalculatingPositionForStackArrangement()
        {
            _pile.Init(Pile.PileType.Waste, Pile.CardArrangement.Stack, Vector3.zero);

            for (int i = 0; i < 3; i++)
            {
                Vector3 result = _pile.CalculateCardPosition(i, 0, null);
                Vector3 expected = _pile.Position + _pile.OffsetDepth * (i + 1) * Vector3.back;
                Assert.That(result == expected);
            }
        }

        [Test]
        public void Should_ReturnCorrectString_When_TypeIsSet()
        {
            _pile.Init(Pile.PileType.Waste, Pile.CardArrangement.Stack, Vector3.zero);
            string expected = $"{_pile.Type}";

            string result = _pile.ToString();

            Assert.That(result == expected);
        }
    }
}
