using System;
using TMPro;

namespace UniRx
{
    public static class UniRxExtensions
    {
        public static IDisposable SubscribeToText(this IObservable<string> source, TextMeshProUGUI text)
        {
            return source.SubscribeWithState(text, (x, t) => t.text = x);
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TextMeshProUGUI text)
        {
            return source.SubscribeWithState(text, (x, t) => t.text = x.ToString());
        }

        public static IDisposable SubscribeToText<T>(this IObservable<T> source, TextMeshProUGUI text, Func<T, string> selector)
        {
            return source.SubscribeWithState(text, (x, t) => t.text = selector(x));
        }
    }
}
