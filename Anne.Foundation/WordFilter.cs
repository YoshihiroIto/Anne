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
    }
}