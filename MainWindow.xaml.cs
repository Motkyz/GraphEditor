using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphEditor.GraphElements;
using Microsoft.Win32;

namespace GraphEditor
{
    public partial class MainWindow : Window
    {
        CancellationTokenSource? _cancellationTokenSource;

        public string? FilePath;

        private Graph _graph = new Graph();
        private List<string> logs = new List<string>();

        private Node _firstSelectedNode;
        private Node _secondSelectedNode;
        private Edge _edgeToChange;

        private bool _AddingNodeMode;
        private bool _AddingEdgeMode;
        private bool _MovingNodeMode;
        private bool _RemovingNodeMode;
        private bool _RemovingEdgeMode;
        private bool _ChangingValue;

        private bool _InMoving;
        private bool _SelectingTwoNodes;
        private bool _SecondNodeSelecting;
        private bool _FirstNodeSelecting;

        private bool _InExecuting;

        private string _SelectedAlgorithm;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            logs.Clear();
            logTxt.Text = string.Empty;
            _SelectedAlgorithm = Algorithms.Text;
            ResetGraph();

            try
            {
                switch (_SelectedAlgorithm)
                {
                    case "Обход графа в ширину":
                        if (_firstSelectedNode == null)
                        {
                            ResetModes();
                            LogUpd("Для начала выберите начальный узел:");
                            _FirstNodeSelecting = true;
                        }
                        else
                        {
                            _InExecuting = true;
                            var widthTraveler = new Traveler(_graph, LogUpd, HighlightNode, HighlightEdge, _cancellationTokenSource);
                            await widthTraveler.WidthTravel(_firstSelectedNode);
                            _firstSelectedNode = null;
                            ResetModes();
                            _InExecuting = false;
                        }
                        break;
                    case "Обход графа в глубину":
                        if (_firstSelectedNode == null)
                        {
                            ResetModes();
                            LogUpd("Для начала выберите начальный узел:");
                            _FirstNodeSelecting = true;
                        }
                        else
                        {
                            _InExecuting = true;
                            var depthTraveler = new Traveler(_graph, LogUpd, HighlightNode, HighlightEdge, _cancellationTokenSource);
                            await depthTraveler.DepthTravel(_firstSelectedNode);
                            _firstSelectedNode = null;
                            ResetModes();
                            _InExecuting = false;
                        }
                        break;
                    case "Поиск максимального потока":
                        if (_firstSelectedNode == null || _secondSelectedNode == null)
                        {
                            ResetModes();
                            LogUpd("Для начала выберите два узла:");
                            LogUpd("Выберите начальный узел:");
                            SelectTwoNodes();
                        }
                        else
                        {
                            _InExecuting = true;
                            var maxFlowSearcher = new MaxFlowSearcher(_graph, _firstSelectedNode, _secondSelectedNode, LogUpd, HighlightNode, HighlightEdge, _cancellationTokenSource);
                            await maxFlowSearcher.FordFulkerson();
                            _firstSelectedNode = null;
                            _secondSelectedNode = null;
                            ResetModes();
                            _InExecuting = false;
                        }
                        break;
                    case "Построение остовного дерева":
                        if (_firstSelectedNode == null)
                        {
                            ResetModes();
                            LogUpd("Для начала выберите начальный узел:");
                            _FirstNodeSelecting = true;
                        }
                        else
                        {
                            _InExecuting = true;
                            var spanningTreeBuilder = new SpanningTreeBuilder(_graph, LogUpd, HighlightNode, HighlightEdge, _cancellationTokenSource);
                            await spanningTreeBuilder.PrimAlgorithm(_firstSelectedNode);
                            GraphDrawer.DrawGraph(_graph, canva);
                            _firstSelectedNode = null;
                            ResetModes();
                            _InExecuting = false;
                        }
                        break;
                    case "Поиск кратчайшего пути":
                        if (_firstSelectedNode == null || _secondSelectedNode == null)
                        {
                            ResetModes();
                            LogUpd("Для начала выберите два узла:");
                            LogUpd("Выберите начальный узел:");
                            SelectTwoNodes();
                        }
                        else
                        {
                            _InExecuting = true;
                            var shortestPathSearcher = new ShortestPathSearcher(_graph, _firstSelectedNode, _secondSelectedNode, LogUpd, HighlightNode, HighlightEdge, _cancellationTokenSource);
                            await shortestPathSearcher.Initialize();
                            _firstSelectedNode = null;
                            _secondSelectedNode = null;
                            ResetModes();
                            _InExecuting = false;
                        }
                        break;
                    default:
                        MessageBox.Show("Для начала выберите действие");
                        break;
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка, попробуйте снова\n" + ex);
            }
        }

        private void SelectTwoNodes()
        {
            _FirstNodeSelecting = true;
            _SelectingTwoNodes = true;
        }

        private void Canvas_MouseDown_ChooseNode(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(canva);

            var chosenNode = _graph.Nodes.FirstOrDefault(n =>
                mousePosition.X >= n.Position.X - 20 && mousePosition.X <= n.Position.X + 20 &&
                mousePosition.Y >= n.Position.Y - 20 && mousePosition.Y <= n.Position.Y + 20);

            if (chosenNode != null) 
            {
                if (_SecondNodeSelecting)
                {
                    _secondSelectedNode = chosenNode;
                    LogUpd($"Конечный узел - [{_secondSelectedNode}]");
                    StartAlgorithm_Click(sender, e);
                }
                else
                {
                    _firstSelectedNode = chosenNode;
                    LogUpd($"Начальный узел - [{_firstSelectedNode}]");
                    if (_SelectingTwoNodes) 
                    {
                        LogUpd("Выберите конечный узел:");
                        _SecondNodeSelecting = true;
                    }
                    else 
                    { 
                        StartAlgorithm_Click(sender, e); 
                    }
                }
            }
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_InExecuting)
            {
                if (_AddingNodeMode)
                {
                    MouseDown_AddNode(sender, e);
                }
                else if (_RemovingNodeMode)
                {
                    MouseDown_RemoveNode(sender, e);
                }
                else if (_MovingNodeMode)
                {
                    MouseDown_MoveNode(sender, e);
                }
                else if (_AddingEdgeMode)
                {
                    MouseDown_AddEdge(sender, e);
                }
                else if (_RemovingEdgeMode)
                {
                    MouseDown_RemoveEdge(sender, e);
                }
                else if (_ChangingValue)
                {
                    MouseDown_ChangeValue(sender, e);
                }
                else if (_SecondNodeSelecting)
                {
                    Canvas_MouseDown_ChooseNode(sender, e);
                }
                else if (_FirstNodeSelecting)
                {
                    Canvas_MouseDown_ChooseNode(sender, e);
                }
            }
        }

        //Блок добавления нового узла
        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();
            _AddingNodeMode = true;
            AddNode.IsChecked = true;
        }

        private void MouseDown_AddNode(object sender, MouseButtonEventArgs e)
        {
            Node node = new Node(_graph.count, e.GetPosition(canva).X, e.GetPosition(canva).Y);
            _graph.AddNode(node);
            GraphDrawer.DrawGraph(_graph, canva);
        }

        //Блок удаления узла
        private void RemoveNode_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();
            _RemovingNodeMode = true;
            RemoveNode.IsChecked = true;
        }

        private void MouseDown_RemoveNode(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(canva);

            var nodeToRemove = _graph.Nodes.FirstOrDefault(n =>
                mousePosition.X >= n.Position.X - 20 && mousePosition.X <= n.Position.X + 20 &&
                mousePosition.Y >= n.Position.Y - 20 && mousePosition.Y <= n.Position.Y + 20);

            if (nodeToRemove != null)
            {
                _graph.RemoveNode(nodeToRemove);
                GraphDrawer.DrawGraph(_graph, canva);
            }
        }

        //Блок перемещения узла
        private void MoveNode_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();
            _MovingNodeMode = true;
            MoveNode.IsChecked = true;
        }

        private void MouseDown_MoveNode(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(canva);

            //Находим узел
            _firstSelectedNode = _graph.Nodes.FirstOrDefault(n =>
                position.X >= n.Position.X - 20 && position.X <= n.Position.X + 20 &&
                position.Y >= n.Position.Y - 20 && position.Y <= n.Position.Y + 20)!;

            if (_firstSelectedNode != null)
            {
                _firstSelectedNode.Color = Node.SelectColor;
                _InMoving = true;
                Canvas.SetZIndex(_firstSelectedNode.Ellipse, 10);
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_MovingNodeMode) return;
            if (_InMoving)
            {
                _firstSelectedNode.Position.X = e.GetPosition(canva).X;
                _firstSelectedNode.Position.Y = e.GetPosition(canva).Y;

                GraphDrawer.DrawGraph(_graph, canva);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_MovingNodeMode) return;
            if (_InMoving)
            {
                _InMoving = false;
                _firstSelectedNode.Color = Node.DefaultColor;
                _firstSelectedNode.Ellipse.Background = _firstSelectedNode.Color;
                _firstSelectedNode = null;
            }
        }

        //Блок добавления новой связи
        private void AddEdge_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();
            _AddingEdgeMode = true;
            AddEdge.IsChecked = true;
        }

        private void MouseDown_AddEdge(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(canva);

            // Находим узел
            var selectedNode = _graph.Nodes.FirstOrDefault(n =>
                mousePosition.X >= n.Position.X - 20 && mousePosition.X <= n.Position.X + 20 &&
                mousePosition.Y >= n.Position.Y - 20 && mousePosition.Y <= n.Position.Y + 20);

            if (selectedNode == null)
            {
                return;
            }

            if (_firstSelectedNode == null)
            {
                _firstSelectedNode = selectedNode;
            }
            else
            {
                if (_firstSelectedNode == selectedNode)
                {
                    MessageBox.Show("Вы выбрали тот же узел");
                    return;
                }

                string valueInput = Microsoft.VisualBasic.Interaction.InputBox(
                    "Введите значение для ребра:",
                    "Добавление ребра",
                    "0");

                if (!int.TryParse(valueInput, out int value) || value <= 0)
                {
                    MessageBox.Show("Некорректное значение");
                    return;
                }

                // Добавляем ребро с заданным весом
                _graph.Edges.Add(new Edge(_firstSelectedNode, selectedNode, value));

                GraphDrawer.DrawGraph(_graph, canva);

                _firstSelectedNode = null;
            }
        }

        //Блок удаления связи
        private void RemoveEdge_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();
            _RemovingEdgeMode = true;
            RemoveEdge.IsChecked = true;
        }

        private void MouseDown_RemoveEdge(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(canva);

            //Находим ребро
            var edgeToRemove = _graph.Edges.FirstOrDefault(edge =>
            {
                var x1 = edge.FirstNode.Position.X;
                var y1 = edge.FirstNode.Position.Y;
                var x2 = edge.SecondNode.Position.X;
                var y2 = edge.SecondNode.Position.Y;

                double distance = DistanceFromPointToLine(position.X, position.Y, x1, y1, x2, y2);
                return distance <= 5;
            });

            if (edgeToRemove != null)
            {
                _graph.RemoveEdge(edgeToRemove);
                GraphDrawer.DrawGraph(_graph, canva);
            }
        }

        //Блок изменения значения для связи
        private void ChangeEdgeValue_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();
            _ChangingValue = true;
            ChangeEdgeValue.IsChecked = true;
        }

        private void MouseDown_ChangeValue(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(canva);

            //Ближайшее ребро
            _edgeToChange = _graph.Edges.FirstOrDefault(edge =>
            {
                var x1 = edge.FirstNode.Position.X;
                var y1 = edge.FirstNode.Position.Y;
                var x2 = edge.SecondNode.Position.X;
                var y2 = edge.SecondNode.Position.Y;

                double distance = DistanceFromPointToLine(position.X, position.Y, x1, y1, x2, y2);
                return distance <= 5;
            })!;

            if (_edgeToChange != null)
            {
                _edgeToChange.Value = "";

                GraphDrawer.DrawGraph(_graph, canva);

                TextBox textBox = new TextBox();
                textBox.MinWidth = 30;
                textBox.FontSize = _edgeToChange.TextBlock.FontSize;
                Canvas.SetLeft(textBox, (_edgeToChange.FirstNode.Position.X + _edgeToChange.SecondNode.Position.X) / 2 - 20);
                Canvas.SetTop(textBox, (_edgeToChange.FirstNode.Position.Y + _edgeToChange.SecondNode.Position.Y) / 2 - 20);
                canva.Children.Add(textBox);
                textBox.KeyDown += TextBox_KeyDown;

                MessageBox.Show("После изменения значения нажмите Enter");
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(((TextBox)sender).Text, out int value) && value > 0)
                {
                    _edgeToChange.Value = value.ToString();
                    GraphDrawer.DrawGraph(_graph, canva);
                    _edgeToChange = null;
                }
                else
                {
                    MessageBox.Show("Было введено некорректное значение");
                }
            }
        }

        private void ResetModes()
        {
            _AddingNodeMode = false;
            _AddingEdgeMode = false;
            _MovingNodeMode = false;
            _RemovingNodeMode = false;
            _RemovingEdgeMode = false;
            _ChangingValue = false;
            _FirstNodeSelecting = false;
            _SecondNodeSelecting = false;
            _SelectingTwoNodes = false;

            AddNode.IsChecked = false;
            RemoveNode.IsChecked = false;
            AddEdge.IsChecked = false;
            RemoveEdge.IsChecked = false;
            MoveNode.IsChecked = false;
            ChangeEdgeValue.IsChecked = false;
        }

        private void ResetGraph()
        {
            foreach (Node n in _graph.Nodes)
            {
                HighlightNode(n, Node.DefaultColor);
                n.TextBlockDistance.Text = "";
            }
            foreach (Edge e in _graph.Edges)
            {
                e.Value = e.Value.Split('/')[0];
                e.TextBlock.Foreground = Edge.DefaultColor;
                HighlightEdge(e, Edge.DefaultColor);
            }
        }

        private void SaveGraph_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();

            SaveFileDialog openFileDialog = new()
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Сохранить граф"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _cancellationTokenSource?.Cancel();
                try
                {
                    FilePath = openFileDialog.FileName;
                    FileHandler.WriteToFile(_graph, FilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении графа: {ex.Message}");
                }
            }
        }

        private void LoadGraph_Click(object sender, RoutedEventArgs e)
        {
            ResetModes();

            OpenFileDialog openFileDialog = new ()
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Загрузить граф"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _cancellationTokenSource?.Cancel();
                try
                {
                    FilePath = openFileDialog.FileName;
                    _graph = FileHandler.Read(FilePath);
                    GraphDrawer.DrawGraph(_graph, canva);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при чтении графа: {ex.Message}");
                }
            }
        }

        private void ClearGraph_Click(object sender, RoutedEventArgs e)
        {
            Cansel_Click(sender, e);
            _graph = new Graph();
            GraphDrawer.DrawGraph(_graph, canva);
        }

        private void HighlightNode(Node node, Brush color)
        {
            node.Color = color;
            node.Ellipse.Background = color;
        }

        private void HighlightEdge(Edge edge, Brush color)
        {
            edge.Color = color;
            GraphDrawer.DrawGraph(_graph, canva);
        }

        private void LogUpd(string comment)
        {
            logs.Add($"{comment}");
            logTxt.Text = string.Join("\n", logs);
        }

        private double DistanceFromPointToLine(double px, double py, double x1, double y1, double x2, double y2)
        {
            double a = px - x1;
            double b = py - y1;
            double c = x2 - x1;
            double d = y2 - y1;

            double dot = a * c + b * d;
            double lenSq = c * c + d * d;
            double param = lenSq != 0 ? dot / lenSq : -1;

            double xx, yy;

            if (param < 0)
            {
                xx = x1;
                yy = y1;
            }
            else if (param > 1)
            {
                xx = x2;
                yy = y2;
            }
            else
            {
                xx = x1 + param * c;
                yy = y1 + param * d;
            }

            double dx = px - xx;
            double dy = py - yy;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void Cansel_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _InExecuting = false;
            _firstSelectedNode = null;
            _secondSelectedNode = null;
            ResetModes();
        }
    }
}