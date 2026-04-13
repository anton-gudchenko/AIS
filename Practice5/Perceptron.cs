using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Practice5
{
    public class Neuron
    {
        public double[] Weights { get; set; } = [];
        public double Offset { get; set; }
        public double Output { get; set; }
        public double Error { get; set; }
    }

    public class NeuronLayer(params Neuron[] neurons)
    {
        private readonly Neuron[] _neurons = neurons;

        public int Size => _neurons.Length;
        public double[] Output => [.. _neurons.Select(x => x.Output)];
        public double Error => _neurons.Select(n => n.Error).Average();

        public Neuron this[int index] => _neurons[index];
    }

    public class NeuronNetwork
    {
        public static readonly Func<double, double> SIGMOID_ACTIVATION = x => 1 / (1 + Math.Exp(-x));
        public static readonly Func<double, double> SIGMOID_DERIVATIVE = x => x * (1 - x);

        public static readonly Func<double, double> RELU_ACTIVATION = x => x < 0 ? 0 : x;
        public static readonly Func<double, double> RELU_DERIVATIVE = x => x < 0 ? 0 : 1;

        public static readonly Func<double, double> QUADRATIC_ERROR = e => Math.Pow(e, 2) / 2;

        private readonly NeuronLayer[] layers;

        public long Parameters
        {
            get
            {
                long parameters = layers[0].Size;
                for (int l = 1; l < layers.Length; l++)
                {
                    NeuronLayer prev = layers[l - 1], cur = layers[l];
                    parameters += cur.Size + (long) prev.Size * cur.Size;
                }
                return parameters;
            }
        }
        public int Input => layers[0].Size;
        public int Output => layers[^1].Size;
        public int HiddenLayers => layers.Length - 2;

        public double AverageError => layers[^1].Error;

        internal NeuronNetwork(NeuronLayer[] layers)
        {
            this.layers = layers;
        }

        public double[] RunForward(Func<double, double> activationFunction, params double[] input)
        {
            if (input.Length != layers[0].Size)
            {
                throw new ArgumentException("input length mismatch");
            }

            //инициализация входного слоя
            {
                NeuronLayer inputLayer = layers[0];
                for (int n = 0; n < inputLayer.Size; n++)
                {
                    inputLayer[n].Output = input[n];
                }
            }

            //обход всех слоёв после первого
            for (int l = 1; l < layers.Length; l++)
            {
                NeuronLayer prev = layers[l - 1];
                NeuronLayer cur = layers[l];
                
                //обход всех нейронов в слое
                for (int n = 0; n < cur.Size; n++)
                {
                    double sum = cur[n].Offset;
                    for (int i = 0; i < prev.Size; i++)
                    {
                        sum += prev[i].Output * cur[n].Weights[i];
                    }
                    cur[n].Output = activationFunction(sum);
                }
            }

            return layers[^1].Output;
        }

        public void RunBackward(Func<double, double> activationFuctionDerivative, double[] answer, bool onlyOutput = false)
        {
            if (answer.Length != layers[^1].Size)
            {
                throw new ArgumentException("output length mismatch");
            }

            //проход по выходному слою
            {
                NeuronLayer outputLayer = layers[^1];
                for (int n = 0; n < outputLayer.Size; n++)
                {
                    outputLayer[n].Error = (outputLayer[n].Output - answer[n]) * activationFuctionDerivative(outputLayer[n].Output);
                }
                if (onlyOutput)
                    return;
            }

            //обход всех скрытых слоёв
            for (int l = layers.Length - 2; l > 0; l--)
            {
                NeuronLayer next = layers[l + 1], cur = layers[l];

                //обход всех нейронов с слое
                for (int n = 0; n < cur.Size; n++)
                {
                    double errorSum = 0;
                    for (int e = 0; e < next.Size; e++)
                    {
                        errorSum += next[e].Error * next[e].Weights[n];
                    }
                    cur[n].Error = errorSum * activationFuctionDerivative(cur[n].Output);
                }
            }
        }

        public void ChangeWeights(double coefficient)
        {
            for (int l = layers.Length - 1; l > 0; l--)
            {
                NeuronLayer prev = layers[l - 1], cur = layers[l];
                for (int n = 0; n < cur.Size; n++)
                {
                    //обновление весов у нейрона
                    double[] weights = cur[n].Weights;
                    for (int w = 0; w < weights.Length; w++)
                    {
                        weights[w] -= prev[w].Output * cur[n].Error * coefficient;
                    }

                    //обновление смещения у нейрона
                    cur[n].Offset -= cur[n].Error * coefficient;
                }
            }
        }

        public void Reset(Func<double> random)
        {
            for (int l = 0; l < layers.Length; l++)
            {
                for (int n = 0; n < layers[l].Size; n++)
                {
                    Neuron neuron = layers[l][n];
                    neuron.Output = 0;
                    neuron.Error = 0;
                    neuron.Offset = random();
                    for (int w = 0; w < neuron.Weights.Length; w++)
                        neuron.Weights[w] = random();
                }
            }
        }

        public void Save(Stream stream)
        {
            BinaryWriter writer = new(new DeflateStream(stream, CompressionLevel.Optimal));

            //запись основных параметров нейросети
            writer.Write(layers.Length);
            for (int l = 0; l < layers.Length; l++)
                writer.Write(layers[l].Size);

            //запись всех параметров
            for (int l = 1; l < layers.Length; l++)
            {
                NeuronLayer layer = layers[l];
                for (int n = 0; n < layer.Size; n++)
                {
                    //запись смещения
                    writer.Write(layer[n].Offset);

                    //запись весов
                    double[] weights = layer[n].Weights;
                    for (int w = 0; w < weights.Length; w++)
                        writer.Write(weights[w]);
                }
            }

            writer.Flush();
        }

        public static NeuronNetwork Load(Stream stream)
        {
            BinaryReader reader = new(new DeflateStream(stream, CompressionMode.Decompress));

            //чтение основных парамтров нейросети
            int layersCount = reader.ReadInt32();
            if (layersCount < 2)
                throw new Exception("invalid count of layers");

            int[] layersSize = new int[layersCount];
            for (int l = 0; l < layersCount; l++)
            {
                layersSize[l] = reader.ReadInt32();
                if (layersSize[l] < 1)
                    throw new Exception($"invalid size of layer {l + 1}");
            }

            NeuronLayer[] layers = new NeuronLayer[layersCount];

            {
                //инициализация входного слоя
                Neuron[] neurons = new Neuron[layersSize[0]];
                for (int n = 0; n < neurons.Length; n++)
                    neurons[n] = new Neuron();
            }

            //чтение всех параметров
            for (int l = 1; l < layersCount; l++)
            {
                Neuron[] neurons = new Neuron[layersSize[l]];
                for (int n = 0; n < neurons.Length; n++)
                {
                    //чтение смещения
                    double offset = reader.ReadDouble();

                    //чтение весов
                    double[] weights = new double[layersSize[l - 1]];
                    for (int w = 0; w < weights.Length; w++)
                        weights[w] = reader.ReadDouble();

                    //сохранение нейрона
                    neurons[n] = new Neuron()
                    {
                        Offset = offset,
                        Weights = weights
                    };
                }

                //сохранение слоя
                layers[l] = new NeuronLayer(neurons);
            }

            throw new Exception();
        }

        public static NeuronNetwork CreateNew(Func<double> weightRandom, Func<double> offsetRandom, params int[] layers)
        {
            if (layers.Length < 2)
                throw new ArgumentException("too few layers");

            NeuronLayer[] neuronLayers = new NeuronLayer[layers.Length];
            for (int l = 0; l < layers.Length; l++)
            {
                if (layers[l] < 1)
                    throw new ArgumentException($"invalid count of neurons in layer {l + 1}");
                Neuron[] neurons = new Neuron[layers[l]];
                for (int n = 0; n < neurons.Length; n++)
                {
                    neurons[n] = new Neuron();
                    if (l > 0)
                    {
                        double[] weights = new double[layers[l - 1]];
                        for (int w = 0; w < weights.Length; w++)
                            weights[w] = weightRandom();
                        neurons[n].Weights = weights;
                        neurons[n].Offset = offsetRandom();
                    }
                }
                neuronLayers[l] = new NeuronLayer(neurons);
            }

            return new NeuronNetwork(neuronLayers);
        }
    }

    public interface ITest
    {
        double[] Test { get; }
        double[] Answer { get; }
    }

    public class Perceptron(NeuronNetwork network)
    {
        public NeuronNetwork Network { get; } = network;

        public double Train(List<ITest> pack, double coefficient)
        {
            for (int i = 0; i < pack.Count; i++)
            {
                ITest test = pack[i];
                double[] output = Network.RunForward(NeuronNetwork.SIGMOID_ACTIVATION, test.Test);
                Network.RunBackward(NeuronNetwork.SIGMOID_DERIVATIVE, test.Answer);
                Network.ChangeWeights(coefficient);
            }
            return coefficient;
        }

        public double Test(List<ITest> pack)
        {
            double[] errors = new double[pack.Count];
            for (int i = 0; i < pack.Count; i++)
            {
                ITest test = pack[i];
                Network.RunForward(NeuronNetwork.SIGMOID_ACTIVATION, test.Test);
                Network.RunBackward(NeuronNetwork.SIGMOID_DERIVATIVE, test.Answer, true);
                errors[i] = Network.AverageError;
            }
            return errors.Average();
        }

        public int Percept(params double[] input)
        {
            double[] result = Network.RunForward(NeuronNetwork.SIGMOID_ACTIVATION, input);

            int max = 0;
            for (int i = 1; i < result.Length; i++)
                if (result[i] > result[max])
                    max = i;
            return max;
        }
    }

    public static class ImageGrid
    {
        public static readonly SolidColorBrush CLEAR_CELL = new(Colors.White);
        public static readonly SolidColorBrush FILLED_CELL = new(Colors.Black);

        public static UniformGrid CreateImageGrid(int initValue = 0, bool interractive = true, MouseButtonEventHandler? customHandler = null)
        {
            UniformGrid grid = new()
            {
                Columns = 3,
                Rows = 3
            };

            Thickness thickness = new(1);
            SolidColorBrush borderBrush = new(Colors.Black);

            for (int i = 0, mask = 1 << 8; i < 9; i++, mask >>= 1)
            {
                Label cell = new()
                {
                    Background = (initValue & mask) != 0 ? FILLED_CELL : CLEAR_CELL
                };

                if (interractive)
                    cell.MouseDown += CellClick;
                if (customHandler != null)
                    cell.MouseDown += customHandler;

                grid.Children.Add(new Border()
                {
                    Child = cell,
                    Margin = thickness,
                    BorderThickness = thickness,
                    BorderBrush = borderBrush
                });
            }

            return grid;
        }

        public static int GetValue(UniformGrid imageGrid)
        {
            int value = 0;
            for (int i = 0, mask = 1 << 8; i < 9; i++, mask >>= 1)
            {
                if (imageGrid.Children[i] is Border border && border.Child is Label cell && cell.Background == FILLED_CELL)
                    value |= mask;
            }
            return value;
        }

        public static double[] GetValueAsInput(UniformGrid imageGrid)
        {
            double[] input = new double[9];
            for (int i = 0, mask = 1 << 8; i < 9; i++, mask >>= 1)
            {
                if (imageGrid.Children[i] is Border border && border.Child is Label cell && cell.Background == FILLED_CELL)
                    input[i] = 1;
            }
            return input;
        }

        public static void SetValue(UniformGrid imageGrid, int value)
        {
            for (int i = 0, mask = 1 << 8; i < 9; i++, mask >>= 1)
            {
                if (imageGrid.Children[i] is Border border && border.Child is Label cell)
                    cell.Background = (value & mask) != 0 ? FILLED_CELL : CLEAR_CELL;
            }
        }

        public static void CellClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Label label)
                return;
            if (label.Background != FILLED_CELL)
                label.Background = FILLED_CELL;
            else
                label.Background = CLEAR_CELL;
        }

        public static double[] GetAsInput(int value, double[]? arr = null)
        {
            double[] input = arr ?? new double[9];
            for (int i = 0, mask = 1 << 8; i < 9; i++, mask >>= 1)
                input[i] = (value & mask) != 0 ? 1 : 0;
            return input;
        }
    }
}
