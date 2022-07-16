using Solitaire.Services;
using TMPro;
using UniRx;
using UnityEngine;
using Zenject;

namespace Solitaire.Presenters
{
    public class GameInfoPresenter : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _labelPoints;
        [SerializeField] TextMeshProUGUI _labelMoves;

        [Inject] PointsService _pointsService;
        [Inject] MovesService _movesService;

        private void Start()
        {
            _pointsService.Points.SubscribeToText(_labelPoints).AddTo(this);
            _movesService.Moves.SubscribeToText(_labelMoves).AddTo(this);
        }
    }
}
