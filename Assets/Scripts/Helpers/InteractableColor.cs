using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Helpers
{
    [RequireComponent(typeof(Selectable))]
    public class InteractableColor : MonoBehaviour
    {
        [SerializeField]
        private Image _image;
        private bool _isNormal = true;

        private Selectable _selectable;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        private void Update()
        {
            var isInteractable = _selectable.IsInteractable();

            if (isInteractable && !_isNormal)
            {
                ChangeAlpha(1f);
                _isNormal = true;
            }
            else if (!isInteractable && _isNormal)
            {
                ChangeAlpha(0.5f);
                _isNormal = false;
            }
        }

        private void ChangeAlpha(float alpha)
        {
            var color = _image.color;
            color.a = alpha;
            _image.color = color;
        }
    }
}
