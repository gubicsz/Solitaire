using UniRx;

namespace Solitaire.Services
{
    public interface IPointsService
    {
        IntReactiveProperty Points { get; }

        void Add(int value);
        void Reset();
        void Set(int value);
    }
}
