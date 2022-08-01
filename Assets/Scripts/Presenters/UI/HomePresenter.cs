using DG.Tweening;
using Solitaire.Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Presenters
{
    public class HomePresenter : OrientationAwarePresenter
    {
        [SerializeField] Button _buttonNewMatch;
        [SerializeField] Button _buttonContinue;
        [SerializeField] Button _buttonOptions;
        [SerializeField] Button _buttonLeaderboard;
        [SerializeField] RectTransform _rectCards;
        [SerializeField] RectTransform _rectSuitsCenter;
        [SerializeField] RectTransform _rectSuitsLeft;
        [SerializeField] RectTransform _rectSuitsRight;

        [Inject] readonly Game _game;
        [Inject] readonly GamePopup _gamePopup;
        [Inject] readonly GameState _gameState;

        RectTransform _rectOptions;
        RectTransform _rectLeaderboard;
        Sequence _sequenceCards;
        Sequence _sequenceSuitsCenter;
        Sequence _sequenceSuitsLeft;
        Sequence _sequenceSuitsRight;

        private void Awake()
        {
            _rectOptions = _buttonOptions.GetComponent<RectTransform>();
            _rectLeaderboard = _buttonLeaderboard.GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            base.Start();

            _game.NewMatchCommand.BindTo(_buttonNewMatch).AddTo(this);
            _game.ContinueCommand.BindTo(_buttonContinue).AddTo(this);
            _gamePopup.OptionsCommand.BindTo(_buttonOptions).AddTo(this);
            _gamePopup.LeaderboardCommand.BindTo(_buttonLeaderboard).AddTo(this);

            // Play animation sequence on state change
            _gameState.State.Where(state => state == Game.State.Home).Subscribe(_ => 
                PlayAnimationSequence(_orientation.State.Value == Orientation.Landscape)).AddTo(this);
        }

        protected override void OnOrientationChanged(bool isLandscape)
        {
            _rectCards.anchoredPosition = isLandscape ? new Vector2(0, -120) : Vector2.zero;

            if (isLandscape)
            {
                _rectOptions.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 50, _rectOptions.sizeDelta.x);
                _rectLeaderboard.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 50, _rectLeaderboard.sizeDelta.x);
            }
            else

            {
                _rectOptions.anchorMin = new Vector2(0.5f, 0f);
                _rectOptions.anchorMax = new Vector2(0.5f, 0f);
                _rectOptions.anchoredPosition = new Vector2(-70f, _rectOptions.sizeDelta.y);

                _rectLeaderboard.anchorMin = new Vector2(0.5f, 0f);
                _rectLeaderboard.anchorMax = new Vector2(0.5f, 0f);
                _rectLeaderboard.anchoredPosition = new Vector2(70f, _rectLeaderboard.sizeDelta.y);
            }

            PlayAnimationSequence(isLandscape);
        }

        private void PlayAnimationSequence(bool isLandscape)
        {
            AnimateCards(ref _sequenceCards);

            if (isLandscape)
            {
                AnimateSuits(ref _sequenceSuitsLeft, _rectSuitsLeft, false);
                AnimateSuits(ref _sequenceSuitsRight, _rectSuitsRight, true);
            }
            else
            {
                AnimateSuits(ref _sequenceSuitsCenter, _rectSuitsCenter, false);
            }
        }

        private void AnimateCards(ref Sequence sequence)
        {
            if (sequence == null)
            {
                sequence = DOTween.Sequence();
                sequence.SetAutoKill(false);

                for (int i = 1; i < _rectCards.childCount; i++)
                {
                    Transform rect = _rectCards.GetChild(i);
                    rect.localEulerAngles = new Vector3(0f, 0f, 37.5f);

                    Tweener tween = rect
                        .DOLocalRotate(new Vector3(0f, 0f, -i * 25f), i * 0.3333f, RotateMode.LocalAxisAdd)
                        .SetEase(Ease.Linear);

                    sequence.Insert(0, tween);
                }
            }
            else
            {
                sequence.Restart();
            }
        }

        private void AnimateSuits(ref Sequence sequence, RectTransform rectSuits, bool isReverse)
        {
            if (sequence == null)
            {
                sequence = DOTween.Sequence();
                sequence.SetAutoKill(false);
                sequence.AppendInterval(1f);

                for (int i = isReverse ? rectSuits.childCount - 1 : 0;
                    isReverse ? i >= 0 : i < rectSuits.childCount;
                    i += isReverse ? -1 : 1)
                {
                    Transform rect = rectSuits.GetChild(i);
                    rect.transform.localScale = Vector3.zero;

                    sequence.Append(rect.DOScale(Vector3.one, 0.125f).SetEase(Ease.InCubic))
                        .Append(rect.DOPunchScale(Vector3.one * 0.5f, 0.125f).SetEase(Ease.OutCubic));
                }
            }
            else
            {
                sequence.Restart();
            }
        }
    }
}
