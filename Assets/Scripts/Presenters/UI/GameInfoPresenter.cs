using Solitaire.Services;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Solitaire.Presenters
{
    public class GameInfoPresenter : OrientationAwarePresenter
    {
        [SerializeField] TextMeshProUGUI _labelPoints;
        [SerializeField] TextMeshProUGUI _labelMoves;
        [SerializeField] RectTransform _rectPoints;
        [SerializeField] RectTransform _rectTime;
        [SerializeField] RectTransform _rectMoves;

        [Inject] readonly IPointsService _pointsService;
        [Inject] readonly IMovesService _movesService;

        protected override void Start()
        {
            base.Start();

            _pointsService.Points.SubscribeToText(_labelPoints).AddTo(this);
            _movesService.Moves.SubscribeToText(_labelMoves).AddTo(this);
        }

        protected override void OnOrientationChanged(bool isLandscape)
        {
            _rectPoints.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, isLandscape ? 50 : 0, _rectPoints.sizeDelta.x);
            _rectPoints.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, isLandscape ? 50 : 0, _rectPoints.sizeDelta.y);

            _rectMoves.anchorMin = isLandscape ? Vector2.zero : Vector2.one;
            _rectMoves.anchorMax = isLandscape ? Vector2.zero : Vector2.one;
            _rectMoves.pivot = isLandscape ? Vector2.zero : Vector2.one;
            _rectMoves.SetInsetAndSizeFromParentEdge(isLandscape ? RectTransform.Edge.Left : RectTransform.Edge.Right, isLandscape ? 50 : 0, _rectMoves.sizeDelta.x);
            _rectMoves.SetInsetAndSizeFromParentEdge(isLandscape ? RectTransform.Edge.Bottom : RectTransform.Edge.Top, isLandscape ? 120 : 0, _rectMoves.sizeDelta.y);
        }
    }
}
