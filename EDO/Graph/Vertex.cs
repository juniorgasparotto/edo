// This program prints out basic information about the crash dump specified.
//
// The platform must match what you are debugging, as we have to load the dac, a native dll.
using EDO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public class Vertex<T>
    {
        public T Data { get; private set; }
        public int InternalId { get; private set; }
        private List<T> Descendents { get; set; }

        public Vertex(T data, int internalId)
        {
            this.Data = data;
            this.Descendents = new List<T>();
            this.InternalId = internalId;
        }
        public override string ToString()
        {
            return Data.ToString();
        }
    }
}