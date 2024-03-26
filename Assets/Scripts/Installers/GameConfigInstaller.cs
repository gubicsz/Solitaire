using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Installers
{
    [CreateAssetMenu(fileName = "GameConfigInstaller", menuName = "Installers/GameConfigInstaller")]
    public class GameConfigInstaller : ScriptableObjectInstaller<GameConfigInstaller>
    {
        [SerializeField]
        private Game.Config _game;

        [SerializeField]
        private Card.Config _card;

        [SerializeField]
        private Audio.Config _audio;

        public override void InstallBindings()
        {
            // Configs
            Container.BindInstances(_game);
            Container.BindInstances(_card);
            Container.BindInstances(_audio);
        }
    }
}
