using Solitaire.Models;
using UnityEngine;
using Zenject;

namespace Solitaire.Services
{
    public class OrientationService : ITickable, IOrientationService
    {
        private readonly OrientationState _orientation;

        public OrientationService(OrientationState orientation)
        {
            _orientation = orientation;
        }

        public void Tick()
        {
#if UNITY_EDITOR
            var orientation =
                Screen.width > Screen.height ? Orientation.Landscape : Orientation.Portrait;
#else
            Orientation orientation =
                Screen.orientation == ScreenOrientation.LandscapeLeft
                || Screen.orientation == ScreenOrientation.LandscapeRight
                    ? Orientation.Landscape
                    : Orientation.Portrait;
#endif

            if (_orientation.State.Value != orientation)
                _orientation.State.Value = orientation;
        }
    }
}
