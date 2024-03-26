using Solitaire.Models;
using UniRx;
using UnityEngine;
using Zenject;

namespace Solitaire.Presenters
{
    public abstract class OrientationAwarePresenter : MonoBehaviour
    {
        [Inject]
        protected readonly OrientationState _orientation;

        protected virtual void Start()
        {
            // Notify on orientation change
            _orientation
                .State.Subscribe(orientation =>
                    OnOrientationChanged(orientation == Orientation.Landscape)
                )
                .AddTo(this);
        }

        protected abstract void OnOrientationChanged(bool isLandscape);
    }
}
