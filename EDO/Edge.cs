using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO
{
    public class Edge<T>
    {
        public Vertex<T> Source { get; private set; }
        public Vertex<T> Target { get; private set; }
        public Edge(Vertex<T> source, Vertex<T> target)
        {
            this.Source = source;
            this.Target = target;
        }
    }
}
