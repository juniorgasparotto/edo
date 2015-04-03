using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO
{
    public class EDObjectCollection : IEnumerable<EDObject>
    {
        private List<EDObject> objects;

        //public IEnumerable<EDObject> Objects
        //{
        //    get 
        //    {
        //        return (IEnumerable<EDObject>)objects;
        //    }
        //}

        public EDObjectCollection()
        {
            this.objects = new List<EDObject>();
        }

        public EDObject GetObjectByName(string name)
        {
            return this.objects.FirstOrDefault(f => f.Name == name);
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

        public void Remove(EDObject obj)
        {
            this.objects.Remove(obj);
            this.objects.RemoveAll(f => f.Name == obj.Name);
        }

        public IEnumerator<EDObject> GetEnumerator()
        {
            return (IEnumerator<EDObject>)objects;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<EDObject>)objects;
        }
    }
}
