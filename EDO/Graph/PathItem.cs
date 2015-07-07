// This program prints out basic information about the crash dump specified.
//
// The platform must match what you are debugging, as we have to load the dac, a native dll.
using EDO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public class PathItem<T>
    {
        public int Level { get; private set; }
        public Vertex<T> Vertex { get; private set; }
        public Edge<T> Edge { get; private set; }
        internal Iteration<T> Key { get; private set; }

        internal PathItem(Iteration<T> key, Edge<T> edge, int level)
        {
            this.Edge = edge;
            this.Level = level;
            this.Key = key;
            this.Vertex = edge.Target;
        }

        public override string ToString()
        {
            return "[" + this.Edge.ToString() + "]";
        }
    }
}