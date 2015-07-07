using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDO
{
    public static class BranchExtensions
    {
        public static IEnumerable<Branch<T>> RemoveCoexistent<T>(this IEnumerable<Branch<T>> source)
        {
            var listRet = source.ToList();
            foreach (var item in source)
            {
                var found = listRet.Where(f => f.IndexCoexistent.Contains(item.IndexCoexistent)).ToList();
                if (found.Count > 1)
                    listRet.Remove(item);
            }
            return (IEnumerable<Branch<T>>)listRet;
        }

        //public class Group<T>
        //{
        //    public List<List<T>> Keys { get; set; }
        //    public List<T> Values { get; set; }
        //    public void Add()
        //}

        public static Dictionary<List<T>, List<T>> Group1<T>(this IEnumerable<Branch<T>> source)
        {
            var list = new Dictionary<List<T>, List<T>>();
            var listRet = source.ToList();
            foreach (var item in source)
            {
                foreach (var el in item.Traverse)
                { 
                    var allFirst = source.Where(f => f.Traverse.Contains(el)).Select(f=>f.Root).Distinct().ToList();
                    //if (!list.ContainsKey(allFirst))
                    //    list.Add(allFirst, new List<T>());

                    //list[allFirst].Add(el);
                }
            }

            return list;
        }

        public static IEnumerable<BranchEntity<T>> Roots<T>(this IEnumerable<Branch<T>> source)
        {
            return source.Select(f=> f.Root).Distinct();
        }

        public static IEnumerable<BranchEntity<T>> Ends<T>(this IEnumerable<Branch<T>> source)
        {
            return source.Select(f => f.End).Distinct();
        }

        public static IEnumerable<Branch<T>> ToPaths<T>(this IEnumerable<T> startLevel, Func<T, IEnumerable<T>> nextLevels = null)
        {
            var factory = new BranchFactory<T>();
            return ToPaths(startLevel, nextLevels, null, 1, factory, null);
        }

        public static IEnumerable<Branch<T>> ToPaths<T>(this IEnumerable<T> startLevel, Func<T, IEnumerable<T>> nextLevels, Branch<T> parent, int level, BranchFactory<T> factory, List<T> iterateds)
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
                    
                    if (level == 1)
                        factory.NextRoot();

                    // Main return
                    var ret = factory.Create(current, parent, level);
                    yield return ret;

                    // Return all descendants if exists
                    if (nexts != null)
                        foreach (var descendant in ToPaths(nexts, nextLevels, ret, level + 1, factory, iterateds))
                            yield return descendant;
                }
                else
                {
                    if (level == 1)
                        factory.NextRoot();

                    yield return factory.Create(current, parent, level);
                }
            }
        }
    }
}
