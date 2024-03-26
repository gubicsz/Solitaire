using System.Linq;
using Solitaire.Models;
using Solitaire.Services;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Solitaire.Presenters
{
    public class GamePresenter : MonoBehaviour
    {
        private const float CamSizeLandscape = 4.25f;
        private const float CamSizePortrait = 8.25f;

        [SerializeField]
        private Physics2DRaycaster _cardRaycaster;

        [SerializeField]
        private PilePresenter _pileStock;

        [SerializeField]
        private PilePresenter _pileWaste;

        [SerializeField]
        private PilePresenter[] _pileFoundations;

        [SerializeField]
        private PilePresenter[] _pileTableaus;

        [Inject]
        private readonly IAudioService _audioService;

        [Inject]
        private readonly Game _game;

        [Inject]
        private readonly GameState _gameState;

        [Inject]
        private readonly OrientationState _orientation;

        private Camera _camera;
        private int _layerInteractable;

        private void Awake()
        {
            _camera = Camera.main;
            _layerInteractable = LayerMask.NameToLayer("Interactable");
        }

        private void Start()
        {
            // Update camera on orientation change
            _orientation.State.Subscribe(AdjustCamera).AddTo(this);

            // Handle game state change
            _gameState.State.Pairwise().Subscribe(HandleGameStateChanges).AddTo(this);

            // Initialize game
            _game.Init(
                _pileStock.Pile,
                _pileWaste.Pile,
                _pileFoundations.Select(p => p.Pile).ToList(),
                _pileTableaus.Select(p => p.Pile).ToList()
            );

            SetCameraLayers(true);
        }

        private void Update()
        {
            // Detect win condition
            if (_gameState.State.Value == Game.State.Playing)
                _game.DetectWinCondition();
        }

        private void AdjustCamera(Orientation orientation)
        {
            _camera.orthographicSize =
                orientation == Orientation.Landscape ? CamSizeLandscape : CamSizePortrait;
        }

        private void HandleGameStateChanges(Pair<Game.State> state)
        {
            if (state.Previous == Game.State.Home)
            {
                // Render everything and play music
                SetCameraLayers(false);
                _audioService.PlayMusic(Audio.Music, 0.3333f);
            }
            else if (state.Current == Game.State.Home)
            {
                // Cull game elements and stop music
                SetCameraLayers(true);
                _audioService.StopMusic();
            }

            // Enable card interactions only while playing
            _cardRaycaster.enabled = state.Current == Game.State.Playing;
        }

        private void SetCameraLayers(bool cullGame)
        {
            if (cullGame)
                // Every layer except Interactable
                _camera.cullingMask = ~(1 << _layerInteractable);
            else
                // Everything
                _camera.cullingMask = ~0;
        }
    }
}
