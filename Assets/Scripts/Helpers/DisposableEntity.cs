using System;
using UniRx;

namespace Solitaire.Helpers
{
    public class DisposableEntity : IDisposable
	{
		private readonly CompositeDisposable _disposables = new CompositeDisposable();

		~DisposableEntity()
		{
			Dispose();
		}

		public void Dispose()
		{
			_disposables.Dispose();
		}

		public void AddDisposable(IDisposable item)
		{
			_disposables.Add(item);
		}
	}

	public static class DisposableExtensions
	{
		public static T AddTo<T>(this T disposable, DisposableEntity entity)
			where T : IDisposable
		{
			entity.AddDisposable(disposable);
			return disposable;
		}
	}
}
