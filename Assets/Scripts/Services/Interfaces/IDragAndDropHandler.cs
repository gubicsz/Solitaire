using System.Collections.Generic;
using Solitaire.Models;
using UnityEngine.EventSystems;

namespace Solitaire.Services
{
    public interface IDragAndDropHandler
    {
        void BeginDrag(PointerEventData eventData, IList<Card> draggedCards);
        void Drag(PointerEventData eventData);
        void Drop();
        void EndDrag();
    }
}
