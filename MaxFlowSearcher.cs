using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using GraphEditor.GraphElements;

namespace GraphEditor
{
    public class MaxFlowSearcher : MainWindow
    {
        CancellationTokenSource? _cancellationTokenSource;

        private Graph _graph;

        private int _nodesCount;
        private int Start;
        private int End;
        private int[,] MatrixOfGraph;

        private Action<string> LogUpd;
        private Action<Node, Brush> HighlightNode;
        private Action<Edge, Brush> HighlightEdge;

        public MaxFlowSearcher(Graph graph, Node startNode, Node endNode, Action<string> logUpd,
            Action<Node, Brush> highlightNode, Action<Edge, Brush> highlightEdge,
            CancellationTokenSource cancellationToken)
        {
            _graph = graph;
            _nodesCount = graph.Nodes.Count;

            Start = startNode.Id - 1;
            End = endNode.Id - 1;

            MatrixOfGraph = GetMatrixOfGraph();
            LogUpd = logUpd;
            HighlightNode = highlightNode;
            HighlightEdge = highlightEdge;
            _cancellationTokenSource = cancellationToken;
        }

        private int[,] GetMatrixOfGraph()
        {
            int[,] result = new int[_nodesCount, _nodesCount];
            foreach (var edge in _graph.Edges)
            {
                int startNode = edge.FirstNode.Id - 1;
                int endNode = edge.SecondNode.Id - 1;

                if (startNode >= 0 && startNode < _nodesCount && endNode >= 0 && endNode < _nodesCount)
                {
                    result[startNode, endNode] = int.Parse(edge.Value);
                    result[endNode, startNode] = int.Parse(edge.Value);
                }
            }

            return result;
        }

        private bool Bfs(int[,] rGraph, int start, int end, int[] path)
        {
            bool[] visitedNodes = new bool[_nodesCount];
            for (int i = 0; i < _nodesCount; ++i)
            {
                visitedNodes[i] = false;
            }

            Queue<int> queue = new Queue<int>();
            queue.Enqueue(start);
            visitedNodes[start] = true;
            path[start] = -1;

            while (queue.Count != 0)
            {
                int u = queue.Dequeue();

                for (int v = 0; v < _nodesCount; v++)
                {
                    if (visitedNodes[v] == false && rGraph[u, v] > 0)
                    {
                        if (v == end)
                        {
                            path[v] = u;
                            return true;
                        }

                        queue.Enqueue(v);
                        path[v] = u;
                        visitedNodes[v] = true;
                    }
                }
            }

            return false;
        }

        public async Task FordFulkerson()
        {
            int u, v;

            int[,] rGraph = new int[_nodesCount, _nodesCount];
            for (u = 0; u < _nodesCount; u++)
            {
                for (v = 0; v < _nodesCount; v++)
                {
                    rGraph[u, v] = MatrixOfGraph[u, v];
                }
            }

            int[] paths = new int[_nodesCount];

            int maxFlow = 0;

            while (Bfs(rGraph, Start, End, paths))
            {
                _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                int path_flow = int.MaxValue;

                List<string> path = new();
                path.Add((End + 1).ToString());

                int prevNode = End;

                List<Edge> edgesPath = new List<Edge>();

                for (v = End; v != Start; v = paths[v])
                {
                    u = paths[v];
                    path_flow = Math.Min(path_flow, rGraph[u, v]);

                    foreach (var e in _graph.Edges)
                    {
                        if (e.FirstNode.Id - 1 == prevNode && e.SecondNode.Id - 1 == u
                            || e.SecondNode.Id - 1 == prevNode && e.FirstNode.Id - 1 == u)
                        {
                            edgesPath.Add(e);
                        }
                    }

                    prevNode = u;
                    path.Add((u + 1).ToString());
                }

                path.Reverse();
                LogUpd($"Найден путь " + string.Join(" -> ", path));             

                foreach (Edge e in edgesPath)
                {
                    _cancellationTokenSource!.Token.ThrowIfCancellationRequested();

                    HighlightEdge(e, Edge.ActiveColorLvl1);
                    HighlightNode(e.FirstNode, Node.ActiveColorLvl1);
                    HighlightNode(e.SecondNode, Node.ActiveColorLvl1);
                    await Task.Delay(1000);
                }
                foreach (Edge e in edgesPath)
                {
                    int temp = e.Value.Split('/').Length > 1 ? int.Parse(e.Value.Split('/')[1]) : 0;
                    e.Value = e.Value.Split('/')[0] + "/" + (path_flow + temp);
                }
                HighlightEdge(edgesPath[0], Edge.ActiveColorLvl1);

                await Task.Delay(1000);
                foreach (Edge e in edgesPath)
                {
                    HighlightEdge(e, Edge.DefaultColor);
                    HighlightNode(e.FirstNode, Node.DefaultColor);
                    HighlightNode(e.SecondNode, Node.DefaultColor);
                }
                edgesPath.Clear();

                for (v = End; v != Start; v = paths[v])
                {
                    u = paths[v];
                    rGraph[u, v] -= path_flow;
                    rGraph[v, u] += path_flow;
                }

                LogUpd($"Его пропускная способность: {path_flow}");
                LogUpd($"Теперь максимальный поток: {maxFlow} + {path_flow} = {maxFlow + path_flow}\n");
                maxFlow += path_flow;              

                await Task.Delay(1000);
            }

            LogUpd($"Максимальный поток от узла [{Start + 1}] к узлу [{End + 1}]: {maxFlow}");
        }
    }
}
