using System;
using TMPro;
using UnityEngine.UI;

namespace UniRx
{
    public static class UniRxExtensions
    {
        public static IDisposable SubscribeToText(
            this IObservable<string> source,
            TextMeshProUGUI text
        )
        {
            return source.SubscribeWithState(text, (x, t) => t.text = x);
        }

        public static IDisposable SubscribeToText<T>(
            this IObservable<T> source,
            TextMeshProUGUI text
        )
        {
            return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
        }

        public static IDisposable SubscribeToText<T>(
            this IObservable<T> source,
            TextMeshProUGUI text,
            Func<T, string> selector
        )
        {
            return source.SubscribeWithState(text, (x, t) => t.text = selector(x));
        }

        public static IDisposable SubscribeToToggle(this IObservable<bool> source, Toggle toggle)
        {
            return source.SubscribeWithState(toggle, (x, t) => t.isOn = x);
        }

        public static IDisposable BindTo(this IReactiveProperty<bool> prop, Toggle toggle)
        {
            var d1 = prop.SubscribeToToggle(toggle);
            var d2 = toggle
                .OnValueChangedAsObservable()
                .SubscribeWithState(prop, (x, p) => p.Value = x);
            return StableCompositeDisposable.Create(d1, d2);
        }
    }
}
