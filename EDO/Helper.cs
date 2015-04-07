using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDO
{
    public static class Helper
    {
        public static string TrimAll(string str)
        {
            return str.Trim().TrimStart('\r', '\n').TrimEnd('\r', '\n');
        }

        public static Dictionary<TKey, TValue> InvertDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var reverse = new Dictionary<TKey, TValue>();

            for (var i = dictionary.Count - 1; i >= 0; i--)
                reverse.Add(dictionary.Keys.ElementAt(i), dictionary.Values.ElementAt(i));

            return reverse;
        }
    }
}
