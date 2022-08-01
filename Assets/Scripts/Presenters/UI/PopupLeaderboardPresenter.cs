using Solitaire.Helpers;
using Solitaire.Models;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class PopupLeaderboardPresenter : OrientationAwarePresenter
    {
        [SerializeField] Button _buttonClose;
        [SerializeField] GameObject _labelEmpty;
        [SerializeField] RectTransform _panelRect;
        [SerializeField] List<LeaderboardItemPresenter> _itemPresenters;

        [Inject] readonly Leaderboard _leaderboard;

        protected override void Start()
        {
            base.Start();

            _leaderboard.CloseCommand.BindTo(_buttonClose).AddTo(this);

            _leaderboard.Items.ObserveCountChanged(true).Subscribe(count =>
            {
                for (int i = 0; i < _itemPresenters.Count; i++)
                {
                    Leaderboard.Item item = i < _leaderboard.Items.Count ? _leaderboard.Items[i] : null;
                    LeaderboardItemPresenter itemPresenter = _itemPresenters[i];
                    itemPresenter.Initialize(item, i);
                }

                _labelEmpty.SetActive(count == 0);
            }).AddTo(this);
        }

        protected override void OnOrientationChanged(bool isLandscape)
        {
            _panelRect.offsetMin = isLandscape ? new Vector2(250, 200) : new Vector2(150, 250);
            _panelRect.offsetMax = isLandscape ? new Vector2(-250, -200) : new Vector2(-150, -250);

            for (int i = 0; i < _itemPresenters.Count; i++)
            {
                LeaderboardItemPresenter itemPresenter = _itemPresenters[i];
                Vector2 size = itemPresenter.GetSize();

                if (isLandscape)
                {
                    // 3x3 grid in landscape
                    itemPresenter.UpdatePosition(new Vector2(i < 3 ? -size.x : i > 5 ? size.x : 0, size.y - (i % 3) * size.y));
                }
                else
                {
                    // 9 vertical items in portrait
                    itemPresenter.UpdatePosition(new Vector2(0f, (4 - i) * size.y));
                }
            }
        }
    }
}
