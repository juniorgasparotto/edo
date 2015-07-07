using System.Collections;
using System.Collections.Generic;

namespace Graph
{
    public class Path<T> : IEnumerable<PathItem<T>>
    {
        private List<PathItem<T>> pathItems;

        public Path()
        {
            this.pathItems = new List<PathItem<T>>();
        }

        internal void Add(PathItem<T> item)
        {
            this.pathItems.Add(item);
        }

        public override string ToString()
        {
            var output = "";
            foreach (var item in pathItems)
                output += (output == "") ? item.ToString() : "." + item.ToString();
            return output;
        }

        public IEnumerator<PathItem<T>> GetEnumerator()
        {
            return pathItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return pathItems.GetEnumerator();
        }
    }
}