using UniRx;

namespace Solitaire.Services
{
    public class MovesService : IMovesService
    {
        public IntReactiveProperty Moves { get; } = new();

        public void Increment()
        {
            Moves.Value += 1;
        }

        public void Reset()
        {
            Moves.Value = 0;
        }
    }
}
