namespace Solitaire.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
