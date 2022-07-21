using DG.Tweening;
using Solitaire.Commands;
using Solitaire.Models;
using Solitaire.Presenters;
using Solitaire.Services;
using UnityEngine;
using Zenject;

namespace Solitaire.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] GameObject _cardPrefab;

        private void Awake()
        {
            // Initialize
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            DOTween.Init();
        }

        public override void InstallBindings()
        {
            InstallServices();
            InstallGame();
            InstallPiles();
            InstallCards();
            InstallCommands();
        }

        void InstallServices()
        {
            Container.BindInterfacesAndSelfTo<CommandService>().AsSingle();
            Container.BindInterfacesAndSelfTo<DragAndDropHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<MovesService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PointsService>().AsSingle();
            Container.BindInterfacesAndSelfTo<HintService>().AsSingle();
            Container.BindInterfacesAndSelfTo<OrientationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AdService>().AsSingle();
            Container.BindInterfacesAndSelfTo<StorageService>().AsSingle();
        }

        void InstallGame()
        {
            Container.Bind<Game>().AsSingle();
            Container.Bind<GameState>().AsSingle();
            Container.Bind<GamePopup>().AsSingle();
            Container.Bind<GameControls>().AsSingle();
            Container.Bind<OrientationState>().AsSingle();
            Container.Bind<Options>().AsSingle();
            Container.Bind<Leaderboard>().AsSingle();
        }

        void InstallPiles()
        {
            Container.Bind<Pile>().AsTransient();
        }

        void InstallCards()
        {
            Container.Bind<Card>().AsTransient();
            Container.BindInterfacesAndSelfTo<CardSpawner>().AsSingle();
            Container.BindFactory<Card.Suits, Card.Types, CardPresenter, CardPresenter.Factory>()
                .FromMonoPoolableMemoryPool(x => x.WithInitialSize(52)
                .FromComponentInNewPrefab(_cardPrefab)
                .UnderTransformGroup("CardPool"));
        }

        void InstallCommands()
        {
            Container.BindFactory<Pile, Pile, DrawCardCommand, DrawCardCommand.Factory>()
                .FromPoolableMemoryPool(x => x.WithInitialSize(256).ExpandByDoubling());
            Container.BindFactory<Card, Pile, Pile, MoveCardCommand, MoveCardCommand.Factory>()
                .FromPoolableMemoryPool(x => x.WithInitialSize(256).ExpandByDoubling());
            Container.BindFactory<Pile, Pile, RefillStockCommand, RefillStockCommand.Factory>()
                .FromPoolableMemoryPool(x => x.WithInitialSize(16).ExpandByDoubling());
        }
    }
}
