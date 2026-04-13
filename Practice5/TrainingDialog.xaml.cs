using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Practice5
{
    /// <summary>
    /// Логика взаимодействия для TrainingDialog.xaml
    /// </summary>
    public partial class TrainingDialog : Window
    {
        private const int _total = 1 << 16;
        private const double _errorLevel = 0.0001;

        public NeuronNetwork? Network { get; set; }
        public int[]? Images { get; set; }
        public double? Coefficient { get; set; }

        private double _error;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public TrainingDialog()
        {
            InitializeComponent();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartTraining()
        {
            if (Network == null || Images == null || Coefficient == null)
                throw new Exception("not all arguments are provided");

            Task.Run(() =>
            {
                //создается массив во всеми возможнами значениями, кроме верных
                List<int> allVariants = new(_total - 5);
                for (int i = 0; i < _total; i++)
                {
                    if (!Images.Contains(i))
                        allVariants.Add(i);
                }

                Random random = new();

                //прогоняются все эпохи, пока не будет достигнут нужный уровень ошибок
                _error = double.MaxValue;
                double maxError = 0, lastError = double.MaxValue;
                double coefficient = Coefficient.Value;
                Perceptron perceptron = new(Network);
                LinkedList<double> errors = new();
                while (_error > _errorLevel && allVariants.Count >= 10)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                        break;
                    var epoch = CreateEpoch(random, allVariants);
                    coefficient = perceptron.Train(epoch, coefficient);
                    _error = perceptron.Test(CreateEpoch(random, allVariants));
                    if (_error > maxError)
                        maxError = _error;
                    if (_error > lastError)
                        coefficient *= 0.5;
                    lastError = _error;
                    if (coefficient < 1e-10)
                        break;
                }
                Dispatcher.Invoke(() =>
                {
                    this.Title = "Обучение завершено";
                    CancelButton.Content = "Закрыть";
                });
            }, _cancellationTokenSource.Token);
            Task.Run(() =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Dispatcher.Invoke(() => Error.Content = $"Коэффициент ошибки: {_error}");
                    Thread.Sleep(50);
                }
            }, _cancellationTokenSource.Token);
        }

        class SimpleTest : ITest
        {
            public required double[] AnswerBuffer { get; set; }
            public required int IntAnswer { get; set; }
            public double[] Answer
            {
                get
                {
                    Array.Fill(AnswerBuffer, 0D);
                    AnswerBuffer[IntAnswer - 1] = 1D;
                    return AnswerBuffer;
                }
            }

            public required double[] TestBuffer { get; set; }
            public required int IntTest { get; set; }
            public double[] Test
            {
                get
                {
                    ImageGrid.GetAsInput(IntTest, TestBuffer);
                    return TestBuffer;
                }
            }
        }

        private List<ITest> CreateEpoch(Random random, List<int> allVariants)
        {
            const int EPOCH_SIZE = 5;
            double[] testBuffer = new double[9];
            double[] answerBuffer = new double[5];
            List<ITest> epoch = new(EPOCH_SIZE);

            //добавление неизвестных образов
            for (int i = 0; i < EPOCH_SIZE - 5; i++)
            {
                int next = random.Next(allVariants.Count);
                epoch.Add(new SimpleTest()
                {
                    AnswerBuffer = answerBuffer,
                    IntAnswer = 0,
                    TestBuffer = testBuffer,
                    IntTest = allVariants[next],
                });
            }

            //добавление известных образов
            epoch.Add(new SimpleTest()
            {
                AnswerBuffer = answerBuffer,
                IntAnswer = 1,
                TestBuffer = testBuffer,
                IntTest = Images[0]
            });
            epoch.Add(new SimpleTest()
            {
                AnswerBuffer = answerBuffer,
                IntAnswer = 2,
                TestBuffer = testBuffer,
                IntTest = Images[1]
            });
            epoch.Add(new SimpleTest()
            {
                AnswerBuffer = answerBuffer,
                IntAnswer = 3,
                TestBuffer = testBuffer,
                IntTest = Images[2]
            });
            epoch.Add(new SimpleTest()
            {
                AnswerBuffer = answerBuffer,
                IntAnswer = 4,
                TestBuffer = testBuffer,
                IntTest = Images[3]
            });
            epoch.Add(new SimpleTest()
            {
                AnswerBuffer = answerBuffer,
                IntAnswer = 5,
                TestBuffer = testBuffer,
                IntTest = Images[4]
            });
            return epoch;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            Error.Content = $"Коэффициент ошибки: {_error}";
            DialogResult = true;
        }
    }
}
