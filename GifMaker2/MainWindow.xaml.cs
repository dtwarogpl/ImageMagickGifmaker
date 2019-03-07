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

                string command =
                        $"convert -loop 0 -delay {_settings.FrameDelay} {string.Join(" ", specifiedFiles.Select(x => "\"" + x.FullName + "\""))} " +
                        $"-resize {_settings.Resize} -quality {_settings.Quality}% -fuzz {_settings.Fuzz}% +dither -layers Optimize \"{outputFilename}\"";


                Task task = Task.Run(() => RunMagickCmd(command));
                //wait for it to end without blocking the main thread
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

        public void RunMagickCmd(string arguments)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
                                         {
                                             RedirectStandardError = true,
                                             CreateNoWindow = true,
                                             UseShellExecute = false,
                                             WindowStyle = ProcessWindowStyle.Hidden,
                                             FileName = Path.Combine(Properties.Settings.Default.ImageMagickPath),
                                             Arguments = arguments
                                         };
            //  startInfo.Arguments.Replace(@" \", "");
            process.StartInfo = startInfo;
            process.Start();
            string errorsMsg = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(errorsMsg))
            {
                if (errorsMsg.Contains("warning/tiff.c/TIFFWarnings/925"))
                {
                    return;
                }

                throw new Exception(errorsMsg);
            }
        }
    }
}
