using Solitaire.Models;
using Solitaire.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Solitaire.Helpers
{
    [RequireComponent(typeof(Selectable))]
    public class InteractableSfx : MonoBehaviour, IPointerUpHandler
    {
        [Inject]
        private readonly IAudioService _audioService;

        private Selectable _selectable;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_selectable.IsInteractable())
                _audioService.PlaySfx(Audio.SfxClick, 1.0f);
        }
    }
}
