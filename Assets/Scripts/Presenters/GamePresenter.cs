using Cysharp.Threading.Tasks;
using Solitaire.Models;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Solitaire.Presenters
{
    public class GamePresenter : MonoBehaviour
    {
        [SerializeField] Physics2DRaycaster _cardRaycaster;
        [SerializeField] PilePresenter _pileStock;
        [SerializeField] PilePresenter _pileWaste;
        [SerializeField] PilePresenter[] _pileFoundations;
        [SerializeField] PilePresenter[] _pileTableaus;
    
        [Inject] Game _game;
        [Inject] GameState _gameState;
        [Inject] OrientationState _orientation;

        Camera _camera;    

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Start()
        {
            // Update camera on orientation change
            _orientation.State.Subscribe(orientation => AdjustCamera(orientation)).AddTo(this);

            // Enable card interactions only while playing
            _gameState.State.Subscribe(gameState => ManageCardRaycaster(gameState)).AddTo(this);

            // Initialize game
            _game.Init(_pileStock.Pile, _pileWaste.Pile, 
                _pileFoundations.Select(p => p.Pile).ToList(), 
                _pileTableaus.Select(p => p.Pile).ToList());
        }

        private void Update()
        {
            // Detect win condition
            if (_gameState.State.Value == Game.State.Playing)
            {
                _game.DetectWinCondition();
            }
        }

        private void AdjustCamera(Orientation orientation)
        {
            _camera.orthographicSize = (orientation == Orientation.Landscape ? 4.25f : 8.25f);
        }

        private void ManageCardRaycaster(Game.State gameState)
        {
            _cardRaycaster.enabled = gameState == Game.State.Playing;
        }
    }
}
