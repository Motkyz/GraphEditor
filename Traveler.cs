using GraphEditor.GraphElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GraphEditor
{
    public class Traveler : MainWindow
    {
        CancellationTokenSource? _cancellationTokenSource;

        private Graph _graph;
        private Action<string> LogUpd;
        private Action<Node, Brush> HighlightNode;
        private Action<Edge, Brush> HighlightEdge;

        public Traveler(Graph graph, Action<string> logUpd, Action<Node, Brush> highlightNode, Action<Edge, Brush> highlightEdge,
            CancellationTokenSource cancellationToken)
        {
            _graph = graph;
            LogUpd = logUpd;
            HighlightNode = highlightNode;
            HighlightEdge = highlightEdge;
            _cancellationTokenSource = cancellationToken;
        }

        public async Task DepthTravel(Node start)
        {
            HashSet<Node> visited = [];
            Stack<Node> stack = [];
            stack.Push(start);


            LogUpd("Начат алгоритм обхода в глубину");

            await Task.Delay(1500);

            while (stack.Count > 0)
            {
                Node currentNode = stack.Pop();
                if (!visited.Contains(currentNode))
                {
                    visited.Add(currentNode);
                    HighlightNode(currentNode, Node.ActiveColorLvl1);
                    LogUpd($"\nПосещаем узел [{currentNode}]");
                    await Task.Delay(1500);

                    foreach (var edge in _graph.Edges)
                    {
                        _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
                        if (edge.FirstNode == currentNode || edge.SecondNode == currentNode)
                        {
                            Node neighbor = edge.FirstNode == currentNode ? edge.SecondNode : edge.FirstNode;
                            if (!visited.Contains(neighbor))
                            {
                                stack.Push(neighbor);
                                HighlightEdge(edge, Edge.ActiveColorLvl1);
                                HighlightNode(neighbor, Node.ActiveColorLvl2);
                                LogUpd($"Переходим по рёбру от узла [{currentNode}] к узлу [{neighbor}]");
                                await Task.Delay(1500);
                            }
                        }
                    }
                }
            }

            LogUpd("\nОбход графа в глубину завершён");
        }

        public async Task WidthTravel(Node start)
        {
            HashSet<Node> visited = [];
            Queue<Node> queue = [];
            queue.Enqueue(start);

            LogUpd("Начат алгоритм обхода в ширину");

            await Task.Delay(1500);

            while (queue.Count > 0)
            {
                Node currentNode = queue.Dequeue();
                if (!visited.Contains(currentNode))
                {
                    visited.Add(currentNode);
                    HighlightNode(currentNode, Node.ActiveColorLvl1);
                    LogUpd($"\nПосещаем узел [{currentNode}]");
                    await Task.Delay(1500);

                    foreach (var edge in _graph.Edges)
                    {
                        _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
                        if (edge.FirstNode == currentNode || edge.SecondNode == currentNode)
                        {
                            Node neighbor = edge.FirstNode == currentNode ? edge.SecondNode : edge.FirstNode;
                            if (!visited.Contains(neighbor))
                            {
                                queue.Enqueue(neighbor);
                                HighlightEdge(edge, Edge.ActiveColorLvl1);
                                LogUpd($"Добавляем в очередь узел [{neighbor}], соседний с узлом [{currentNode}]");
                                HighlightNode(neighbor, Node.ActiveColorLvl2);
                                await Task.Delay(1500);
                            }
                        }
                    }
                }
            }

            LogUpd("\nОбход графа в ширину завершён");
        }
    }
}
