// This program prints out basic information about the crash dump specified.
//
// The platform must match what you are debugging, as we have to load the dac, a native dll.

using EDO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph
{
    public class Graph<T>
    {
        private List<List<KeyValuePair<Iteration<T>, Edge<T>>>> storagePathes;
        private List<KeyValuePair<Iteration<T>, Edge<T>>> currentPath;
        private List<KeyValuePair<Iteration<T>, Edge<T>>> lastPath;
        private List<Vertex<T>> vertexes;
        private List<Edge<T>> edges;
        private List<Path<T>> storagePathes2;
        private Path<T> currentPath2;
        private Path<T> lastPath2;

        public int CountIteration { get; private set; }
        public string ReadingOrder { get; private set; }
        
        public IEnumerable<Edge<T>> Edges
        {
            get
            {
                return edges.AsEnumerable();
            }
        }

        public IEnumerable<Vertex<T>> Vertexes
        {
            get
            {
                return vertexes.AsEnumerable();
            }
        }

        public IEnumerable<Path<T>> Pathes
        {
            get
            {
                return storagePathes2.AsEnumerable();
            }
        }


        private Graph()
        {
            //this.storagePathes = new List<List<KeyValuePair<Iteration<T>, Edge<T>>>>();
            //this.currentPath = new List<KeyValuePair<Iteration<T>, Edge<T>>>();
            
            this.storagePathes2 = new List<Path<T>>();
            this.currentPath2 = new Path<T>();
            this.vertexes = new List<Vertex<T>>();
            this.edges = new List<Edge<T>>();
        }

        public static List<Graph<T>> ToGraphs(IEnumerable<T> source, Func<T, List<T>> nextVertexCallback)
        {
            var graph = new Graph<T>();
            var graphs = new List<Graph<T>>();
            
            var iteration = new Iteration<T>()
            {
                Enumerator = source.GetEnumerator(),
                Level = 0,
                DataKeyIteration = default(T)
            };

            var iterations = new List<Iteration<T>>();
            iterations.Add(iteration);
            
            var countIterationForAll = 0;
            var allCountAlreadyFinishedIterationForAll = 0;

            while (true)
            {
                bool executed = false;
                
                if (iteration.Level == 0)
                {
                    graph = new Graph<T>();
                    graphs.Add(graph);
                }

                while (iteration.Enumerator.MoveNext())
                {
                    countIterationForAll++;
                    graph.CountIteration++;
                    executed = true;

                    var data = iteration.Enumerator.Current;
                    var dataParent = iteration.IterationParent != null ? iteration.IterationParent.Enumerator.Current : default(T);

                    // Create or get current vertex
                    Vertex<T> vertex = graph.vertexes.Where(f=> f.Data.Equals(data)).FirstOrDefault();
                    if (vertex == null)
                    {
                        vertex = new Vertex<T>(data, graph.vertexes.Count + 1);
                        graph.vertexes.Add(vertex);
                    }
                    
                    // Get parent vertex
                    Vertex<T> vertexParent = null;
                    if (dataParent != null)
                        vertexParent = graph.vertexes.Where(f => f.Data.Equals(dataParent)).FirstOrDefault();

                    // Prevent recursion, infinite loop. eg: "A + B + [A]" where [A] already exists in path
                    var exists = graph.VertexExistsInCurrentPath(vertex);

                    var edge = graph.edges.LastOrDefault(f => f.Source == vertexParent && f.Target == vertex);
                    if (edge == null)
                    {
                        edge = new Edge<T>();
                        edge.Source = vertexParent;
                        edge.Target = vertex;
                        graph.edges.Add(edge);
                    }
                    
                    graph.AddInCurrentPath(iteration, edge);

                    // Specify sequencial iteration
                    graph.ReadingOrder += (string.IsNullOrWhiteSpace(graph.ReadingOrder) ? "" : ".") + "[" + vertex.ToString() + "]";

                    // Get nexts vertexes
                    var nexts = nextVertexCallback(data);

                    if (nexts != null && nexts.Count > 0 && !exists)
                    {
                        iteration = new Iteration<T>()
                        {
                            Enumerator = nexts.GetEnumerator(),
                            Level = iteration.Level + 1,
                            //DataKeyIteration = iteration.Enumerator.Current,
                            IterationParent = iteration
                        };

                        iterations.Add(iteration);
                    }
                    else
                    {
                        graph.ClosePath();
                    }
                }

                if (!executed)
                {
                    allCountAlreadyFinishedIterationForAll++;
                    graph.CountIteration++;
                }

                // Remove iteration because is empty
                iterations.Remove(iteration);

                if (iterations.Count == 0)
                    break;

                iteration = iterations.LastOrDefault();
            }

            var debug = countIterationForAll + allCountAlreadyFinishedIterationForAll;
            return graphs;
        }

        private void AddInCurrentPath(Iteration<T> iteration, Edge<T> edge)
        {
            //if (this.currentPath == null)
            //{
            //    this.currentPath = new List<KeyValuePair<Iteration<T>, Edge<T>>>();

            //    if (lastPath != null && lastPath.Where(f => f.Key == iteration).Any())
            //    {
            //        foreach (var path in lastPath)
            //            if (path.Key == iteration)
            //                break;
            //            else
            //                this.currentPath.Add(path);
            //    }
            //}

            //this.currentPath.Add(new KeyValuePair<Iteration<T>, Edge<T>>(iteration, edge));

            //***
            if (this.currentPath2 == null)
            {
                this.currentPath2 = new Path<T>();

                if (lastPath2 != null && lastPath2.Where(f => f.Key == iteration).Any())
                {
                    foreach (var path in lastPath2)
                        if (path.Key == iteration)
                            break;
                        else
                            this.currentPath2.Add(path);
                }
            }

            this.currentPath2.Add(new PathItem<T>(iteration, edge, iteration.Level));
        }

        private void ClosePath()
        {
            //this.storagePathes.Add(currentPath);
            //this.lastPath = this.currentPath;
            //this.currentPath = null;

            this.storagePathes2.Add(currentPath2);
            this.lastPath2 = this.currentPath2;
            this.currentPath2 = null;
        }

        private bool VertexExistsInCurrentPath(Vertex<T> vertex)
        {
            //if (this.currentPath != null)
            //    return this.currentPath.Where(f => f.Value == edge).Any();

            if (this.currentPath2 != null)
                return this.currentPath2.Where(f => f.Edge.Target == vertex).Any();

            return false;
        }
    }
}