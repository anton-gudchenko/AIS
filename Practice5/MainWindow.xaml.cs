using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Practice5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// </summary>
    public partial class MainWindow : Window
    {
        private NeuronNetwork _network;
        private readonly UniformGrid[] _trainingImages = new UniformGrid[5];
        private readonly UniformGrid _controlGrid;

        public NeuronNetwork Network {
            get => _network;
            set {
                _network = value;
                NetworkStats.Content = $"Свойства нейросети: {value.Parameters} параметров, {value.Input} входных нейронов, {value.Output} выходных нейронов, {value.HiddenLayers} скрытых слоёв";
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrainingGrid();
            _controlGrid = InitializeControlGrid();
            Random random = new();
            Network = NeuronNetwork.CreateNew(() => random.NextDouble() * 0.5, () => 0, 9, 15, 5);
        }

        private void InitializeTrainingGrid()
        {
            Label label;

            //перый образ
            label = CreateTrainingButton(0b100010111, 0);
            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 1);
            TrainingGrid.Children.Add(label);

            //второй образ
            label = CreateTrainingButton(0b101001111, 1);
            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 2);
            TrainingGrid.Children.Add(label);

            //третий образ
            label = CreateTrainingButton(0b111001011, 2);
            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 3);
            TrainingGrid.Children.Add(label);

            //четвёрный образ
            label = CreateTrainingButton(0b110010110, 3);
            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 4);
            TrainingGrid.Children.Add(label);

            //пятый образ
            label = CreateTrainingButton(0b110010100, 4);
            Grid.SetColumn(label, 1);
            Grid.SetRow(label, 5);
            TrainingGrid.Children.Add(label);
        }

        private Label CreateTrainingButton(int imageValue, int index)
        {
            UniformGrid imageGrid = ImageGrid.CreateImageGrid(imageValue, false);
            imageGrid.Width = 36;
            imageGrid.Height = 36;
            imageGrid.Focusable = false;
            _trainingImages[index] = imageGrid;
            Label label = new()
            {
                Content = imageGrid,
                Margin = new Thickness(2),
                Padding = new Thickness(2),
            };
            label.MouseDown += (sender, evt) =>
            {
                ImageGridDialog dialog = new(imageValue);
                if (dialog.ShowDialog() == true)
                    ImageGrid.SetValue(imageGrid, dialog.Value);
            };

            return label;
        }

        private UniformGrid InitializeControlGrid()
        {
            UniformGrid imageGrid = ImageGrid.CreateImageGrid(customHandler: ControlClick);
            imageGrid.Width = 300;
            imageGrid.Height = 300;

            MainPanel.Children.Insert(1, imageGrid);

            return imageGrid;
        }

        private void ControlClick(object sender, MouseButtonEventArgs e)
        {
            Perceptron perceptron = new(_network);
            double[] input = ImageGrid.GetValueAsInput(_controlGrid);

            int result = perceptron.Percept(input);
            Result.Content = $"Ответ перцептрона: образ {result + 1}";
        }

        private void NetworkTypeButton(object sender, RoutedEventArgs e)
        {
            if (FilePanel == null)
                return;

            if (sender is not RadioButton button)
                return;
            switch (button.Tag.ToString())
            {
                case "default":
                    FilePanel.Visibility = Visibility.Collapsed;
                    CustomPanel.Visibility = Visibility.Collapsed;
                    break;
                case "custom":
                    FilePanel.Visibility= Visibility.Collapsed;
                    CustomPanel.Visibility = Visibility.Visible;
                    break;
                case "import":
                    FilePanel.Visibility = Visibility.Visible;
                    CustomPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Title = "Выберете файл с нйросетью",
                Multiselect = false,
                CheckFileExists = true,
                Filter = "Файлы нейросети (*.nn)|*.nn|Все файлы (*.*)|*.*"
            };
            if (ofd.ShowDialog() != true)
                return;

            using Stream fileStream = ofd.OpenFile();
            try
            {
                Network = NeuronNetwork.Load(fileStream);
                NetworkPath.Text = ofd.FileName[(ofd.FileName.LastIndexOf('\\') + 1)..];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка чтения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TrainingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TraningCoefficient.Text, out double coefficient))
            {
                MessageBox.Show("Неверный формат записи коэффициента обучения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            TrainingDialog dialog = new()
            {
                Network = Network,
                Images = [.. _trainingImages.Select(ig => ImageGrid.GetValue(ig))],
                Coefficient = coefficient
            };
            dialog.StartTraining();
            dialog.ShowDialog();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Random random = new();
            _network.Reset(random.NextDouble);
            MessageBox.Show("Все веса и смещения сброшены", "Уведомление", MessageBoxButton.OK);
        }
    }
}
