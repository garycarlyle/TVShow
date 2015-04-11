using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TVShow.Events;
using TVShow.ViewModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using TVShow.CustomPanels;
using TVShow.Helpers;

namespace TVShow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private bool _mediaPlayerIsPlaying = false;
        private bool _userIsDraggingSlider = false;

        public string MovieLoadingProgress { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) =>
            {
                Loaded -= MainWindow_Loaded;
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    if (_mediaPlayerIsPlaying)
                    {
                        mePlayer.Stop();
                        mePlayer.Source = null;
                    }
                    vm.ConnexionInErrorEvent -= OnConnexionInError;
                    vm.MovieLoading -= OnMovieLoading;
                    vm.MovieStoppedDownloading -= OnMovieStoppedDownloading;
                    vm.MovieSelected -= OnMovieSelected;
                    vm.MovieBuffered -= OnMovieBuffered;
                }

                ViewModelLocator.Cleanup();
            };
            ShowTitleBar = false;
            Loaded += MainWindow_Loaded;
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.ConnexionInErrorEvent += OnConnexionInError;
                vm.MovieLoading += OnMovieLoading;
                vm.MovieStoppedDownloading += OnMovieStoppedDownloading;
                vm.MovieSelected += OnMovieSelected;
                vm.MovieBuffered += OnMovieBuffered;
                vm.MovieLoadingProgress += OnMovieLoadingProgress;
            }
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 30 }
            );
        }

        private void OnMovieLoadingProgress(object sender, Events.MovieLoadingProgressEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (ProgressBar.Visibility == Visibility.Collapsed)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                }
                else if (StopLoadMovieButton.Visibility == Visibility.Collapsed)
                {
                    StopLoadMovieButton.Visibility = Visibility.Visible;
                }
                else if (LoadingText.Visibility == Visibility.Collapsed)
                {
                    LoadingText.Visibility = Visibility.Visible;
                }

                ProgressBar.Value = e.Progress;
                double percentage = Math.Round(e.Progress, 1)*50;
                if (percentage >= 100)
                {
                    percentage = 100;
                }
                LoadingText.Text = "Loading : " + percentage + "%" + " ( " + e.DownloadRate + " kB/s)";
            });
        }

        private void OnMovieLoading(object sender, EventArgs e)
        {
            #region Fading Opacity
            DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            PowerEase opacityEasingFunction = new PowerEase();
            opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
            EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0));
            EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(0.1, KeyTime.FromPercent(1.0),
                opacityEasingFunction);
            opacityAnimation.KeyFrames.Add(startOpacityEasing);
            opacityAnimation.KeyFrames.Add(endOpacityEasing);

            MovieContainer.BeginAnimation(OpacityProperty, opacityAnimation);

            #endregion
        }

        void OnMovieSelected(object sender, EventArgs e)
        {
            MovieContainer.Opacity = 1.0;
            MoviePage.IsOpen = true;
        }

        private void OnMovieBuffered(object sender, MovieBufferedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                MoviePlayer.IsOpen = true;
                mePlayer.Source = new Uri(e.PathToFile);
                mePlayer.Play();
                _mediaPlayerIsPlaying = true;
            });
        }

        private async void OnConnexionInError(object sender, EventArgs e)
        {
            MetroDialogSettings settings = new MetroDialogSettings();
            settings.ColorScheme = MetroDialogColorScheme.Theme;
            MessageDialogResult result = await
                this.ShowMessageAsync("Internet connection error.",
                    "You seem to have an internet connection error. Please retry.",
                    MessageDialogStyle.Affirmative, settings);
            if (result == MessageDialogResult.Affirmative)
            {
                if (MoviePage.IsOpen)
                {
                    MoviePage.IsOpen = false;
                    MoviesUc.Opacity = 0;
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
            if (_mediaPlayerIsPlaying)
            {
                StatusBarItemPlay.Visibility = Visibility.Collapsed;
                StatusBarItemPause.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBarItemPlay.Visibility = Visibility.Visible;
                StatusBarItemPause.Visibility = Visibility.Collapsed;
            }
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            _mediaPlayerIsPlaying = true;
            if (_mediaPlayerIsPlaying)
            {
                StatusBarItemPlay.Visibility = Visibility.Collapsed;
                StatusBarItemPause.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBarItemPlay.Visibility = Visibility.Visible;
                StatusBarItemPause.Visibility = Visibility.Collapsed;
            }
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _mediaPlayerIsPlaying;
            if (_mediaPlayerIsPlaying)
            {
                StatusBarItemPlay.Visibility = Visibility.Collapsed;
                StatusBarItemPause.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBarItemPlay.Visibility = Visibility.Visible;
                StatusBarItemPause.Visibility = Visibility.Collapsed;
            }
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
            _mediaPlayerIsPlaying = false;
            if (_mediaPlayerIsPlaying)
            {
                StatusBarItemPlay.Visibility = Visibility.Collapsed;
                StatusBarItemPause.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBarItemPlay.Visibility = Visibility.Visible;
                StatusBarItemPause.Visibility = Visibility.Collapsed;
            }
        }

        private void OnMovieStoppedDownloading(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (_mediaPlayerIsPlaying)
                {
                    mePlayer.Stop();
                    mePlayer.Source = null;
                    _mediaPlayerIsPlaying = false;
                }

                ProgressBar.Visibility = Visibility.Collapsed;
                StopLoadMovieButton.Visibility = Visibility.Collapsed;
                LoadingText.Visibility = Visibility.Collapsed;
                ProgressBar.Value = 0.0;

                #region Fading Opacity

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.1, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                MovieContainer.BeginAnimation(OpacityProperty, opacityAnimation);

                #endregion
            });
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss") + " / " + TimeSpan.FromSeconds(vm.Movie.Runtime * 60).ToString(@"hh\:mm\:ss");
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
    }
}