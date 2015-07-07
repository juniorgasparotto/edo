// This program prints out basic information about the crash dump specified.
//
// The platform must match what you are debugging, as we have to load the dac, a native dll.

using EDO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph 
{
    internal class Iteration<T>
    {
        public IEnumerator<T> Enumerator { get; set; }
        public T DataKeyIteration { get; set; }
        public int Level { get; set; }

        //public List<T> Items { get; set; }
        public Iteration()
        {
            //Items = new List<T>();
        }

        public override string ToString()
        {
            if (DataKeyIteration == null) 
                return "";

            return DataKeyIteration.ToString();
        }

        public Iteration<T> IterationParent { get; set; }
    }
}