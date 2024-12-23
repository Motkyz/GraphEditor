using GraphEditor.GraphElements;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphEditor
{
    public class FileHandler
    {
        const int WordCoordId = 0;
        const int CoordinateXId = 1;
        const int CoordinateYId = 2;
        const int coordinatePlaces = 3;

        public static Graph Read(string filePath)
        {
            List<string[]> stringsNodes = new List<string[]>();
            List<Node> nodes = new List<Node>();
            List<Edge> edges = new List<Edge>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] ar = line.Split(";");
                    stringsNodes.Add(ar);
                }
            }

            for (int i = 0; i < stringsNodes.Count; i++)
            {
                double x = double.Parse(stringsNodes[i][stringsNodes.Count + CoordinateXId]);
                double y = double.Parse(stringsNodes[i][stringsNodes.Count + CoordinateYId]);
                nodes.Add(new Node(i + 1, x, y));
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = 0; j < nodes.Count; j++)
                {
                    int lengthConnection = int.Parse(stringsNodes[i][j]);
                    if (lengthConnection != 0)
                    {
                        Edge newEdge = new Edge(nodes[i], nodes[j], lengthConnection);

                        if (!Edge.EdgeExist(edges, newEdge))
                        {
                            edges.Add(newEdge);
                        }
                    }
                }
            }

            return new Graph(nodes, edges);
        }

        public static void WriteToFile(Graph graph, string filePath)
        {
            int countOfVertice = graph.Nodes.Count;
            List<string[]> stringsVertices = new List<string[]>();
            for (int i = 0; i < countOfVertice; i++)
            {
                string[] arrayConnectionAndCoordinate = new string[countOfVertice + coordinatePlaces];
                for (int j = 0; j < countOfVertice; j++)
                {
                    Edge checkEdge = null;
                    if (i == j)
                    {
                        arrayConnectionAndCoordinate[j] = "0";
                    }
                    else
                    {
                        checkEdge = Edge.FindEdge(
                        Node.FindNode(graph.Nodes[i].Id, graph.Nodes),
                        Node.FindNode(graph.Nodes[j].Id, graph.Nodes),
                        graph.Edges);
                    }

                    if (checkEdge != null)
                    {
                        arrayConnectionAndCoordinate[j] = checkEdge.Value.ToString();
                    }
                    else
                    {
                        arrayConnectionAndCoordinate[j] = "0";
                    }
                }

                stringsVertices.Add(arrayConnectionAndCoordinate);
            }

            int counter = 0;
            foreach (Node node in graph.Nodes)
            {
                Point point = node.Position;
                for (int j = 0; j < coordinatePlaces; j++)
                {
                    if (j == WordCoordId)
                    {
                        stringsVertices[counter][countOfVertice + WordCoordId] = "Coordinates:";
                    }
                    else if (j == CoordinateXId)
                    {
                        stringsVertices[counter][countOfVertice + CoordinateXId] = point.X.ToString();
                    }
                    else if (j == CoordinateYId)
                    {
                        stringsVertices[counter][countOfVertice + CoordinateYId] = point.Y.ToString();
                    }
                }
                counter++;
            }

            string[] str = stringsVertices.Select(x => string.Join(';', x)).ToArray();
            File.WriteAllLines(filePath, str);
        }
    }
}
