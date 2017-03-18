using System.Windows;
using Microsoft.Win32;
using NAudio.Wave;
using NSpeech;
using NSpeech.Verification;

namespace TestApplication
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictor dictor1;
        Signal dictor2;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void trainButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "WAV Files|*.wav"
            };
            openFileDialog.FileOk += (o, args) =>
            {
                var fileName = ((OpenFileDialog) o).FileName;
                int sampleRate;
                var samples = ReadFile(fileName, out sampleRate);

                dictor1 = new Dictor("Dictor 1", new Signal(samples, sampleRate));
            };
            openFileDialog.ShowDialog(this);
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "WAV Files|*.wav"
            };
            openFileDialog.FileOk += (o, args) =>
            {
                var fileName = ((OpenFileDialog)o).FileName;
                int sampleRate;
                var samples = ReadFile(fileName, out sampleRate);

                dictor2 = new Signal(samples, sampleRate);
            };

            openFileDialog.ShowDialog(this);

            var result = dictor1.Verify(dictor2);

            resultLabel.Content = "Result: " + result;
        }

        private static float[] ReadFile(string fileName, out int sampleRate)
        {
            float[] file;
            using (var reader = new WaveFileReader(fileName))
            {
                var sampleProvider = reader.ToSampleProvider();
                file = new float[reader.SampleCount];
                sampleProvider.Read(file, 0, (int)reader.SampleCount);
                sampleRate = reader.WaveFormat.SampleRate;
            }
            return file;
        }
    }
}
