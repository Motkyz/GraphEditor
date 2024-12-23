using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphEditor.GraphElements
{
    public class Node : IComparable<Node>
    {
        public static readonly Brush DefaultColor = Brushes.White;
        public static readonly Brush SelectColor = Brushes.LightBlue; //подсветка узла, как выбранного
        public static readonly Brush ActiveColorLvl1 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E90FF")); //подсветка узла, как пути
        public static readonly Brush ActiveColorLvl2 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A6D2FF")); //подсветка узла, как пути
        public static readonly int Radius = 25;

        public int Id;
        public Point Position;
        public Border Ellipse;
        public TextBlock TextBlock;
        public TextBlock TextBlockDistance;
        public Brush Color;

        public Node(int id, double x, double y)
        {
            Color = DefaultColor;
            Id = id;
            Position = new Point(x, y);

            Ellipse = new Border()
            {
                Width = 50,
                Height = 50,
                CornerRadius = new CornerRadius(50),
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Black,
                Background = Color,
            };

            TextBlock = new TextBlock()
            {
                Text = (Id).ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = 18,
            };

            TextBlockDistance = new TextBlock()
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = 18,
                Foreground = ActiveColorLvl1
            };

            Ellipse.Child = TextBlock;
        }

        public static Node FindNode(int id, List<Node> nodes)
        {
            foreach (Node n in nodes)
            {
                if (n.Id == id)
                {
                    return n;
                }
            }

            return null;
        }

        public int CompareTo(Node node)
        {
            return Id.CompareTo(node.Id);
        }

        public override string ToString() 
        { 
            return Id.ToString(); 
        }
    }
}
