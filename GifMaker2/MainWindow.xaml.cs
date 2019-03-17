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
        private readonly ViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new ViewModel(_settings);
            this.DataContext = _viewModel;
        }

        private void DropBorder_OnDragEnter(object sender, DragEventArgs e)
        {
            DragDropHelper.SetIsDragOver((DependencyObject)sender, true);
        }

        private void DropBorder_OnPreviewDragLeave(object sender, DragEventArgs e)
        {
            DragDropHelper.SetIsDragOver((DependencyObject)sender, false);
        }

        //todo: Dodać sprawdzanie wymiarów i wyrzucić bląd jeśli są one różne - funkcja optimize (co ona robi?)
        //todo: obsługa błędów i komunikatów o błędach 
        private async void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            try
            {
                ShowProgressBar();

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


                HideProgressBar();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
                HideProgressBar();
            }
        }

        private void HideProgressBar()
        {
            _viewModel.SetReady();
            _viewModel.InfoText = "Done. Drop more files!";
        }

        private void ShowProgressBar()
        {
          _viewModel.SetBusy();
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
                            .Quality = _settings.Quality;
                    collection[index]
                            .ColorFuzz = new Percentage(_settings.Fuzz);
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

        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((Slider)sender).Value = Math.Round(((Slider)sender).Value, 0);
        }

        private void AppbarSettings_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.SettingsPanelVisibility = Visibility.Visible;
        }

        private void SaveSettings(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _viewModel.SettingsPanelVisibility = Visibility.Collapsed;
                _viewModel.UpdateSettings();
            }
            catch(Exception exception) { MessageBox.Show(exception.Message); }

        }

        private void ResetSettings(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _viewModel.ResetSettings();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }

    public class DragDropHelper
    {
        public static readonly DependencyProperty IsDragOverProperty = DependencyProperty.RegisterAttached(
            "IsDragOver", typeof(bool), typeof(DragDropHelper), new PropertyMetadata(default(bool)));

        public static void SetIsDragOver(DependencyObject element, bool value)
        {
            element.SetValue(IsDragOverProperty, value);
        }

        public static bool GetIsDragOver(DependencyObject element)
        {
            return (bool)element.GetValue(IsDragOverProperty);
        }
    }
}
