using UnityEngine;
using UnityEngine.UI;

namespace Solitaire.Helpers
{
    [RequireComponent(typeof(Selectable))]
    public class InteractableColor : MonoBehaviour
    {
        [SerializeField] Image _image;

        Selectable _selectable;
        bool _isNormal = true;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        private void Update()
        {
            bool isInteractable = _selectable.IsInteractable();

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
            Color color = _image.color;
            color.a = alpha;
            _image.color = color;
        }
    }
}
