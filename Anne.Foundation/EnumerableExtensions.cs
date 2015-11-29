using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Anne.Foundation
{
    public static class EnumerableExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> self)
        {
            return new ObservableCollection<T>(self);
        }
    }
}