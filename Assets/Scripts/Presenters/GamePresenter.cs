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
    
        [Inject] readonly Game _game;
        [Inject] readonly GameState _gameState;
        [Inject] readonly OrientationState _orientation;
        [Inject] readonly AudioService _audioService;

        Camera _camera;

        const float CamSizeLandscape = 4.25f;
        const float CamSizePortrait = 8.25f;

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Start()
        {
            // Update camera on orientation change
            _orientation.State.Subscribe(AdjustCamera).AddTo(this);

            // Enable card interactions only while playing
            _gameState.State.Subscribe(ManageCardRaycaster).AddTo(this);

            // Manage music based on game state
            _gameState.State.Pairwise().Subscribe(ManageMusic).AddTo(this);

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
                CamSizeLandscape : CamSizePortrait);
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
