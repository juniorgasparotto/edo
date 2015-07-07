using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDO
{
    public class Branch<T>
    {
        public BranchEntity<T> End { get; private set; }
        public BranchEntity<T> Root { get; private set; }
        public int Level { get; private set; }

        public string Index { get; private set; }
        public string IndexCoexistent { get; private set; }
        public IEnumerable<BranchEntity<T>> Traverse { get; private set; }
        public Branch<T> Parent { get; private set; }
        public int CountChildren { get; set; }

        public Branch(BranchEntity<T> root, BranchEntity<T> end, Branch<T> parent, int level, string index, string indexCoexistent, IEnumerable<BranchEntity<T>> entities)
        {
            this.Level = level;
            this.Root = root;
            this.End = end;
            this.Parent = parent;
            this.Index = index;
            this.IndexCoexistent = indexCoexistent;
            this.Traverse = entities;
        }

        public string Debug()
        {
            var str = this.IndexCoexistent;
            var strEntities = "";
            foreach (var i in this.Traverse)
            {
                if (strEntities == "")
                    strEntities = i.Entity.ToString();
                else
                    strEntities += " > " + i.Entity.ToString();
            }

            return str + " (" + strEntities + ")";
        }

        public override string ToString()
        {
            return this.Index;
        }
    }
}

