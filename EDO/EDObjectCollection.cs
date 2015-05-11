using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO
{
    public class EDObjectCollection : ICollection<EDObject>
    {
        private List<EDObject> objects;
        public EDObjectCollection()
        {
            this.objects = new List<EDObject>();
        }

        public void Add(EDObject obj)
        {
            var exists = this.objects.Exists(f => f == obj);
            if (!exists)
            {
                if (this.objects.Exists(f => f.Name == obj.Name))
                    throw new Exception(string.Format("Object '{0}' alredy exists in object collection {1}", obj.Name));

                this.objects.Add(obj);
            }
        }

        public bool Remove(EDObject obj)
        {
            return this.objects.Remove(obj);
            //var res = this.objects.Remove(obj);
            //var res2 = this.objects.RemoveAll(f => f.Name == obj.Name);
            //return res ? true : (res2 > 0 ? true : false);
        }

        public IEnumerator<EDObject> GetEnumerator()
        {
            return this.objects.GetEnumerator();
        }

        //public void Reverse()
        //{
        //    this.objects.Reverse();
        //}

        public void Clear()
        {
            this.objects.Clear();
        }

        public bool Contains(EDObject item)
        {
            return this.objects.Contains(item);
        }

        public EDObject Contains(string name)
        {
            return this.objects.FirstOrDefault(f => f.Name == name);
        }

        public void CopyTo(EDObject[] array, int arrayIndex)
        {
            this.objects.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get 
            {
                return this.objects.Count;    
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return objects.GetEnumerator();
        }

        bool ICollection<EDObject>.IsReadOnly
        {
            get
            {
                return ((ICollection<EDObject>)objects).IsReadOnly;
            }
        }
    }
}
