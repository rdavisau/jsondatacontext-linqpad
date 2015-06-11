using System;
using System.Collections.Generic;
using System.Linq;
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

        public static string SanitiseClassName(this string originalName)
        {
            var replacers = new[]
                               {
                                    "\n", "'", " ", "*", "/", "-", "(", ")", ".", "!", "?", "#", ":", "+", "{", "}", "&",
                                    ","
                                };
            var tuples = replacers.Select(r => Tuple.Create(r, "_")).ToList();

            var newName = originalName.ReplaceAll(tuples);
                if (char.IsNumber(newName[0]))
                    newName = "_" + newName;

                return newName;
        }
    }


}