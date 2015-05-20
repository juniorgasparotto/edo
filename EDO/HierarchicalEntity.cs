using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EDO
{
    public class HierarchicalEntity
    {
        public UniqueIdentityList parents;
        public UniqueIdentityList children;

        public string Identity { get; private set; }

        public List<HierarchicalEntity> Children 
        {
            get
            {
                return children.ToList();
            }
        }

        public List<HierarchicalEntity> Parents
        {
            get
            {
                return parents.ToList();
            }
        }

        public HierarchicalEntity(string id)
        {
            this.Identity = id;
            this.children = new UniqueIdentityList();
            this.parents = new UniqueIdentityList();
        }

        public void AddChild(HierarchicalEntity obj)
        {
            //this.ValidateAdd(obj);
            this.children.Add(obj);
            obj.parents.Add(this);
        }

        public void RemoveChild(HierarchicalEntity obj)
        {
            if (this.children.Remove(obj))
                obj.parents.Remove(this);
        }

        public bool ExistsHierarchically(string id)
        {
            if (this.FindHierarchically(id) != null)
                return true;

            return false;
        }

        public HierarchicalEntity FindHierarchically(string id)
        {
            return this.Descendants().FirstOrDefault(f => f.IdentityIsEquals(id));
        }

        public List<HierarchicalEntity> Descendants()
        {
            return this.Children.Traverse(f => f.Children).ToList();
        }

        public List<HierarchicalEntity> DescendantsAndThis()
        {
            var list = this.Descendants();
            if (!list.Contains(this))
                list.Add(this);
            return list;
        }

        public bool IdentityIsEquals(string name)
        {
            if (this.Identity == name)
                return true;

            return false;
        }

        //private void ValidateAdd(HierarchicalEntity obj)
        //{
        //    var found = this.FindHierarchically(obj.Identity);
        //    if (found != null && obj != found)
        //        throw new EntityAlreadyExistsException(string.Format("Object '{0}' already exists directly or indirectly.", obj.Identity));
        //}

        public void Validate()
        {
            var validateIdDuplicated = new UniqueIdentityList();
            var listToValidade = this.DescendantsAndThis();
            foreach (var item in listToValidade)
            {
                try
                {
                    validateIdDuplicated.Add(item);
                    validateIdDuplicated.AddRange(item.Children.Traverse(f => f.Children));
                }
                catch(EntityAlreadyExistsException ex)
                {
                    validateIdDuplicated.Clear();
                    throw ex;
                }
            }
        }
    }
}
