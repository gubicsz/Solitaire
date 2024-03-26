using NUnit.Framework;
using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Tests
{
    [TestFixture]
    public class CardTest : ZenjectUnitTestFixture
    {
        [SetUp]
        public void CommonInstall()
        {
            Container.Bind<Card>().AsSingle();
            Container.Bind<Pile>().AsSingle();
            Container.Inject(this);
        }

        [Inject]
        private readonly Card _card;

        [Inject]
        private readonly Pile _pile;

        [Test]
        public void Should_InitReactiveProperties_When_Created()
        {
            Assert.That(_card.IsFaceUp != null);
            Assert.That(_card.Position != null);
            Assert.That(_card.Order != null);
            Assert.That(_card.Alpha != null);
            Assert.That(_card.IsVisible != null);
            Assert.That(_card.IsInteractable != null);
        }

        [Test]
        public void Should_BeInPile_When_PileIsNotNull()
        {
            _card.Pile = _pile;

            Assert.That(_card.IsInPile);
        }

        [Test]
        public void Should_BeOnBottom_When_ItIsFirstCardOfAPile()
        {
            _pile.AddCard(_card);
            _pile.AddCard(new Card());

            Assert.That(_card.IsOnBottom);
        }

        [Test]
        public void Should_BeOnTop_When_ItIsLastCardOfAPile()
        {
            _pile.AddCard(new Card());
            _pile.AddCard(_card);

            Assert.That(_card.IsOnTop);
        }

        [Test]
        public void Should_BeMovable_When_ItIsFacingUpOnTopOfTheWastePile()
        {
            _pile.Init(Pile.PileType.Waste, Pile.CardArrangement.TopThree, Vector3.zero);
            _pile.AddCard(new Card());
            _pile.AddCard(_card);
            _card.Flip();

            Assert.That(_card.IsMoveable);
        }

        [Test]
        public void Should_BeMovable_When_ItIsFacingUpAndNotInTheWastePile()
        {
            _pile.Init(Pile.PileType.Tableau, Pile.CardArrangement.Waterfall, Vector3.zero);
            _pile.AddCard(_card);
            _pile.AddCard(new Card());
            _card.Flip();

            Assert.That(_card.IsMoveable);
        }

        [Test]
        public void Should_BeDrawable_When_ItIsFacingDownOnTopOfTheStockPile()
        {
            _pile.Init(Pile.PileType.Stock, Pile.CardArrangement.Stack, Vector3.zero);
            _pile.AddCard(new Card());
            _pile.AddCard(_card);

            Assert.That(_card.IsDrawable);
        }

        [Test]
        public void Should_SetSuitAndType_When_Initialized()
        {
            var suit = Card.Suits.Spade;
            var type = Card.Types.Ace;

            _card.Init(suit, type);

            Assert.That(_card.Suit == suit);
            Assert.That(_card.Type == type);
        }

        [Test]
        public void Should_SetDefaultValues_When_Reseted()
        {
            var position = Vector3.zero;

            _card.Reset(position);

            Assert.That(_card.Pile == null);
            Assert.That(_card.IsFaceUp.Value == false);
            Assert.That(_card.Position.Value == position);
            Assert.That(_card.Order.Value == 0);
            Assert.That(_card.Alpha.Value == 1f);
            Assert.That(_card.IsVisible.Value);
            Assert.That(_card.IsInteractable.Value);
            Assert.That(_card.DragOrigin == Vector3.zero);
            Assert.That(_card.DragOffset == Vector3.zero);
            Assert.That(_card.OrderToRestore == 0);
            Assert.That(_card.IsDragged == false);
        }

        [Test]
        public void Should_NotChangeSuitAndType_When_Reseted()
        {
            var suit = Card.Suits.Spade;
            var type = Card.Types.Ace;

            _card.Init(suit, type);
            _card.Reset(Vector3.zero);

            Assert.That(_card.Suit == suit);
            Assert.That(_card.Type == type);
        }

        [Test]
        public void Should_ChangeFacing_When_Flipped()
        {
            var isFaceUp = _card.IsFaceUp.Value;

            _card.Flip();

            Assert.That(_card.IsFaceUp.Value != isFaceUp);
        }

        [Test]
        public void Should_ReturnCorrectString_When_SuitAndTypeAreSet()
        {
            var expected = $"{_card.Suit} {_card.Type}";

            var result = _card.ToString();

            Assert.That(result == expected);
        }

        [Test]
        [TestCase(Card.Types.Two, 2)]
        [TestCase(Card.Types.Three, 3)]
        [TestCase(Card.Types.Four, 4)]
        [TestCase(Card.Types.Five, 5)]
        [TestCase(Card.Types.Six, 6)]
        [TestCase(Card.Types.Seven, 7)]
        [TestCase(Card.Types.Eight, 8)]
        [TestCase(Card.Types.Nine, 9)]
        [TestCase(Card.Types.Ten, 10)]
        [TestCase(Card.Types.Jack, 10)]
        [TestCase(Card.Types.Queen, 10)]
        [TestCase(Card.Types.King, 10)]
        [TestCase(Card.Types.Ace, 11)]
        public void Should_ReturnCorrectValue_When_TypeIsSet(Card.Types type, int expected)
        {
            _card.Init(Card.Suits.Spade, type);

            var value = _card.GetValue();

            Assert.That(value == expected);
        }
    }
}
