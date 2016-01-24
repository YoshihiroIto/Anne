using System.Linq;
using System.Text.RegularExpressions;

namespace Anne.Foundation
{
    public class WordFilter
    {
        private readonly Regex[] _regexes;

        public WordFilter()
            : this(string.Empty)
        {
        }

        public WordFilter(string src)
        {
            _regexes = src
                .Split(' ')
                .Where(x => x.Length >= 3)
                .Select(x => MigemoHelper.Instance.MakeRegex(x))
                .ToArray();
        }

        public bool IsMatch(string s)
        {
            return _regexes.All(r => r.IsMatch(s));
        }

        public override bool Equals(object obj)
        {
            var w = obj as WordFilter;

            return w != null && _regexes.Select(x => x.ToString()).SequenceEqual(w._regexes.Select(x => x.ToString()));
        }

        public bool Equals(WordFilter w)
        {
            return w != null && _regexes.Select(x => x.ToString()).SequenceEqual(w._regexes.Select(x => x.ToString()));
        }

        public override int GetHashCode()
        {
            if (_regexes.Length == 0)
                return _regexes.GetHashCode();

            return _regexes.Aggregate(0, (current, r) => current ^ r.ToString().GetHashCode());
        }
    }
}