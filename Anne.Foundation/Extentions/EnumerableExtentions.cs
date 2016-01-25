using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Anne.Foundation.Extentions
{
    public static class EnumerableExtentions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> c)
        {
            return new ObservableCollection<T>(c);
        }
    }
}