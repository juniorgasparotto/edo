using NCalc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EDO
{
    public class EDObject
    {
        public string Name { get; private set; }
        public List<EDObject> References { get; private set; }
        public List<EDObject> Parents { get; private set; }
        public EDObjectCollection Collection { get; private set; }

        public EDObject(string name)
        {
            this.Name = name;
            this.References = new List<EDObject>();
            this.Parents = new List<EDObject>();
            this.Collection = new EDObjectCollection();
        }

        public void AddReference(EDObject obj)
        {
            var exists = this.References.Exists(f => f == obj);
            if (!exists)
            {
                if (this.References.Exists(f => f.Name == obj.Name))
                    throw new Exception(string.Format("Object '{0}' alredy exists in object {1}", obj.Name, this.Name));

                this.References.Add(obj);
                obj.Parents.Add(this);
            }

            this.Collection.Add(obj);

        }

        public void RemoveReference(EDObject obj)
        {
            var countRem = this.References.RemoveAll(f => f.Name == obj.Name);
            
            if (countRem > 0) 
            {
                obj.Parents.Remove(this);
                obj.Parents.RemoveAll(f => f.Name == this.Name);
            }

            //if (this.Collection != null)
            //    this.Collection.Remove(obj);
        }

        public bool HasDirectOrIndirectReference(EDObject objectVerify)
        {
            foreach (var p in this.References)
            {
                if (p == objectVerify)
                    return true;

                if (p.HasDirectOrIndirectReference(objectVerify))
                    return true;
            }

            return false;
        }
    }
}
