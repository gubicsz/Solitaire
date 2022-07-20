using Cysharp.Threading.Tasks;
using Solitaire.Models;
using Solitaire.Services;
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
        [Inject] AudioService _audioService;

        Camera _camera;

        const float _camSizeLandscape = 4.25f;
        const float _camSizePortrait = 8.25f;

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

            // Manage music based on game state
            _gameState.State.Pairwise().Subscribe(pair => ManageMusic(pair)).AddTo(this);

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
            _camera.orthographicSize = (orientation == Orientation.Landscape ? 
                _camSizeLandscape : _camSizePortrait);
        }

        private void ManageCardRaycaster(Game.State gameState)
        {
            _cardRaycaster.enabled = gameState == Game.State.Playing;
        }

        private void ManageMusic(Pair<Game.State> pair)
        {
            if (pair.Previous == Game.State.Home)
            {
                _audioService.PlayMusic(Audio.Music, 0.3333f);
            }
            else if (pair.Current == Game.State.Home)
            {
                _audioService.StopMusic();
            }
        }
    }
}
