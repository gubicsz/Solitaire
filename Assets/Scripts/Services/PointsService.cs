using UniRx;
using UnityEngine;

namespace Solitaire.Services
{
    public class PointsService : IPointsService
    {
        public IntReactiveProperty Points { get; private set; } = new IntReactiveProperty();

        public void Set(int value)
        {
            Points.Value = value;
        }

        public void Add(int value)
        {
            Set(Mathf.Max(Points.Value + value, 0));
        }

        public void Reset()
        {
            Set(0);
        }
    }
}
