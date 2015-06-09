using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDO
{
    public static class BranchExtensions
    {
        public static IEnumerable<Branch<T>> Roots<T>(this IEnumerable<Branch<T>> source)
        {
            // Never exists a item with EntityLevel > 1 or < 1
            return source.Where(f=>f.EntityLevel == 1);
        }

        public static IEnumerable<Branch<T>> SubBranchesAndSelf<T>(this IEnumerable<Branch<T>> source, Branch<T> from)
        {
            // With delegate, mantain the enumerator! lazy!
            Func<Branch<T>, Branch<T>, bool> getCurrent = (test,cur) =>
            {
                if (test.Parent == cur)
                {
                    from = test;
                    return true;
                }
                else 
                {
                    return false;
                }
            };

            yield return from;
            foreach (var item in source.Where(f => getCurrent(f, from)))
                yield return item;
        }

        public static IEnumerable<Branch<T>> ToBranches<T>(this IEnumerable<T> startLevel, Func<T, IEnumerable<T>> nextLevels = null)
        {
            return ToBranches(startLevel, nextLevels, null, 1, new BranchIndex<T>(), null);
        }

        public static IEnumerable<Branch<T>> ToBranches<T>(this IEnumerable<T> startLevel, Func<T, IEnumerable<T>> nextLevels, Branch<T> parentIndex, int level, BranchIndex<T> indexer, List<T> iterateds)
        {
            foreach (T current in startLevel)
            {
                if (level == 1)
                    iterateds = new List<T>();

                if (!iterateds.Contains(current))
                {
                    iterateds.Add(current);

                    IEnumerable<T> nexts;
                    if (nextLevels != null)
                        nexts = nextLevels(current);
                    else
                        nexts = (IEnumerable<T>)current;
                    
                    // Main return
                    var ret = new Branch<T>(level, current, parentIndex);

                    if (level == 1)
                        indexer.NextRoot();

                    indexer.NextLevel(ret, level);
                    
                    yield return ret;

                    // Return all descendants if exists
                    if (nexts != null)
                        foreach (var descendant in ToBranches(nexts, nextLevels, ret, level + 1, indexer, iterateds))
                            yield return descendant;
                }
                else
                {
                    var ret = new Branch<T>(level, current, parentIndex);
                    if (level == 1)
                        indexer.NextRoot();

                    indexer.NextLevel(ret, level);

                    yield return ret;
                }
            }
        }
    }
}
