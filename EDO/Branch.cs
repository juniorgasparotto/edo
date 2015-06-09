using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDO
{
    public class BranchIndex<T>
    {
        private Dictionary<int, VariableData> levelsInfo = new Dictionary<int, VariableData>();
        //private Dictionary<T, long> binariesKeys = new Dictionary<T, long>();
        //private int lastBinary = 1;

        private class VariableData
        {
            public int CurrentSubLevel { get; set; }
            public T CurrentEntity { get; set; }

            public VariableData(T currentEntity, int currentLevel)
            {
                this.CurrentEntity = currentEntity;
                this.CurrentSubLevel = currentLevel;
            }
        }

        public void NextRoot()
        {
            if (levelsInfo.Count > 1)
            {
                var first = levelsInfo.FirstOrDefault();
                levelsInfo = new Dictionary<int, VariableData>();
                levelsInfo.Add(first.Key, new VariableData(default(T), first.Value.CurrentSubLevel));
            }
        }

        public void NextLevel(Branch<T> obj, int level)
        {
            //if (!binariesKeys.ContainsKey(obj.Entity))
            //{
            //    var id = lastBinary;
            //    binariesKeys.Add(obj.Entity, id);
            //    lastBinary *= 2;
            //}

            if (!levelsInfo.ContainsKey(level))
            {
                levelsInfo.Add(level, new VariableData(obj.Entity, level));
            }
            else
            { 
                levelsInfo[level].CurrentSubLevel++;
                levelsInfo[level].CurrentEntity = obj.Entity;
            }

            var index = "";
            var list = new List<T>();
            //long binaryPath = 0;

            foreach(var sub in levelsInfo.Where(f=>f.Key <= level))
            {
                //if (sub.Value.CurrentSubLevel != 1)
                //    binaryPath |= binariesKeys[sub.Value.CurrentEntity];

                if (index != "")
                    index += "." + sub.Value.CurrentSubLevel.ToString();
                else
                    index = sub.Value.CurrentSubLevel.ToString();
                list.Add(sub.Value.CurrentEntity);
            }

            //obj.BinaryId = binariesKeys[obj.Entity];
            //obj.BinaryPath = binaryPath;
            obj.Index = index;
            obj.Entities = list;
        }
    }

    public class Branch<T>
    {
        public int EntityLevel { get; private set; }
        public T Entity { get; private set; }
        public Branch<T> Parent { get; private set; }

        //public long BinaryId { get; internal set; }
        //public long BinaryPath { get; internal set; }
        public string Index { get; internal set; }
        public IEnumerable<T> Entities { get; internal set; }

        public Branch(int level, T entity, Branch<T> parent)
        {
            this.EntityLevel = level;
            this.Entity = entity;
            this.Parent = parent;
        }

        //public bool IsDescendantOf(Branch<T> test)
        //{
        //    if ((this.BinaryPath & test.BinaryId) == test.BinaryId)
        //    {
        //        if (this.BinaryPath == test.BinaryPath)
        //            return false;
        //        else if (this.Entity.ToString() == test.Entity.ToString())
        //            return false;
        //        return true;
        //    }
                
        //    return false;
        //}

        public override string ToString()
        {
            var str = this.Index;
            var strEntities = "";
            foreach (var i in this.Entities)
            {
                if (strEntities == "")
                    strEntities = i.ToString();
                else 
                    strEntities += " > " + i.ToString();
            }

            return str + "(" + strEntities + ")";
        }
    }
}

