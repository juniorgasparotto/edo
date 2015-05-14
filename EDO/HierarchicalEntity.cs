using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EDO
{
    public class HierarchicalEntity
    {
        public string Name { get; private set; }
        public UniqueEntityList References { get; private set; }
        public UniqueEntityList Parents { get; private set; }

        public HierarchicalEntity(string name)
        {
            this.Name = name;
            this.References = new UniqueEntityList();
            this.Parents = new UniqueEntityList();
        }

        public void Add(HierarchicalEntity obj)
        {
            this.ValidateAdd(obj);
            this.References.Add(obj);
            obj.Parents.Add(this);
        }

        public void Remove(HierarchicalEntity obj)
        {
            if (this.References.Remove(obj))
                obj.Parents.Remove(this);
        }

        public bool ExistsHierarchically(string name)
        {
            if (this.FindHierarchically(name) != null)
                return true;

            return false;
        }

        public HierarchicalEntity FindHierarchically(string name)
        {
            var allToName = this.References.Traverse(f => f.References);
            var b = allToName.FirstOrDefault(f => f.IdentityIsEquals(name));
            return b;
        }

        public bool IdentityIsEquals(string name)
        {
            if (this.Name == name)
                return true;

            return false;
        }

        private void ValidateAdd(HierarchicalEntity obj)
        {
            var found = this.FindHierarchically(obj.Name);
            if (found != null && obj != found)
                throw new EntityAlreadyExistsException(string.Format("Object '{0}' already exists directly or indirectly.", obj.Name));
        }

        public HorizontalCollection ToHorizontalCollection()
        {
            var list = new HorizontalCollection();
            list.Add(this);
            return list;
        }
    }
}
