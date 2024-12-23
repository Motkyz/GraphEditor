using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GraphEditor.GraphElements
{
    public static class GraphDrawer
    {
        public static void DrawGraph(Graph graph, Canvas canva)
        {
            canva.Children.Clear();

            foreach (Node node in graph.Nodes)
            {
                Canvas.SetZIndex(node.Ellipse, 3);
                Canvas.SetLeft(node.Ellipse, node.Position.X - Node.Radius);
                Canvas.SetTop(node.Ellipse, node.Position.Y - Node.Radius);
                canva.Children.Add(node.Ellipse);

                Canvas.SetZIndex(node.TextBlockDistance, 3);
                Canvas.SetLeft(node.TextBlockDistance, node.Position.X);
                Canvas.SetTop(node.TextBlockDistance, node.Position.Y - Node.Radius * 2);
                canva.Children.Add(node.TextBlockDistance);
            }

            foreach (Edge edge in graph.Edges)
            {
                edge.UpdateLine();
                Canvas.SetZIndex(edge.Line, 0);
                canva.Children.Add(edge.Line);

                Canvas.SetLeft(edge.TextBlock, (edge.FirstNode.Position.X + edge.SecondNode.Position.X) / 2 - 20);
                Canvas.SetTop(edge.TextBlock, (edge.FirstNode.Position.Y + edge.SecondNode.Position.Y) / 2 - 20);
                canva.Children.Add(edge.TextBlock);
            }
        }


    }
}
