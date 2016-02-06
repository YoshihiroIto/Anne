using System.Collections.Generic;

namespace Anne.Foundation.Extentions
{
    public static class DictionaryExtensions
    {
        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue notFoundValue = default (TValue))
        {
            TValue value;

            return source.TryGetValue(key, out value) ? value : notFoundValue;
        }
    }
}