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

        public override void InstallBindings()
        {
            // Services
            Container.BindInterfacesAndSelfTo<CommandService>().AsSingle();
            Container.BindInterfacesAndSelfTo<DragAndDropHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<MovesService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PointsService>().AsSingle();
            Container.BindInterfacesAndSelfTo<OrientationService>().AsSingle();

            // Game
            Container.Bind<Game>().AsSingle();
            Container.Bind<GameState>().AsSingle();
            Container.Bind<GameControls>().AsSingle();
            Container.Bind<OrientationState>().AsSingle();

            // Piles
            Container.Bind<Pile>().AsTransient();

            // Cards
            Container.Bind<Card>().AsTransient();
            Container.BindInterfacesAndSelfTo<CardSpawner>().AsSingle();
            Container.BindFactory<Card.Suits, Card.Types, CardPresenter, CardPresenter.Factory>().FromMonoPoolableMemoryPool(
                x => x.WithInitialSize(52).FromComponentInNewPrefab(_cardPrefab).UnderTransformGroup("CardPool"));
        }
    }
}
