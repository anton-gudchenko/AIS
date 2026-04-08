using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Practice2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            string text = InitialBlocksText.Text;
            if (!int.TryParse(text, out int initialBlocks))
            {
                MessageBox.Show(this, "Неверная записть числа");
                return;
            }
            if (initialBlocks < 0)
            {
                MessageBox.Show(this, "Число не должно быть отрицательным");
                return;
            }
            if (initialBlocks == 0 || initialBlocks > 15)
            {
                MessageBox.Show(this, "Количество начальных блоков должно быть от 1 до 15 включительно");
                return;
            }

            InitialBlocksText.IsEnabled = false;
            SolveButton.Content = "Решение...";
            SolveButton.IsEnabled = false;
            Steps.Items.Clear();

            var moves = TowerOfHanoi.LoopSolve(initialBlocks);
            int[] pivots = [initialBlocks, 0, 0];
            Steps.Items.Add($"{pivots[0]}; {pivots[1]}; {pivots[2]}");
            moves.ForEach(move => {
                --pivots[move.FromPivot];
                ++pivots[move.ToPivot];
                Steps.Items.Add($"{pivots[0]}; {pivots[1]}; {pivots[2]} ({move})");
            });

            InitialBlocksText.IsEnabled = true;
            SolveButton.Content = "Решить";
            SolveButton.IsEnabled = true;
            StepsCount.Text = "Шагов в решении: " + moves.Count;
        }

        private class Pivot
        {
            private readonly List<Rectangle> _blocks = [];

            public int Count => _blocks.Count;

            public void Push(Rectangle block)
            {
                _blocks.Add(block);
            }

            public Rectangle Pop()
            {
                Rectangle last = _blocks[^1];
                _blocks.RemoveAt(_blocks.Count - 1);
                return last;
            }
        }
    }
}
