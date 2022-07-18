using Solitaire.Models;
using Solitaire.Services;

namespace Solitaire.Commands
{
    public class DrawCardCommand : ICommand
    {
        readonly Card _card;
        readonly Pile _pileStock;
        readonly Pile _pileWaste;
        readonly AudioService _audioService;

        public DrawCardCommand(Card card, Pile pileStock, Pile pileWaste, AudioService audioService)
        {
            _card = card;
            _pileStock = pileStock;
            _pileWaste = pileWaste;
            _audioService = audioService;
        }

        public void Execute()
        {
            _card.Flip();
            _pileWaste.AddCard(_card);

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }

        public void Undo()
        {
            _card.Flip();
            _pileStock.AddCard(_card);

            _audioService.PlaySfx(Audio.SfxDraw, 0.5f);
        }
    }
}
