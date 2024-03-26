using UniRx;

namespace Solitaire.Services
{
    public interface IMovesService
    {
        IntReactiveProperty Moves { get; }

        void Increment();
        void Reset();
    }
}
