using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDO
{
    public class BranchEntity<T>
    {
        public T Entity { get; internal set; }
        public int InternalId { get; internal set; }
        public int NumberOfReferences { get; internal set; }
        public Dictionary<BranchEntity<T>, List<BranchEntity<T>>> Roots = new Dictionary<BranchEntity<T>, List<BranchEntity<T>>>();
        public List<int> Levels { get; internal set; }

        public BranchEntity(T entity)
        {
            this.Entity = entity;
            //this.Roots = new List<BranchEntity<T>>();
            this.Levels = new List<int>();
        }

        public IEnumerable<BranchEntity<T>> Get()
        {
            var listRet = Roots.ToList();
            foreach (var item in Roots)
            {
                var found = listRet.Where(f => f.Value.Contains(item.Key)).ToList();
                if (found.Count > 1)
                {
                    var remove = true;
                    foreach(var test in found)
                    {
                        if (item.Value.Contains(test.Key))
                        {
                            remove = false;
                            break;
                        }
                    }
                    if (remove)
                        listRet.Remove(item);
                }
            }

            foreach (var item in listRet)
                yield return item.Key;
        }

        public override string ToString()
        {
            return this.Entity.ToString();
        }
    }
}

