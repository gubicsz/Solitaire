using Solitaire.Commands;
using UniRx;

namespace Solitaire.Services
{
    public interface ICommandService
    {
        BoolReactiveProperty CanUndo { get; }

        void Add(ICommand command);
        void Reset();
        void Undo();
    }
}
