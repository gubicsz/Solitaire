using Solitaire.Helpers;
using Solitaire.Models;
using Zenject;

namespace Solitaire.Presenters
{
    public class OrientationStatePresenter : StateReactor<Orientation>
    {
        [Inject]
        private readonly OrientationState _orientationState;

        protected override StateModel<Orientation> Model => _orientationState;

        protected override void SetVisibility(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}
