using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDO
{
    public static class RecursiveExtensions
    {
        // The Traverse extension can be added to types of IEnumerable.
        // Returns T
        // Input Param
        // "fnRecurse" - delegate with one parameter T and returns IEnumerable
        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> fnRecurse)
        {
            return Traverse(source, fnRecurse, new List<object>());
        }

        private static IEnumerable<T> Traverse<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> fnRecurse, List<object> iterateds)
        {
            foreach (T item in source)
            {
                if (!iterateds.Contains(item))
                { 
                    iterateds.Add(item);

                    yield return item;

                    var seqRecurse = fnRecurse(item);

                    if (seqRecurse != null)
                    {

                        //Making Recursive call to Traverse using 
                        //results from the lambda expression
                        foreach (T itemRecurse in Traverse(seqRecurse, fnRecurse, iterateds))
                        {
                            yield return itemRecurse;
                        }
                    }
                }
            }
        }
    }
}
