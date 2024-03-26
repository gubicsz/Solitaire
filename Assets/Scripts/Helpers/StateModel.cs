using System;
using UniRx;

namespace Solitaire.Helpers
{
    public abstract class StateModel<T> : DisposableEntity
        where T : Enum
    {
        public StateModel(T state)
        {
            State = new ReactiveProperty<T>(state);
        }

        public ReactiveProperty<T> State { get; private set; }
    }
}
