using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO
{
    public class Vertex<T>
    {
        public T Data { get; private set; }

        public Vertex(T data)
        {
            this.Data = data;
        }
    }
}
