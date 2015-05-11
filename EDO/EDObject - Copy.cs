using NCalc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDO
{
    public class EdoList : List<EDObject>
    {
        private bool ValidateAdd(EDObject obj)
        {
            var exists = this.Exists(f => f == obj);
            if (!exists)
            {
                if (this.Exists(f => f.Name == obj.Name))
                    throw new Exception(string.Format("Object '{0}' alredy exists in object collection {1}", obj.Name));
            }

            return !exists;
        }

        public new void Add(EDObject obj)
        {
            if (this.ValidateAdd(obj))
                base.Add(obj);
        }

        public new void Insert(int index, EDObject item)
        {
            if (this.ValidateAdd(item))
                base.Insert(index, item);
        }

        public new EDObject this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (this.ValidateAdd(value))
                    base[index] = value;
            }
        }

        public new void CopyTo(EDObject[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length; i++)
                this.Add(array[i]);
        }
    }
}
