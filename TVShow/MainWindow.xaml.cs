using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using TVShow.Events;
using TVShow.ViewModel;
using GalaSoft.MvvmLight.Threading;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

namespace TVShow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Properties

        #region Property -> MediaPlayerIsPlaying
        private bool MediaPlayerIsPlaying;
        #endregion

        #region Property -> UserIsDraggingSlider
        private bool UserIsDraggingSlider;
        #endregion

        #region Property -> timer
        private DispatcherTimer timer;
        #endregion

        #region Property -> MovieLoadingProgress
        public string MovieLoadingProgress { get; set; }
        #endregion

        private void ToggleFullScreen(object sender, RoutedEventArgs e)
        {
            UseNoneWindowStyle = true;
            IgnoreTaskbarOnMaximize = true;
            WindowState = WindowState.Maximized;
            if (MediaPlayer != null && MediaPlayer.Source != null)
            {
                MediaPlayer.Stretch = Stretch.Fill;
            }
        }

        private void BackToBoxedScreen(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                UseNoneWindowStyle = false;
                IgnoreTaskbarOnMaximize = false;
                WindowState = WindowState.Normal;
                MediaPlayer.Stretch = Stretch.Uniform;
            }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            ShowTitleBar = false;
            Loaded += MainWindow_Loaded;

            // Action when window is about to close
            Closing += (s, e) =>
            {
                // Unsubscribe events
                Loaded -= MainWindow_Loaded;

                // Stop playing and downloading a movie if any
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    if (vm.IsDownloadingMovie)
                    {
                        MediaPlayer.Stop();
                        MediaPlayer.Close();
                        MediaPlayer.Source = null;
                        vm.StopDownloadingMovie();
                    }

                    // Unsubscribe events
                    vm.ConnectionError -= OnConnectionInError;
                    vm.DownloadingMovie -= OnDownloadingMovie;
                    vm.StoppedDownloadingMovie -= OnStoppedDownloadingMovie;
                    vm.LoadedMovie -= OnLoadedMovie;
                    vm.BufferedMovie -= OnBufferedMovie;
                    vm.LoadingMovieProgress -= OnLoadingMovieProgress;
                }

                ViewModelLocator.Cleanup();
            };
        }
        #endregion

        #region Methods

        #region Method -> MainWindow_Loaded
        /// <summary>
        /// Subscribes to events when window is loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedEventArgs</param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.ConnectionError += OnConnectionInError;
                vm.DownloadingMovie += OnDownloadingMovie;
                vm.StoppedDownloadingMovie += OnStoppedDownloadingMovie;
                vm.LoadedMovie += OnLoadedMovie;
                vm.LoadingMovie += OnLoadingMovie;
                vm.BufferedMovie += OnBufferedMovie;
                vm.LoadingMovieProgress += OnLoadingMovieProgress;
            }

            /*
             * This is used to override the default value of WPF framerate (default is 60fps). 
             * To boost performance, we can set it down to 30 (good compromise between performance and good visual)
             */
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 30 }
            );

            /*
             * Usefull for SliderProgress
             */
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            PreviewKeyDown += new KeyEventHandler(BackToBoxedScreen);
        }

        #endregion

        #region Method -> OnLoadingMovieProgress
        /// <summary>
        /// Report progress when a movie is loading and set to visible the progressbar, the cancel button and the LoadingText label
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieLoadingProgressEventArgs</param>
        private void OnLoadingMovieProgress(object sender, MovieLoadingProgressEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (ProgressBar.Visibility == Visibility.Collapsed)
                {
                    ProgressBar.Visibility = Visibility.Visible;
                }

                if (StopLoadingMovieButton.Visibility == Visibility.Collapsed)
                {
                    StopLoadingMovieButton.Visibility = Visibility.Visible;
                }

                if (LoadingText.Visibility == Visibility.Collapsed)
                {
                    LoadingText.Visibility = Visibility.Visible;
                }

                ProgressBar.Value = e.Progress;

                // The percentage here is related to the buffering progress
                double percentage = e.Progress/Helpers.Constants.MinimumBufferingBeforeMoviePlaying*100.0;

                if (percentage >= 100)
                {
                    percentage = 100;
                }

                if (e.DownloadRate >= 1000)
                {
                    LoadingText.Text = "Buffering : " + Math.Round(percentage, 0) + "%" + " ( " + e.DownloadRate / 1000 + " MB/s)";
                }
                else
                {
                    LoadingText.Text = "Buffering : " + Math.Round(percentage, 0) + "%" + " ( " + e.DownloadRate + " kB/s)";
                }
            });
        }
        #endregion

        #region Method -> OnDownloadingMovie
        /// <summary>
        /// Fade in the movie page's opacity when a movie is loading
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnDownloadingMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                #region Fade in opacity
                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                Content.BeginAnimation(OpacityProperty, opacityAnimation);
                #endregion
            });
        }
        #endregion

        #region Method -> OnLoadingMovie
        /// <summary>
        /// Open the movie flyout when a movie is selected from the main interface, show progress and hide content
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        void OnLoadingMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                MoviePage.IsOpen = true;

                MovieProgressBar.Visibility = Visibility.Visible;
                MovieContainer.Visibility = Visibility.Collapsed;

                MovieContainer.Opacity = 0.0;
            });
        }
        #endregion

        #region Method -> OnLoadedMovie
        /// <summary>
        /// Show content and hide progress when a movie is loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        void OnLoadedMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                MovieProgressBar.Visibility = Visibility.Collapsed;
                MovieContainer.Visibility = Visibility.Visible;

                #region Fade in opacity

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                MovieContainer.BeginAnimation(OpacityProperty, opacityAnimation);

                #endregion
            });
        }
        #endregion

        #region Method -> OnBufferedMovie
        /// <summary>
        /// Play the movie when buffered
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieBufferedEventArgs</param>
        private void OnBufferedMovie(object sender, MovieBufferedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                #region Dispatcher Timer

                timer.Tick += Timer_Tick;
                timer.Start();

                #endregion

                // Open the player and play the movie
                MoviePlayer.IsOpen = true;
                MediaPlayer.Source = new Uri(e.PathToFile);
                MediaPlayer.Play();
                MediaPlayer.StretchDirection = StretchDirection.Both;

                MediaPlayerIsPlaying = true;

                ProgressBar.Visibility = Visibility.Collapsed;
                StopLoadingMovieButton.Visibility = Visibility.Collapsed;
                LoadingText.Visibility = Visibility.Collapsed;
            });
        }
        #endregion

        #region Method -> OnConnectionInError
        /// <summary>
        /// Open the popup when a connection error has occured
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieBufferedEventArgs</param>
        private async void OnConnectionInError(object sender, EventArgs e)
        {
            // Set and open a MetroDialog to inform that a connection error occured
            MetroDialogSettings settings = new MetroDialogSettings();
            settings.ColorScheme = MetroDialogColorScheme.Theme;
            MessageDialogResult result = await
                this.ShowMessageAsync("Internet connection error.",
                    "You seem to have an internet connection error. Please retry.",
                    MessageDialogStyle.Affirmative, settings);

            if (MoviesUc.ProgressRing.IsActive)
            {
                MoviesUc.ProgressRing.IsActive = false;
            }

            // Catch the response's user (when clicked OK)
            if (result == MessageDialogResult.Affirmative)
            {
                // Close the movie page
                if (MoviePage.IsOpen)
                {
                    MoviePage.IsOpen = false;
                    // Hide the movies list (the connection is in error, so no movie manipulation is available)
                    MoviesUc.Opacity = 0;
                }
            }
        }
        #endregion

        #region Method -> OnStoppedDownloadingMovie
        /// <summary>
        /// Close the player and go back to the movie page when the downloading of the movie has stopped
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnStoppedDownloadingMovie(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (MediaPlayer != null && MediaPlayer.Source != null)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Close();
                    MediaPlayer.Source = null;
                    MediaPlayerIsPlaying = false;
                }

                #region Dispatcher Timer
                timer.Tick -= Timer_Tick;
                timer.Stop();
                #endregion

                ProgressBar.Visibility = Visibility.Collapsed;
                StopLoadingMovieButton.Visibility = Visibility.Collapsed;
                LoadingText.Visibility = Visibility.Collapsed;
                ProgressBar.Value = 0.0;

                #region Fade out opacity

                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.0, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1.0, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);

                Content.BeginAnimation(OpacityProperty, opacityAnimation);

                #endregion
            });
        }
        #endregion

        #region Method -> Timer_Tick
        /// <summary>
        /// Report the playing progress on the timeline
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((MediaPlayer.Source != null) && (MediaPlayer.NaturalDuration.HasTimeSpan) && (!UserIsDraggingSlider))
            {
                SliderProgress.Minimum = 0;
                SliderProgress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                SliderProgress.Value = MediaPlayer.Position.TotalSeconds;
            }
        }
        #endregion

        #region Method -> Play_CanExecute
        /// <summary>
        /// Each time the CanExecute play command change, update the visibility of Play/Pause buttons in the player
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">CanExecuteRoutedEventArgs</param>
        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (MediaPlayer != null) && (MediaPlayer.Source != null);
            if (MediaPlayerIsPlaying)
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
        #endregion

        #region Method -> Play_Executed
        /// <summary>
        /// Play the current movie
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">ExecutedRoutedEventArgs</param>
        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MediaPlayer.Play();
            MediaPlayerIsPlaying = true;

            StatusBarItemPlay.Visibility = Visibility.Collapsed;
            StatusBarItemPause.Visibility = Visibility.Visible;
        }
        #endregion

        #region Method -> Pause_CanExecute
        /// <summary>
        /// Each time the CanExecute play command change, update the visibility of Play/Pause buttons in the player
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">CanExecuteRoutedEventArgs</param>
        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = MediaPlayerIsPlaying;
            if (MediaPlayerIsPlaying)
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
        #endregion

        #region Method -> Pause_Executed
        /// <summary>
        /// Pause the movie playing
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">CanExecuteRoutedEventArgs</param>
        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MediaPlayer.Pause();
            MediaPlayerIsPlaying = false;

            StatusBarItemPlay.Visibility = Visibility.Visible;
            StatusBarItemPause.Visibility = Visibility.Collapsed;
        }
        #endregion        

        #region Method -> SliderProgress_DragStarted
        /// <summary>
        /// Report when dragging is used
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">DragStartedEventArgs</param>
        private void SliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            UserIsDraggingSlider = true;
        }
        #endregion

        #region Method -> SliderProgress_DragCompleted
        /// <summary>
        /// Report when user has finished dragging
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">DragCompletedEventArgs</param>
        private void SliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UserIsDraggingSlider = false;
            MediaPlayer.Position = TimeSpan.FromSeconds(SliderProgress.Value);
        }
        #endregion

        #region Method -> SliderProgress_ValueChanged
        /// <summary>
        /// Report runtime when progress changed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedPropertyChangedEventArgs</param>
        private void SliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                TextProgressStatus.Text = TimeSpan.FromSeconds(SliderProgress.Value).ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture) + " / " + TimeSpan.FromSeconds(vm.Movie.Runtime * 60).ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture);
            }
        }
        #endregion

        #region Method -> Grid_MouseWheel
        /// <summary>
        /// When user uses the mousewheel, update the volume
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MouseWheelEventArgs</param>
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
        #endregion

        #endregion
    }
}