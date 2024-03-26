using System.Collections.Generic;
using Solitaire.Models;

namespace Solitaire.Services
{
    public interface IHintService
    {
        Pile CheckPilesForMove(IList<Pile> piles, Card card);
        Pile FindValidMove(Card card);
        Hint GetHint();
    }
}
