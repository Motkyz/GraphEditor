using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphEditor.GraphElements
{
    public class Graph
    {
        public List<Node> Nodes;
        public List<Edge> Edges;
        public int count = 1;

        public Graph()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

        public Graph(List<Node> Nodes, List<Edge> Edges)
        {
            this.Nodes = Nodes;
            this.Edges = Edges;
            count = Nodes.Count + 1;
        }

        public void AddNode(Node node)
        {
            Nodes.Add(node);
            count++;
        }

        public void AddEdge(Node first, Node second, int value)
        {
            Edges.Add(new Edge(first, second, value));
        }

        public void RemoveNode(Node node)
        {
            Nodes.Remove(node);
            Edges.RemoveAll(e => e.FirstNode == node || e.SecondNode == node);
            count--;
        }

        public void RemoveEdge(Edge e)
        {
            Edges.Remove(e);
        }
    }
}
