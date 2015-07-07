using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EDO
{
    public class Path<T>
    {
        public Path<T> Before { get; set; }
        public Path<T> After { get; set; }
        public List<Edge<T>> Edges { get; set; }
    }

    public class BranchFactory<T>
    {
        private Dictionary<int, SlotData> currentDataInLevel = new Dictionary<int, SlotData>();
        private Dictionary<T, BranchEntity<T>> entities = new Dictionary<T, BranchEntity<T>>();
        private int lastId = 1;

        public Dictionary<BranchEntity<T>, List<BranchEntity<T>>> roots = new Dictionary<BranchEntity<T>, List<BranchEntity<T>>>();
        public Dictionary<int, Dictionary<T, int>> indexes = new Dictionary<int, Dictionary<T, int>>();

        private class SlotData
        {
            public int Level { get; set; }
            public BranchEntity<T> BranchEntity { get; set; }

            public SlotData(BranchEntity<T> branchEntity, int level)
            {
                this.BranchEntity = branchEntity;
                this.Level = level;
            }

            public override string ToString()
            {
                return this.BranchEntity.ToString();
            }
        }

        public void NextRoot()
        {
            if (indexes.Count > 1)
            {
                var first = indexes.FirstOrDefault();
                indexes = new Dictionary<int, Dictionary<T, int>>();
                indexes.Add(first.Key, first.Value);
            }

            if (currentDataInLevel.Count > 1)
            {
                var first = currentDataInLevel.FirstOrDefault();
                currentDataInLevel = new Dictionary<int, SlotData>();
                currentDataInLevel.Add(first.Key, new SlotData(null, first.Value.Level));
            }
        }

        public Branch<T> Create(T entity, Branch<T> parent, int level)
        {
            BranchEntity<T> branchEntity = null;

            if (!entities.ContainsKey(entity))
            {
                branchEntity = new BranchEntity<T>(entity);
                entities.Add(entity, branchEntity);
                branchEntity.InternalId = lastId++;
            }
            else
            {
                branchEntity = entities[entity];
            }
            
            if (!currentDataInLevel.ContainsKey(level))
            {
                currentDataInLevel.Add(level, new SlotData(branchEntity, 1));
            }
            else
            { 
                currentDataInLevel[level].Level++;
                currentDataInLevel[level].BranchEntity = entities[entity];
            }

            if (!indexes.ContainsKey(level))
                indexes.Add(level, new Dictionary<T, int>());

            if (!indexes[level].ContainsKey(entity))
            {
                int newLevel = 1;
                if (indexes.ContainsKey(level - 1)) 
                {
                    var last = indexes[level - 1].LastOrDefault();
                    newLevel = (last.Key == null ? 1 : last.Value + 1);
                }
                indexes[level].Add(entity, newLevel);
            }
            else
                indexes[level][entity]++;

            var index = "";
            var indexCoexistent = "";
            var traverse = new List<BranchEntity<T>>();
            BranchEntity<T> root = null;

            foreach(var slot in currentDataInLevel.Where(f=>f.Key <= level))
            {
                if (index != "")
                {
                    index += "." + indexes[slot.Key].Count;
                    indexCoexistent += "." + "[" + slot.Value.BranchEntity.InternalId + "]";
                }
                else
                {
                    index = indexes[slot.Key].Count.ToString();
                    indexCoexistent = "[" + slot.Value.BranchEntity.InternalId + "]";
                }
               
                if (slot.Key == 1)
                    root = slot.Value.BranchEntity;

                traverse.Add(slot.Value.BranchEntity);
            }

            // Add new information for branch entity
            if (level == 1)
            {
                if (!roots.ContainsKey(root))
                    roots.Add(root, new List<BranchEntity<T>>());

                //branchEntity.Levels.Add(level);
                //branchEntity.NumberOfReferences++;
            }

            roots[root].Add(branchEntity);

            //roots[root].Add(branchEntity);
            branchEntity.Roots = roots;
            return new Branch<T>(root, branchEntity, parent, level, index, indexCoexistent, traverse);
        }
    }
}

