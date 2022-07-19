using System;
using UniRx;

namespace Solitaire.Helpers
{
    public abstract class StateModel<T> : DisposableEntity where T : Enum
    {
        public ReactiveProperty<T> State { get; private set; }

        public StateModel(T state)
        {
            State = new ReactiveProperty<T>(state);
        }
    }
}
