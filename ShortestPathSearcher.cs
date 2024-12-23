using GraphEditor.GraphElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace GraphEditor
{
    public class ShortestPathSearcher
    {
        private Graph _graph;
        private Action<string> LogUpd;
        private Action<Node, Brush> HighlightNode;
        private Action<Edge, Brush> HighlightEdge;
        private Dictionary<Node, Dictionary<Node, double>> graph = new();
        private Dictionary<int, Node> SavedNodesForPath = new Dictionary<int, Node>();

        private Node Start;
        private Node End;

        public ShortestPathSearcher(Graph graph, Node start, Node end, 
            Action<string> logUpd, Action<Node, Brush> highlightNode, Action<Edge, Brush> highlightEdge)
        {
            _graph = graph;
            LogUpd = logUpd;
            HighlightNode = highlightNode;
            HighlightEdge = highlightEdge;
            Start = start;
            End = end;
        }

        public async Task Initialize()
        {
            //Построение структуры, нужной для алгоритма из переданного графа
            foreach (Edge e in _graph.Edges)
            {
                graph.TryAdd(e.FirstNode, new Dictionary<Node, double>() { { e.SecondNode, int.Parse(e.Value) } });
                graph.TryAdd(e.SecondNode, new Dictionary<Node, double>() { { e.FirstNode, int.Parse(e.Value) } });

                graph[e.FirstNode].TryAdd(e.SecondNode, int.Parse(e.Value));
                graph[e.SecondNode].TryAdd(e.FirstNode, int.Parse(e.Value));
            }

            await Dijkstra();
        }

        public async Task Dijkstra()
        {
            var distances = new Dictionary<Node, double>();
            var priorityQueue = new SortedSet<(double distance, Node node)>();

            // Инициализация дистанций
            foreach (var node in graph.Keys)
            {
                distances[node] = double.PositiveInfinity;
                node.TextBlockDistance.Text = "∞";
            }
            distances[Start] = 0;
            priorityQueue.Add((0, Start));
            Start.TextBlockDistance.Text = "0";

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Min;
                priorityQueue.Remove(current);

                double currentDistance = current.distance;
                Node currentNode = current.node;

                LogUpd($"Выбираем ближайший узел - [{currentNode}]");
                LogUpd($"Текущее расстояние до него - {currentDistance}\n");

                // Обрабатываем только вершину с наименьшим расстоянием
                if (currentDistance > distances[currentNode])
                {
                    continue;
                }

                // Обрабатываем соседей
                LogUpd($"Просматриваем соседей для узла [{currentNode}]");
                HighlightNode(currentNode, Node.ActiveColorLvl1);
                foreach (var neighbor in graph[currentNode])
                {
                    Edge chosenEdge;
                    Node neighbourNode = neighbor.Key;
                    double weight = neighbor.Value;
                    double distance = currentDistance + weight;

                    LogUpd($"Рассматриваем узел [{neighbor.Key}]");
                    HighlightNode(neighbourNode, Node.ActiveColorLvl2);
                    LogUpd($"Расстояние до узла [{neighbourNode}]: {currentDistance} + {weight} = {distance}");
                    HighlightEdge(chosenEdge = Edge.FindEdge(currentNode, neighbourNode, _graph.Edges), Edge.ActiveColorLvl1);
                    await Task.Delay(1000);

                    // Рассматриваем этот новый путь только в том случае, если он лучше любого пути, который мы нашли до сих пор
                    if (distance < distances[neighbourNode])
                    {
                        LogUpd($"Так как новый путь ({distance}) короче предыдущего ({distances[neighbourNode]}), то запоминаем его\n");
                        distances[neighbourNode] = distance;
                        priorityQueue.Add((distance, neighbourNode));
                        SavedNodesForPath[neighbourNode.Id] = currentNode;
                        neighbourNode.TextBlockDistance.Text = $"{distance}";
                    }
                    else
                    {
                        LogUpd($"Так как новый путь ({distance}) длиннее предыдущего ({distances[neighbourNode]}), то пропускаем его\n");
                    }

                    HighlightNode(neighbourNode, Node.DefaultColor);
                    HighlightEdge(chosenEdge, Edge.DefaultColor);
                }

                HighlightNode(currentNode, Node.DefaultColor);
            }

            var path = new List<Node>();
            var nodeForPath = End;
            while (nodeForPath != null)
            {
                path.Insert(0, nodeForPath);
                nodeForPath = SavedNodesForPath.ContainsKey(nodeForPath.Id) ? SavedNodesForPath[nodeForPath.Id] : null;
            }

            for (int i = 0; i < path.Count - 1; i++) 
            {
                HighlightNode(path[i], Node.ActiveColorLvl1);
                HighlightEdge(Edge.FindEdge(path[i], path[i+1], _graph.Edges), Edge.ActiveColorLvl1);
            }
            HighlightNode(End, Node.ActiveColorLvl1);
            LogUpd($"Кратчайший путь: {string.Join(" -> ", path)}");
            LogUpd($"Длина кратчайшего пути: {distances[End]}");
        }
    }
}
