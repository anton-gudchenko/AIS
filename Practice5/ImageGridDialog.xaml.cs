using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Practice5
{
    /// <summary>
    /// Логика взаимодействия для ImageGridDialog.xaml
    /// </summary>
    public partial class ImageGridDialog : Window
    {
        private readonly UniformGrid _imageGrid;

        public int Value => ImageGrid.GetValue(_imageGrid);

        public ImageGridDialog() : this(0)
        {

        }

        public ImageGridDialog(int initValue)
        {
            InitializeComponent();
            _imageGrid = ImageGrid.CreateImageGrid(initValue);
            _imageGrid.Width = 200;
            _imageGrid.Height = 200;
            MainPanel.Children.Add(_imageGrid);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
