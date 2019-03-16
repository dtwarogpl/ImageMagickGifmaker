using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using ImageMagick;

namespace GifMaker2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Properties.Settings _settings = Properties.Settings.Default;
        public MainWindow()
        {
            InitializeComponent();

            if(!File.Exists(Path.Combine(_settings.ImageMagickPath)))
            {
                MessageBox.Show($"Nie odnaleziono pliku wykonywalnego biblioteki ImageMagick \r\n Zdefiniowana ściezka: {Properties.Settings.Default.ImageMagickPath}");
                Application.Current.Shutdown();
            }


        }
        //todo: Dodać sprawdzanie wymiarów i wyrzucić bląd jeśli są one różne - funkcja optimize (co ona robi?)
        private async void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            try
            {
                Working.Visibility = Visibility.Visible;
                TextBlock.Visibility = Visibility.Collapsed;

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                List<FileInfo> specifiedFiles = GetAllFilesFromDirOrPath(files)
                        .Select(x => new FileInfo(x))
                        .ToList();

                if(specifiedFiles.Count == 0) return;

                var directory = specifiedFiles.First()
                                              .DirectoryName;

                if(directory == null) return;

                var outputFilename = Path.Combine(directory, $"{specifiedFiles.Last() .Name}_({specifiedFiles.Count})_.gif");


                Task task = Task.Run(() => Render(files, outputFilename));
              
                await task;


                Working.Visibility = Visibility.Collapsed;
                TextBlock.Text = "Done. Drop more files!";
                TextBlock.Visibility = Visibility.Visible;
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
                Working.Visibility = Visibility.Collapsed;
                TextBlock.Text = "Drop some files here";
                TextBlock.Visibility = Visibility.Visible;
            }
        }

        private void Render(string[] files, string outputFilename)
        {
            using(MagickImageCollection collection = new MagickImageCollection())
            {
                for(var index = 0; index < files.Length; index++)
                {
                    var file = files[index];
                    collection.Add(file);
                    collection[index]
                            .AnimationDelay = _settings.FrameDelay;
                    collection[index]
                            .Quality = Int32.Parse(_settings.Quality);
                    collection[index]
                            .ColorFuzz = new Percentage(Int32.Parse(_settings.Fuzz));
                    collection[index]
                            .Resize(new MagickGeometry(_settings.Resize));
                }

                QuantizeSettings settings = new QuantizeSettings();
                settings.Colors = 256;
                collection.Quantize(settings);
                //collection.Optimize();

                // Optionally optimize the images (images should have the same size).
                collection.Optimize();

                collection.Write(outputFilename);
            }
            // the code that you want to measure comes here
        }

        private  List<string> GetAllFilesFromDirOrPath(string[] paths)
        {
            var specifiedFiles = new List<string>();
            foreach (var path in paths)
            {
                FileAttributes attr = File.GetAttributes(path);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    var ext = new List<string> { ".jpg", ".jpeg",  };

                    specifiedFiles.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                                     .Where(s => ext.Any(x =>
                                                     {
                                                         var extension = System.IO.Path.GetExtension(s);
                                                         return extension != null && extension.ToLower() ==x;
                                                     })));
                }

                else specifiedFiles.Add(path);
            }

            return specifiedFiles;
        }

       
    }
}
