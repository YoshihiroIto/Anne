using System;
using System.Collections.Generic;

namespace Anne.Foundation.Extentions
{
    public static class ListExtensions
    {
        // http://stackoverflow.com/questions/594518/is-there-a-lower-bound-function-on-a-sortedlistk-v   をもとに改変
        // list:ソート済みのもの
        public static int MakeInsertIndex<T>(this IList<T> list, T value, Func<T, T, int> comp)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (list.Count == 0)
                return 0;
            
            int lo = 0, hi = list.Count - 1;

            while (lo < hi)
            {
                var m = (hi + lo) / 2;

                if (comp(list[m], value) < 0)
                    lo = m + 1;
                else
                    hi = m - 1;
            }

            if (comp(list[lo], value) < 0)
                lo++;

            return lo;
        }

         
    }
}