using KeepCoding;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Logging
{
    internal class Logs
    {
        internal const ushort LineLength = 27, RowLength = 16;

        internal int Count { get { return _logs.Count; } }
        internal string[] Keys { get { return _logs.Keys.ToArray(); } }
        internal List<string>[] Values { get { return _logs.Values.ToArray(); } }

        private readonly SortedDictionary<string, List<string>> _logs = new SortedDictionary<string, List<string>>();

        internal string Add(string condition)
        {
            var formatted = InsertNewlines(condition);

            if (!formatted.HasValue)
                return null;

            string key = formatted.Value.Key;
            
            var value = formatted.Value.Value;

            if (_logs.ContainsKey(key))
            {
                _logs[key] = _logs[key].Concat(value).ToList();
                return null;
            }

            _logs.Add(key, value);
            return key;
        }

        private KeyValuePair<string, List<string>>? InsertNewlines(string condition)
        {
            condition = condition.Replace('\n', ' ');

            var pair = Seperate(condition.Replace("ModBombComponent #", ""));

            if (!pair.HasValue)
                return null;

            var format = new StringBuilder(pair.Value.Value);

            int lowerBound = 0;

            for (int i = LineLength; i < pair.Value.Value.Length; i--)
            {
                if (lowerBound >= i)
                {
                    lowerBound += LineLength;
                    format.Insert(lowerBound, "\n");
                    i = lowerBound + LineLength;
                }

                else if (char.IsWhiteSpace(pair.Value.Value[i]))
                {
                    format[i] = '\n';

                    lowerBound = i + 1;
                    i += LineLength;
                }
            }

            return new KeyValuePair<string, List<string>>(pair.Value.Key, format.ToString().Split('\n').Append("").ToList());
        }

        private KeyValuePair<string, string>? Seperate(string condition)
        {
            var regex = new Regex(@"\[(.+#\d+)\]:* ");
            var match = regex.Match(condition);

            if (!match.Success)
                return null;

            string key = match.Value.Skip(match.Value.StartsWith("[The ") ? 5 : 1).TakeWhile(c => c != ']').Join(""),
                value = condition.Replace(match.Value, "");

            return new KeyValuePair<string, string>(key, value);
        }
    }
}