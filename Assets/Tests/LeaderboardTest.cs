using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Solitaire.Models;
using Solitaire.Services;
using Zenject;

namespace Solitaire.Tests
{
    [TestFixture]
    public class LeaderboardTest : ZenjectUnitTestFixture
    {
        [SetUp]
        public void CommonInstall()
        {
            Container.Bind<Leaderboard>().AsSingle();
            Container.Bind<GameState>().AsSingle();
            Container.Bind<GamePopup>().AsSingle();
            Container.Bind<IStorageService>().FromSubstitute();
            Container.Inject(this);
        }

        [Inject]
        readonly Leaderboard _leaderboard;

        [Inject]
        readonly GameState _gameState;

        [Inject]
        readonly GamePopup _gamePopup;

        [Inject]
        readonly IStorageService _storageService;

        readonly Leaderboard.Data _testLeaderboard =
            new()
            {
                Items = new List<Leaderboard.Item>()
                {
                    new Leaderboard.Item()
                    {
                        Points = 1230,
                        Date = DateTime.Now.AddDays(-2).ToString("HH:mm, MM/dd/yyyy")
                    },
                    new Leaderboard.Item()
                    {
                        Points = 150,
                        Date = DateTime.Now.AddDays(-1).ToString("HH:mm, MM/dd/yyyy")
                    },
                    new Leaderboard.Item()
                    {
                        Points = 90,
                        Date = DateTime.Now.ToString("HH:mm, MM/dd/yyyy")
                    },
                }
            };

        [Test]
        public void Should_InitializeItems_When_Created()
        {
            Assert.That(_leaderboard.Items != null);
        }

        [Test]
        public void Should_InitializeCloseCommand_When_Created()
        {
            Assert.That(_leaderboard.CloseCommand != null);
            Assert.That(_leaderboard.CloseCommand.CanExecute.Value == false);
        }

        [Test]
        public void Should_BeAbleToExecuteCloseCommand_When_PopupStateIsLeaderboard()
        {
            _gamePopup.State.Value = Game.Popup.Leaderboard;

            Assert.That(_leaderboard.CloseCommand.CanExecute.Value == true);
        }

        [Test]
        public void Should_ClosePopupAndContinuePlaying_When_ExecutingCloseInPausedAppState()
        {
            _gamePopup.State.Value = Game.Popup.Leaderboard;
            _gameState.State.Value = Game.State.Paused;

            _leaderboard.CloseCommand.Execute();

            Assert.That(_gamePopup.State.Value == Game.Popup.None);
            Assert.That(_gameState.State.Value == Game.State.Playing);
        }

        [Test]
        public void Should_ClosePopup_When_ExecutingCloseInHomeAppState()
        {
            _gamePopup.State.Value = Game.Popup.Leaderboard;
            _gameState.State.Value = Game.State.Home;

            _leaderboard.CloseCommand.Execute();

            Assert.That(_gamePopup.State.Value == Game.Popup.None);
            Assert.That(_gameState.State.Value == Game.State.Home);
        }

        [Test]
        public void Should_OrderItems_When_NewItemAdded()
        {
            for (int i = _testLeaderboard.Items.Count - 1; i >= 0; i--)
            {
                _leaderboard.Add(_testLeaderboard.Items[i]);
            }

            CollectionAssert.AreEqual(_testLeaderboard.Items, _leaderboard.Items);
        }

        [Test]
        public void Should_ReturnCorrectValue_When_ComparingTwoItems()
        {
            int firstBiggerThanSecond = _leaderboard.Compare(
                _testLeaderboard.Items[0],
                _testLeaderboard.Items[1]
            );
            int firstSmallerThanSecond = _leaderboard.Compare(
                _testLeaderboard.Items[1],
                _testLeaderboard.Items[0]
            );
            int firstEqualsSecond = _leaderboard.Compare(
                _testLeaderboard.Items[0],
                _testLeaderboard.Items[0]
            );

            Assert.That(firstBiggerThanSecond == 1);
            Assert.That(firstSmallerThanSecond == -1);
            Assert.That(firstEqualsSecond == 0);
        }

        [Test]
        public void Should_CallStorageService_When_Saved()
        {
            _leaderboard.Save();

            _storageService.Received().Save(Arg.Any<string>(), Arg.Any<Leaderboard.Data>());
        }

        [Test]
        public void Should_CallStorageService_WhenLoaded()
        {
            _leaderboard.Load();

            _storageService.Received().Load<Leaderboard.Data>(Arg.Any<string>());
        }

        [Test]
        public void Should_NotUpdateItems_When_LoadingNullData()
        {
            int count = _leaderboard.Items.Count;
            _storageService
                .Load<Leaderboard.Data>(Arg.Any<string>())
                .Returns(default(Leaderboard.Data));

            _leaderboard.Load();

            Assert.That(_leaderboard.Items.Count == count);
        }

        [Test]
        public void Should_UpdateItems_When_LoadingData()
        {
            _storageService.Load<Leaderboard.Data>(Arg.Any<string>()).Returns(_testLeaderboard);

            _leaderboard.Load();

            CollectionAssert.AreEqual(_testLeaderboard.Items, _leaderboard.Items);
        }
    }
}
