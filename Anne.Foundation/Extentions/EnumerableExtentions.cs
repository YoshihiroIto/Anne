using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Anne.Foundation.Extentions
{
    public static class EnumerableExtentions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return new ObservableCollection<T>(source);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, bool> onNext)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (onNext == null) throw new ArgumentNullException(nameof(onNext));

            var isFirst = true;

            foreach (var s in source)
            {
                onNext(s, isFirst);
                isFirst = false;
            }
        }
    }
}