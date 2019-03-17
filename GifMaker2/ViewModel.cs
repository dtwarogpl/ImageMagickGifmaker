using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using GifMaker2.Annotations;
using GifMaker2.Properties;

namespace GifMaker2
{
    public class ViewModel:INotifyPropertyChanged
    {
        private Visibility _dropTextVisibility;
        private Visibility _progressBarVisibility=Visibility.Collapsed;
        
        private string _infoText;
        private Visibility _settingsPanelVisibility = Visibility.Collapsed;
        private int _fuzz;
        private int _quality;
        private int _frameDelay;
        private string _resizeGeometry;
        public event PropertyChangedEventHandler PropertyChanged;

        private Settings Settings { get; set; }

        public ViewModel()
        {
            Init();
        }

        public void ResetSettings()
        {
            Settings.Reset();
            SetSettingsValues(Settings);
        }

        public ViewModel(Settings settings)
        {
            Init();
            SetSettingsValues(settings);
        }

        private void SetSettingsValues(Settings settings)
        {
            Settings = settings;
            FrameDelay = Settings.FrameDelay;
            Fuzz = Settings.Fuzz;
            ResizeGeometry = Settings.Resize;
            Quality = Settings.Quality;
        }

        private void Init()
        {
            DropTextVisibility = Visibility.Visible;
            ProgressBarVisibility = Visibility.Collapsed;
            SettingsPanelVisibility = Visibility.Collapsed;
            InfoText = "Drop files here";
        }

        public int Fuzz
        {
            get => _fuzz;
            set
            {
                if(value == _fuzz) return;

                _fuzz = value;
                OnPropertyChanged();
               
            }
        }

        public int Quality
        {
            get => _quality;
            set
            {
                if(value == _quality) return;

                _quality = value;
                OnPropertyChanged();
              
            }
        }
        public int FrameDelay
        {
            get => _frameDelay;
            set
            {
                if(value == _frameDelay) return;

                _frameDelay = value;
                OnPropertyChanged();
               
            }
        }

        public string ResizeGeometry
        {
            get => _resizeGeometry;
            set
            {
                if(value == _resizeGeometry) return;

                _resizeGeometry = value;
                OnPropertyChanged();
               
            }
        }

        public void UpdateSettings()
        {
            Settings.FrameDelay = FrameDelay;
            Settings.Fuzz = Fuzz;
            Settings.Quality = Quality;
            Settings.Resize = ResizeGeometry;
            Settings.Save();
        }

        public string InfoText
        {
            get => _infoText;
            set
            {
                if(value == _infoText) return;

                _infoText = value;
                OnPropertyChanged();
            }
        }

        public Visibility SettingsPanelVisibility
        {
            get { return _settingsPanelVisibility; }
            set
            {
                if(value == _settingsPanelVisibility) return;

                _settingsPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility DropTextVisibility
        {
            get => _dropTextVisibility;
            set
            {
                if(value == _dropTextVisibility) return;

                _dropTextVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                if(value == _progressBarVisibility) return;

                _progressBarVisibility = value;
                OnPropertyChanged();
            }
        }

        public void SetBusy()
        {
            ProgressBarVisibility = Visibility.Visible;
            DropTextVisibility = Visibility.Collapsed;
        }

        public void SetReady()
        {
            ProgressBarVisibility = Visibility.Collapsed;
            DropTextVisibility = Visibility.Visible;
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}