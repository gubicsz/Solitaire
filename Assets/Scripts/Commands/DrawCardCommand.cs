using Solitaire.Models;

namespace Solitaire.Commands
{
    public class DrawCardCommand : ICommand
    {
        readonly Pile _pileStock;
        readonly Pile _pileWaste;
        readonly Card _card;

        public DrawCardCommand(Pile pileStock, Pile pileWaste, Card card)
        {
            _pileStock = pileStock;
            _pileWaste = pileWaste;
            _card = card;
        }

        public void Execute()
        {
            _card.Flip();
            _pileWaste.AddCard(_card);
        }

        public void Undo()
        {
            _card.Flip();
            _pileStock.AddCard(_card);
        }
    }
}
