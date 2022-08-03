using Solitaire.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire.Services
{
    public class DragAndDropHandler : IDragAndDropHandler
    {
        readonly Camera _camera;
        IList<Card> _draggedCards;

        const int DragOrder = 100;

        public DragAndDropHandler()
        {
            _camera = Camera.main;
            _draggedCards = new List<Card>();
        }

        public void BeginDrag(PointerEventData eventData, IList<Card> draggedCards)
        {
            Vector3 dragPos = PointerToWorldPoint(eventData);
            _draggedCards = draggedCards;

            for (int i = 0; i < _draggedCards.Count; i++)
            {
                Card card = _draggedCards[i];
                card.DragOrigin = card.Position.Value;
                card.DragOffset = card.DragOrigin - dragPos;
                card.OrderToRestore = card.Pile.Cards.IndexOf(card);
                card.Order.Value = DragOrder + i;
                card.IsDragged = true;
            }
        }

        public void Drag(PointerEventData eventData)
        {
            Vector3 dragPos = PointerToWorldPoint(eventData);

            for (int i = 0; i < _draggedCards.Count; i++)
            {
                Card card = _draggedCards[i];
                card.Position.Value = dragPos + card.DragOffset;
            }
        }

        public void Drop()
        {
            for (int i = 0; i < _draggedCards.Count; i++)
            {
                Card card = _draggedCards[i];
                card.IsDragged = false;
            }
        }

        public void EndDrag()
        {
            for (int i = 0; i < _draggedCards.Count; i++)
            {
                Card card = _draggedCards[i];

                if (!card.IsDragged)
                {
                    continue;
                }

                card.IsDragged = false;
                card.Order.Value = card.OrderToRestore;
                card.Position.Value = card.DragOrigin;
            }
        }

        private Vector3 PointerToWorldPoint(PointerEventData eventData)
        {
            Vector3 screenPoint = new Vector3(eventData.position.x, eventData.position.y, -_camera.transform.position.z);
            return _camera.ScreenToWorldPoint(screenPoint);
        }
    }
}
