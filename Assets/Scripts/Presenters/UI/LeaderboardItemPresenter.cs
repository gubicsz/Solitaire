using Solitaire.Models;
using TMPro;
using UnityEngine;

namespace Solitaire.Presenters
{
    public class LeaderboardItemPresenter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _label;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void Initialize(Leaderboard.Item item, int index)
        {
            if (item == null)
            {
                SetVisiblity(false);
                return;
            }

            _label.text = $"#{index + 1} <color=#00FF80>{item.Points}</color> - {item.Date}";
            SetVisiblity(true);
        }

        public void UpdatePosition(Vector2 position)
        {
            _rect.anchoredPosition = position;
        }

        public Vector2 GetSize()
        {
            return _rect.sizeDelta;
        }

        private void SetVisiblity(bool isVisible)
        {
            if ((!gameObject.activeSelf && isVisible) || (gameObject.activeSelf && !isVisible))
                gameObject.SetActive(isVisible);
        }
    }
}
