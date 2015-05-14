using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO
{
    public class UniqueEntityList : List<HierarchicalEntity>
    {
        private bool ValidateAdd(HierarchicalEntity obj)
        {
            var exists = this.Exists(f => f == obj);
            if (!exists)
            {
                if (this.Exists(f => f.Name == obj.Name))
                    throw new EntityAlreadyExistsException(string.Format("Object '{0}' already exists in collection", obj.Name));
            }

            return !exists;
        }

        public new void Add(HierarchicalEntity obj)
        {
            if (this.ValidateAdd(obj))
                base.Add(obj);
        }

        public new void AddRange(IEnumerable<HierarchicalEntity> list)
        {
            foreach (var item in list)
                this.Add(item);
        }

        public new void Insert(int index, HierarchicalEntity item)
        {
            if (this.ValidateAdd(item))
                this.Insert(index, item);
        }

        public new void InsertRange(int index, IEnumerable<HierarchicalEntity> list)
        {
            foreach (var item in list)
                this.Insert(index, item);
        }

        public new HierarchicalEntity this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                this.Add(value);
            }
        }

        public new void CopyTo(HierarchicalEntity[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length; i++)
                this.Add(array[i]);
        }
    }
}
