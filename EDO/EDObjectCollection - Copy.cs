//using NCalc;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace EDO
//{
//    public class EDObjectCollection2 : ICollection<EDObject2>
//    {
//        private List<EDObject2> objects;
//        public EDObjectCollection2()
//        {
//            this.objects = new List<EDObject2>();
//        }

//        public void Add(EDObject2 obj)
//        {
//            var exists = this.objects.Exists(f => f == obj);
//            if (!exists)
//            {
//                if (this.objects.Exists(f => f.Name == obj.Name))
//                    throw new Exception(string.Format("Object '{0}' alredy exists in object collection {1}", obj.Name));

//                this.objects.Add(obj);
//            }
//        }

//        public bool Remove(EDObject2 obj)
//        {
//            return this.objects.Remove(obj);
//            //var res = this.objects.Remove(obj);
//            //var res2 = this.objects.RemoveAll(f => f.Name == obj.Name);
//            //return res ? true : (res2 > 0 ? true : false);
//        }

//        public IEnumerator<EDObject2> GetEnumerator()
//        {
//            return this.objects.GetEnumerator();
//        }

//        //public void Reverse()
//        //{
//        //    this.objects.Reverse();
//        //}

//        public void Clear()
//        {
//            this.objects.Clear();
//        }

//        public bool Contains(EDObject2 item)
//        {
//            return this.objects.Contains(item);
//        }

//        public EDObject2 Contains(string name)
//        {
//            return this.objects.FirstOrDefault(f => f.Name == name);
//        }

//        public void CopyTo(EDObject2[] array, int arrayIndex)
//        {
//            this.objects.CopyTo(array, arrayIndex);
//        }

//        public int Count
//        {
//            get 
//            {
//                return this.objects.Count;    
//            }
//        }

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return objects.GetEnumerator();
//        }

//        bool ICollection<EDObject2>.IsReadOnly
//        {
//            get
//            {
//                return ((ICollection<EDObject2>)objects).IsReadOnly;
//            }
//        }
//    }
//}
