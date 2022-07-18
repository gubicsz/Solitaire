using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Installers
{
    [CreateAssetMenu(fileName = "GameConfigInstaller", menuName = "Installers/GameConfigInstaller")]
    public class GameConfigInstaller : ScriptableObjectInstaller<GameConfigInstaller>
    {
        [SerializeField] Game.Config _game;
        [SerializeField] Card.Config _card;
        [SerializeField] Audio.Config _audio;

        public override void InstallBindings()
        {
            // Configs
            Container.BindInstances(_game);
            Container.BindInstances(_card);
            Container.BindInstances(_audio);
        }
    }
}