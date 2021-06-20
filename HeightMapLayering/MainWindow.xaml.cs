using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Path = System.IO.Path;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace HeightMapLayering
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

        private void OriginalFileButtonOnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() != true)
                return;

            OriginalFileName.Text = openFileDialog.FileName;
            DestinationName.Text =
                Path.GetDirectoryName(openFileDialog.FileName) + Path.DirectorySeparatorChar + "layers";
        }

        private void RunButtonOnClick(object sender, RoutedEventArgs e)
        {
            var originalImage = Image.Load<Rgba32>(OriginalFileName.Text);
            var layersNumber = int.Parse(LayersNumber.Text);
            var darknessPerLayer = (byte) Math.Ceiling(256 / (decimal) layersNumber);
            var darknessFactor = byte.Parse(DarknessFactor.Text);

            for (var layerNumber = 1; layerNumber <= layersNumber; layerNumber++)
            {
                var layerImage = new Image<Rgba32>(originalImage.Width, originalImage.Height);

                for (var rowIndex = 0; rowIndex < originalImage.Height; rowIndex++)
                {
                    var originalPixelRowSpan = originalImage.GetPixelRowSpan(rowIndex);
                    var layerPixelRowSpan = layerImage.GetPixelRowSpan(rowIndex);

                    for (var columnIndex = 0; columnIndex < originalImage.Width; columnIndex++)
                    {
                        var originalPixel = originalPixelRowSpan[columnIndex];

                        var brightness = originalPixel.R;
                        if (layerNumber == 1)
                        {
                            brightness = (byte) Math.Round((originalPixel.R + originalPixel.G + originalPixel.B) / 3d);
                        }

                        var darkness = BrightnessToDarkness(brightness);

                        if (darkness <= darknessPerLayer * 1.25)
                        {
                            originalPixelRowSpan[columnIndex] = new Rgba32(255, 255, 255, 255);
                            var adjustedBrightness = DarknessToBrightness((byte)(BrightnessToDarkness(brightness) * darknessFactor));
                            layerPixelRowSpan[columnIndex] = new Rgba32(adjustedBrightness, adjustedBrightness, adjustedBrightness, 255);
                        }
                        else
                        {
                            var adjustedBrightnessDelta = DarknessToBrightness((byte) (darknessPerLayer * darknessFactor));
                            var newBrightness = DarknessToBrightness((byte) (darkness - darknessPerLayer));

                            originalPixelRowSpan[columnIndex] = new Rgba32(
                                newBrightness,
                                newBrightness,
                                newBrightness,
                                255
                            );
                            layerPixelRowSpan[columnIndex] = new Rgba32(
                                adjustedBrightnessDelta,
                                adjustedBrightnessDelta,
                                adjustedBrightnessDelta,
                                255
                            );
                        }
                    }
                }

                Directory.CreateDirectory(DestinationName.Text);

                layerImage.SaveAsPng(DestinationName.Text + Path.DirectorySeparatorChar + layerNumber.ToString() +
                                     ".png");
            }
        }

        private static byte DarknessToBrightness(byte darknessDelta)
        {
            return (byte) (255 - darknessDelta);
        }

        private static byte BrightnessToDarkness(byte brightness)
        {
            return (byte) (255 - brightness);
        }
    }
}