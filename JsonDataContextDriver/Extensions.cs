using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JsonDataContextDriver
{
    public static class Extensions
    {
        public static string ReplaceAll(this string s, IEnumerable<Tuple<string, string>> replacements)
        {
            foreach (var repl in replacements)
                s = s.Replace(repl.Item1, repl.Item2);
		
            return s;
        }

        public static IEnumerable<T> DoEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
                yield return item;
            }
        }

    }
}