﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Annotations;
using DynamicData.Kernel;

namespace DynamicData.Internal
{
	internal sealed class Sort<T>
	{
		private readonly IObservable<IChangeSet<T>> _source;
		private readonly IComparer<T> _comparer;
		private readonly SortOptions _sortOptions;
		private readonly ChangeAwareList<T> _innerList = new ChangeAwareList<T>();

		public Sort([NotNull] IObservable<IChangeSet<T>> source, [NotNull] IComparer<T> comparer, SortOptions sortOptions)
		{
		    if (source == null) throw new ArgumentNullException(nameof(source));
		    if (comparer == null) throw new ArgumentNullException(nameof(comparer));
		    _source = source;
			_comparer = comparer;
			_sortOptions = sortOptions;
		}

		public IObservable<IChangeSet<T>> Run()
		{
			return _source.Select(Process);
		}

		private IChangeSet<T> Process(IChangeSet<T> changes)
		{
            //TODO: Can this be optimised? Perhaps option of add to end then do inline sort

            changes.ForEach(change =>
			{

				switch (change.Reason)
				{
					case ListChangeReason.Add:
					{
						var current = change.Item.Current;
						Insert(current);
						break;
					}
					case ListChangeReason.AddRange:
					{
						var ordered = change.Range.OrderBy(t => t, _comparer).ToList();
						if (_innerList.Count == 0)
						{
							_innerList.AddRange(ordered);
						}
						else
						{
							change.Range.ForEach(Insert);
						}
						break;
					}
					case ListChangeReason.Update:
					{
						var current = change.Item.Current;
						//TODO: check whether an item should stay in the same position
						//i.e. update and move
						Remove(change.Item.Previous.Value);
						Insert(current);
						break;
					}
					case ListChangeReason.Remove:
					{
						var current = change.Item.Current;
						Remove(current);
						break;
					}
					case ListChangeReason.RemoveRange:
						{
							change.Range.ForEach(Remove);
							break;
						}
					case ListChangeReason.Clear:
					{
						_innerList.Clear();
                        break;
					}

				}
			});

			return _innerList.CaptureChanges();
		}



		private void Remove(T item)
		{
			var index = GetCurrentPosition(item);
			_innerList.RemoveAt(index);

		}

		private void Insert(T item)
		{
			var index = GetInsertPosition(item);
			_innerList.Insert(index,item);

		}

		private int GetInsertPosition(T item)
		{
			return _sortOptions == SortOptions.UseBinarySearch
				? GetInsertPositionBinary(item)
				: GetInsertPositionLinear(item);
		}

		private int GetInsertPositionLinear(T item)
		{
			for (var i = 0; i < _innerList.Count; i++)
			{
				if (_comparer.Compare(item, _innerList[i]) < 0)
					return i;
			}
			return _innerList.Count;
		}

		private int GetInsertPositionBinary(T item)
		{
			int index = _innerList.BinarySearch(item, _comparer);
			int insertIndex = ~index;

			//sort is not returning uniqueness
			if (insertIndex < 0)
				throw new SortException("Binary search has been specified, yet the sort does not yeild uniqueness");
			return insertIndex;
		}

		private int GetCurrentPosition(T item)
		{
			int index = _sortOptions == SortOptions.UseBinarySearch
				? _innerList.BinarySearch(item,_comparer)
				: _innerList.IndexOf(item);

			if (index < 0)
				throw new SortException("Current item cannot be found");

			return index;
		}
	}
}