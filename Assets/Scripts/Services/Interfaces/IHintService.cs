using Solitaire.Models;
using System.Collections.Generic;

namespace Solitaire.Services
{
    public interface IHintService
    {
        Pile CheckPilesForMove(IList<Pile> piles, Card card);
        Pile FindValidMove(Card card);
        Hint GetHint();
    }
}