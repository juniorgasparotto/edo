using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph
{
    public static class GraphExtension
    {
        public static List<Graph<T>> ToGraphs<T>(this IEnumerable<T> source, Func<T, List<T>> nextVertexCallback)
        {
            return Graph<T>.ToGraphs(source, nextVertexCallback);
        }
    }
}
