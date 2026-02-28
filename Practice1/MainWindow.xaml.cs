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

namespace Practice1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task? solutionTask;
        private CancellationTokenSource? solutionCancel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (solutionTask != null && solutionTask.Status == TaskStatus.Running)
            {
                solutionCancel!.Cancel();
                Thread.Sleep(500);
            }

            SolutionPath.Items.Clear();

            solutionCancel = new();
            solutionTask = Task.Run(SolveProblem, solutionCancel.Token);
        }

        public void AddSolutionState(MC_ProblemState state)
        {
            Dispatcher.Invoke(() =>
            {
                SolutionPath.Items.Clear();
                MC_ProblemState? parent = state;
                while (parent != null)
                {
                    SolutionPath.Items.Insert(0, parent!);
                    parent = parent.PreviousState as MC_ProblemState;
                }
            });
        }

        public void RemoveSolutionState(MC_ProblemState state)
        {
            Dispatcher.Invoke(() =>
            {
                SolutionPath.Items.Clear();
            });
        }

        public bool IsSolutionCancelled()
        {
            return solutionCancel != null && solutionCancel.IsCancellationRequested;
        }

        private class MC_SolutionTracker(MainWindow mainWindow) : AbstractSolutionTracker<MC_ProblemState, RiverTransition>
        {
            private readonly MainWindow mainWindow = mainWindow;

            public override void StatePulled(MC_ProblemState state)
            {
                if (mainWindow.IsSolutionCancelled())
                {
                    throw new TaskCanceledException();
                }
                mainWindow.AddSolutionState((MC_ProblemState)state);
                Thread.Sleep(500);
            }

            public override void StateDiscarded(MC_ProblemState state)
            {
                if (mainWindow.IsSolutionCancelled())
                {
                    throw new TaskCanceledException();
                }
                mainWindow.RemoveSolutionState((MC_ProblemState)state);
            }
        }

        private void SolveProblem()
        {
            MC_ProblemState initialState = MC_ProblemState.Initialize(3, 3);
            var solution = ProblemSolver.Solve(initialState, () => new MC_StateResolver(initialState), new MC_SolutionTracker(this));
            Dispatcher.Invoke(() =>
            {
                if (solution.Count != 0)
                    MessageBox.Show(this, "Задача решена", "Выполнение завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(this, "Задача не решена", "Выполнение завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }
}
