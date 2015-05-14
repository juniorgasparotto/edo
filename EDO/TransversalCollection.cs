using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO
{
    public class HorizontalCollection : IEnumerable<HierarchicalEntity>
    {
        private UniqueEntityList objects;
        private bool invalidated = false;

        public HorizontalCollection()
        {
            this.objects = new UniqueEntityList();
        }

        public void Refresh()
        {
            this.invalidated = false;
            var copy = new UniqueEntityList();
            var merge = new UniqueEntityList();
            foreach (var item in this.objects)
            {
                try
                {
                    copy.Add(item);
                    merge.AddRange(item.References.Traverse(f => f.References));
                    foreach (var itemMerge in merge)
                        if (!this.objects.Contains(itemMerge))
                            copy.Add(itemMerge);
                }
                catch(EntityAlreadyExistsException ex)
                {
                    invalidated = true;
                    throw ex;
                }
            }

            this.objects.Clear();
            this.objects = copy;
        }

        public void Add(HierarchicalEntity obj)
        {
            this.IsInvalidated();
            this.objects.Add(obj);
            this.objects.AddRange(obj.References.Traverse(f => f.References));
        }

        //public IEnumerable<HierarchicalEntity> GetAllDirectOrIndirectReferences(HierarchicalEntity obj)
        //{
        //    this.IsInvalidated();
        //    foreach (var item in this.objects)
        //        yield return item.FindHierarchically(obj.Name);
        //}

        public bool Remove(HierarchicalEntity obj)
        {
            this.IsInvalidated();

            if (!this.objects.Contains(obj))
                return false;
            
            foreach (var item in this.objects)
            {
                if (item.ExistsHierarchically(obj.Name))
                    throw new RemoveDeniedException(string.Format("Unable to remove the object '{0}' because it is being used by the object '{1}' directly or indirectly.", obj.Name, item.Name));
            }

            this.objects.Remove(obj);
            return true;
        }

        public bool Contains(HierarchicalEntity item)
        {
            this.IsInvalidated();
            return this.objects.Contains(item);
        }

        public HierarchicalEntity Contains(string name)
        {
            this.IsInvalidated();
            return this.objects.FirstOrDefault(f => f.Name == name);
        }

        public int Count
        {
            get 
            {
                this.IsInvalidated();
                return this.objects.Count;    
            }
        }

        public IEnumerator<HierarchicalEntity> GetEnumerator()
        {
            this.IsInvalidated();
            return this.objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            this.IsInvalidated();
            return objects.GetEnumerator();
        }

        private void IsInvalidated()
        {
            if (invalidated)
                throw new InvalidatedCollectionException("The collection is invalidated. Please corrected it or create another.");
        }
    }
}
