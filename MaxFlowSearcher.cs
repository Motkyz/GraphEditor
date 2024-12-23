using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GraphEditor.GraphElements;

namespace GraphEditor
{
    public class MaxFlowSearcher : MainWindow
    {
        const int MAXN = 1000; // Максимальное количество вершин
        const int INF = int.MaxValue; // Константа "бесконечность"

        List<Node> path = new List<Node>();
        List<Edge> edgesPath = new List<Edge>();

        private Action<string> LogUpd;
        private Action<Node, Brush> HighlightNode;
        private Action<Edge, Brush> HighlightEdge;
        private Graph _graph;
        private int _nodesCount;
        private int _source;
        private int _sink; // Количество вершин, источник и сток
        int[,] valueMatrix; // Матрица емкостей
        int[,] flowMatrix; // Матрица потоков
        int[] depths = new int[MAXN]; // Массив глубин
        int[] pointers = new int[MAXN]; // Указатели на текущее ребро

        public MaxFlowSearcher(Graph graph, int source, int sink, Action<string> logUpd, Action<Node, Brush> highlightNode, Action<Edge, Brush> hilglightEdge)
        {
            _graph = graph;
            _nodesCount = _graph.Nodes.Count; // Количество вершин
            _source = source; // Источник
            _sink = sink; // Сток
            valueMatrix = new int[_nodesCount, _nodesCount];
            flowMatrix = new int[_nodesCount, _nodesCount];
            LogUpd = logUpd;
            HighlightNode = highlightNode;
            HighlightEdge = hilglightEdge;
        }

        // Пример инициализации и использования
        public async Task Initialize()
        {
            //Заполняем матрицу ёмкостей
            foreach (Edge e in _graph.Edges)
            {
                valueMatrix[e.FirstNode.Id - 1, e.SecondNode.Id - 1] = int.Parse(e.Value);
            }

            // Запуск алгоритма
            await Dinic();
        }

        // Основная функция алгоритма Диница
        public async Task Dinic()
        {
            int flow = 0;

            while (Bfs())
            {
                Array.Fill(pointers, 0);
                path.Clear();
                edgesPath.Clear();
                path.Add(Node.FindNode(_sink + 1, _graph.Nodes));

                int pushed;
                while ((pushed = Dfs(_source, INF)) != 0)
                {
                    path.Reverse();
                    LogUpd($"Найден путь " + string.Join(" -> ", path));
                    LogUpd($"Его пропускная способность: {pushed}");

                    foreach (Edge e in edgesPath)
                    {
                        e.Value += "/" + pushed;
                        HighlightEdge(e, Edge.ActiveColorLvl1);
                        HighlightNode(e.FirstNode, Node.ActiveColorLvl1);
                        HighlightNode(e.SecondNode, Node.ActiveColorLvl1);
                        await Task.Delay(1000);
                    }

                    await Task.Delay(1000);
                    foreach (Edge e in edgesPath)
                    {
                        e.Value = e.Value.Split('/')[0];
                        HighlightEdge(e, Edge.DefaultColor);
                        HighlightNode(e.FirstNode, Node.DefaultColor);
                        HighlightNode(e.SecondNode, Node.DefaultColor);
                    }
                    edgesPath.Clear();

                    LogUpd($"Теперь максимальный поток = {flow} + {pushed} = {flow + pushed}\n");
                    flow += pushed;
                }
            }

            LogUpd($"Максимальный поток: {flow}");
        }

        // BFS для поиска увеличивающего пути
        bool Bfs()
        {
            Queue<int> queue = new Queue<int>(); // Очередь
            Array.Fill(depths, -1);
            queue.Enqueue(_source);
            depths[_source] = 0;

            while (queue.Count != 0)
            {
                int node = queue.Dequeue();
                for (int to = 0; to < _nodesCount; to++)
                {
                    if (depths[to] == -1 && flowMatrix[node, to] < valueMatrix[node, to])
                    {
                        queue.Enqueue(to);
                        depths[to] = depths[node] + 1;
                        //LogUpd($"Узел {to} добавлен в очередь, глубина: {depths[to]}.");
                    }
                }
            }

            return depths[_sink] != -1;
        }

        // DFS для отправки потока
        int Dfs(int node, int flow)
        {
            if (flow == 0)
            {
                return 0;
            }

            if (node == _sink)
            {
                //LogUpd($"Достигнут сток, возвращаем поток: {flow}.");
                return flow;
            }
 
            for (int to = pointers[node]; to < _nodesCount; to++)
            {
                if (depths[to] != depths[node] + 1)
                {
                    continue;
                }

                int pushed = Dfs(to, Math.Min(flow, valueMatrix[node, to] - flowMatrix[node, to]));
                if (pushed > 0)
                {
                    path.Add(Node.FindNode(node + 1, _graph.Nodes));
                    edgesPath.Add(Edge.FindEdge(path[path.Count - 1], path[path.Count - 2], _graph.Edges));
                    flowMatrix[node, to] += pushed;
                    flowMatrix[to, node] -= pushed;
                    return pushed;
                }

                pointers[node]++;
            }

            return 0;
        }
    }
}
