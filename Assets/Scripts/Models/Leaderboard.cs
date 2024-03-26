using System;
using System.Collections.Generic;
using System.Linq;
using Solitaire.Helpers;
using Solitaire.Services;
using UniRx;

namespace Solitaire.Models
{
    public class Leaderboard : DisposableEntity, IComparer<Leaderboard.Item>
    {
        private const string StorageKey = "Leaderboard";
        private const int MaxCount = 9;
        private readonly GamePopup _gamePopup;

        private readonly GameState _gameState;
        private readonly IStorageService _storageService;

        public Leaderboard(GameState gameState, GamePopup gamePopup, IStorageService storageService)
        {
            _gameState = gameState;
            _gamePopup = gamePopup;
            _storageService = storageService;

            Items = new ReactiveCollection<Item>();
            CloseCommand = new ReactiveCommand(
                gamePopup.State.Select(s => s == Game.Popup.Leaderboard)
            );
            CloseCommand.Subscribe(_ => Close()).AddTo(this);
        }

        public ReactiveCollection<Item> Items { get; }
        public ReactiveCommand CloseCommand { get; }

        public int Compare(Item x, Item y)
        {
            return x.CompareTo(y);
        }

        public void Add(Item leaderboardItem)
        {
            // Add new item to the original list
            var originalList = Items.ToList();
            originalList.Add(leaderboardItem);

            // Order items and update leaderboard
            var orderedList = originalList.OrderByDescending(x => x, this).Take(MaxCount).ToList();

            // Update leaderboard
            if (orderedList != null)
                UpdateLeaderboard(orderedList);
        }

        public void Save()
        {
            var leaderboard = new Data { Items = Items.ToList() };

            // Save leaderboard to the device
            _storageService.Save(StorageKey, leaderboard);
        }

        public void Load()
        {
            // Load leaderboard from the device
            var leaderboard = _storageService.Load<Data>(StorageKey);

            // Update leaderboard
            if (leaderboard != null)
                UpdateLeaderboard(leaderboard.Items);
        }

        private void UpdateLeaderboard(IList<Item> items)
        {
            // Handle error
            if (items == null)
                return;

            // Clear reactive list
            Items.Clear();

            // Add items one by one
            for (var i = 0; i < items.Count; i++)
                Items.Add(items[i]);
        }

        private void Close()
        {
            if (_gameState.State.Value == Game.State.Paused)
            {
                _gameState.State.Value = Game.State.Playing;
                _gamePopup.State.Value = Game.Popup.None;
            }
            else
            {
                _gamePopup.State.Value = Game.Popup.None;
            }
        }

        [Serializable]
        public class Item : IComparable<Item>
        {
            public int Points;
            public string Date;

            public int CompareTo(Item other)
            {
                var comparePoints = Points.CompareTo(other.Points);

                if (comparePoints == 0)
                    return Date.CompareTo(other.Date);

                return comparePoints;
            }
        }

        [Serializable]
        public class Data
        {
            public List<Item> Items;
        }
    }
}
