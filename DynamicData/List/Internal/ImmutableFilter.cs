using System;
using System.Reactive.Linq;
using DynamicData.Annotations;

namespace DynamicData.Internal
{
	internal class ImmutableFilter<T>
	{
		private readonly IObservable<IChangeSet<T>> _source;
		private readonly Func<T, bool> _predicate;
		private readonly ChangeAwareList<T> _filtered = new ChangeAwareList<T>();


		public ImmutableFilter([NotNull] IObservable<IChangeSet<T>> source, [NotNull]Func<T, bool> predicate)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));
			_source = source;
			_predicate = predicate;
		}

		public IObservable<IChangeSet<T>> Run()
		{
		    return _source.Select(changes =>
		    {
		        _filtered.Filter(changes, _predicate);
		        return _filtered.CaptureChanges();
		    })
		    .NotEmpty();
		}
	}


}