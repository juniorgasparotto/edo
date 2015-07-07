// This program prints out basic information about the crash dump specified.
//
// The platform must match what you are debugging, as we have to load the dac, a native dll.
using EDO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public class Edge<T>
    {
        public Vertex<T> Source { get; internal set; }
        public Vertex<T> Target { get; internal set; }
        public Edge(Vertex<T> source, Vertex<T> target)
        {
            this.Source = source;
            this.Target = target;
        }

        public Edge()
        {

        }

        public override string ToString()
        {
            return (Source == null ? "" : Source.ToString()) + ", " + (Target == null ? "" : Target.ToString());
        }
    }
}