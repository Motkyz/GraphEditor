using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.GraphElements
{
    public class Edge
    {
        public static readonly Brush DefaultColor = Brushes.Black;
        public static readonly Brush SelectColor = Brushes.Red; //подсветка связи, как выбранной
        public static readonly Brush SelectColorLvl2 = Brushes.Yellow;
        public static readonly Brush ActiveColorLvl1 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E90FF")); //подсветка связи, как пути
        public static readonly Brush UnActiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f2f2f2"));

        public Node FirstNode;
        public Node SecondNode;

        public TextBlock TextBlock;
        public Line Line;
        public Brush Color;

        public string Value;

        public Edge(Node firstNode, Node secondNode, int value) 
        {
            Color = DefaultColor;
            FirstNode = firstNode;
            SecondNode = secondNode;
            Value = value.ToString();

            Line = new Line() 
            {
                X1 = FirstNode.Position.X,
                Y1 = FirstNode.Position.Y,
                X2 = SecondNode.Position.X,
                Y2 = SecondNode.Position.Y,
                Stroke = Color,
                StrokeThickness = 3
            };

            TextBlock = new TextBlock()
            {
                Text = value.ToString(),
                FontSize = 16,
                Foreground = Brushes.Black,
            };
        }

        public void UpdateLine() 
        {
            Line.X1 = FirstNode.Position.X;
            Line.Y1 = FirstNode.Position.Y;
            Line.X2 = SecondNode.Position.X;
            Line.Y2 = SecondNode.Position.Y;
            Line.Stroke = Color;

            TextBlock.Text = Value;
        }

        public static bool EdgeExist(List<Edge> edges, Edge edge)
        {
            foreach (Edge e in edges)
            {
                if ((e.FirstNode.Id == edge.FirstNode.Id && e.SecondNode.Id == edge.FirstNode.Id)
                    || (e.FirstNode.Id == edge.SecondNode.Id && e.SecondNode.Id == edge.FirstNode.Id))
                {
                    return true;
                }
            }
            return false;
        }

        public static Edge FindEdge(Node vertice1, Node vertice2, List<Edge> edges)
        {
            foreach (Edge e in edges)
            {
                if ((e.FirstNode.Id == vertice1.Id && e.SecondNode.Id == vertice2.Id)
                    || (e.SecondNode.Id == vertice1.Id && e.FirstNode.Id == vertice2.Id))
                {
                    return e;
                }
            }
            return null;
        }

        public static Edge FindEdge(int id1, int id2, List<Edge> edges)
        {
            foreach (Edge e in edges)
            {
                if ((e.FirstNode.Id == id1 && e.SecondNode.Id == id2)
                    || (e.SecondNode.Id == id1 && e.FirstNode.Id == id2))
                {
                    return e;
                }
            }
            return null;
        }
    }
}
