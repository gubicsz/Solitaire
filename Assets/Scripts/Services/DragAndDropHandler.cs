using Solitaire.Models;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Solitaire.Services
{
    public class DragAndDropHandler
    {
        Camera _camera;
        IList<Card> _draggedCards;

        const int dragOrder = 100;

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
                card.DragOrder = card.Order.Value;
                card.DragOrigin = card.Position.Value;
                card.DragOffset = card.DragOrigin - dragPos;
                card.Order.Value = dragOrder + i;
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
                card.Position.Value = card.DragOrigin;
                card.Order.Value = card.DragOrder;
            }
        }

        private Vector3 PointerToWorldPoint(PointerEventData eventData)
        {
            Vector3 screenPoint = new Vector3(eventData.position.x, eventData.position.y, -_camera.transform.position.z);
            return _camera.ScreenToWorldPoint(screenPoint);
        }
    }
}
