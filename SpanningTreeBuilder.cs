using GraphEditor.GraphElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GraphEditor
{
    public class SpanningTreeBuilder : MainWindow
    {
        CancellationTokenSource? _cancellationTokenSource;

        private Graph _graph;
        private Action<string> LogUpd;
        private Action<Node, Brush> HighlightNode;
        private Action<Edge, Brush> HighlightEdge;

        public SpanningTreeBuilder(Graph graph, Action<string> logUpd, Action<Node, Brush> highlightNode, Action<Edge, Brush> highlightEdge, 
            CancellationTokenSource cancellationToken)
        {
            _graph = graph;
            LogUpd = logUpd;
            HighlightNode = highlightNode;
            HighlightEdge = highlightEdge;
            _cancellationTokenSource = cancellationToken;
        }

        public async Task PrimAlgorithm(Node start)
        {
            List<Edge> usedEdges = new List<Edge>();
            List<Edge> notUsedEdges = new List<Edge>(_graph.Edges);

            List<Node> usedNodes = new List<Node>();
            List<Node> notUsedNodes = new List<Node>(_graph.Nodes);

            int numberV = notUsedNodes.Count;

            LogUpd($"Отмечаем начальный узел [{start}]\n");
            usedNodes.Add(start);
            notUsedNodes.Remove(start);
            HighlightNode(start, Node.ActiveColorLvl1);

            while (notUsedNodes.Count > 0)
            {
                int minEdgeVal = -1;
                //LogUpd($"Ищем минимальное ребро между соседними узлами");
                LogUpd($"Просматриваем все рёбра и ищем минимальное между отмеченным и не отмеченным узлом:");
                for (int i = 0; i < notUsedEdges.Count; i++)
                {
                    if (usedNodes.Contains(notUsedEdges[i].FirstNode) && notUsedNodes.Contains(notUsedEdges[i].SecondNode) ||
                        usedNodes.Contains(notUsedEdges[i].SecondNode) && notUsedNodes.Contains(notUsedEdges[i].FirstNode))
                    {
                        HighlightEdge(notUsedEdges[i], Edge.SelectColor);
                        if (usedNodes.Contains(notUsedEdges[i].FirstNode))
                        {
                            HighlightNode(notUsedEdges[i].FirstNode, Node.ActiveColorLvl1);
                            HighlightNode(notUsedEdges[i].SecondNode, Node.ActiveColorLvl2);
                            
                        }
                        else
                        {
                            HighlightNode(notUsedEdges[i].SecondNode, Node.ActiveColorLvl1);
                            HighlightNode(notUsedEdges[i].FirstNode, Node.ActiveColorLvl2);
                        }
                        LogUpd($"Смотрим ребро между узлами [{notUsedEdges[i].FirstNode}] и [{notUsedEdges[i].SecondNode}], его вес {notUsedEdges[i].Value}, запомним его");
                        _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
                        await Task.Delay(1000);

                        if (minEdgeVal == -1)
                        {
                            minEdgeVal = i;
                        }
                        else if (int.Parse(notUsedEdges[i].Value) < int.Parse(notUsedEdges[minEdgeVal].Value))
                        {
                            minEdgeVal = i;
                        }
                        _cancellationTokenSource!.Token.ThrowIfCancellationRequested();
                        await Task.Delay(1000);

                        HighlightEdge(notUsedEdges[i], Edge.SelectColorLvl2);
                    }
                }

                LogUpd($"\nМинимальное ребро - ребро между узлами [{notUsedEdges[minEdgeVal].FirstNode.Id}] и [{notUsedEdges[minEdgeVal].SecondNode.Id}] с весом {notUsedEdges[minEdgeVal].Value}, помечаем его");
                
                HighlightEdge(notUsedEdges[minEdgeVal], Edge.SelectColor);
                await Task.Delay(1000);

                if (usedNodes.Contains(notUsedEdges[minEdgeVal].FirstNode))
                {
                    LogUpd($"Также помечаем узел [{notUsedEdges[minEdgeVal].SecondNode.Id}]\n");
                    HighlightNode(notUsedEdges[minEdgeVal].SecondNode, Node.ActiveColorLvl1);
                    usedNodes.Add(notUsedEdges[minEdgeVal].SecondNode);
                    notUsedNodes.Remove(notUsedEdges[minEdgeVal].SecondNode);
                }
                else
                {
                    LogUpd($"Также помечаем узел [{notUsedEdges[minEdgeVal].FirstNode.Id}]\n");
                    HighlightNode(notUsedEdges[minEdgeVal].FirstNode, Node.ActiveColorLvl1);
                    usedNodes.Add(notUsedEdges[minEdgeVal].FirstNode);
                    notUsedNodes.Remove(notUsedEdges[minEdgeVal].FirstNode);
                }
                //заносим новое ребро в дерево и удаляем его из списка неиспользованных
                usedEdges.Add(notUsedEdges[minEdgeVal]);
                notUsedEdges.RemoveAt(minEdgeVal);

                HighlightEdge(usedEdges[usedEdges.Count-1], Edge.ActiveColorLvl1);

                foreach (var n in notUsedNodes)
                {
                    HighlightNode(n, Node.DefaultColor);
                }
                foreach (var e in notUsedEdges) 
                {
                    HighlightEdge(e, Edge.DefaultColor);
                }
            }

            foreach (var edge in notUsedEdges)
            {
                HighlightEdge(edge, Edge.UnActiveColor);
                edge.TextBlock.Foreground = Edge.UnActiveColor;
            }

            LogUpd("Алгоритм Прима завершён\n");
        }
    }
}
