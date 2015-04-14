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
            Closing += async (s, e) =>
            {
                // Unsubscribe events
                Loaded -= MainWindow_Loaded;

                // Stop playing and downloading a movie if any
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    if (MediaPlayerIsPlaying)
                    {
                        mePlayer.Close();
                        mePlayer.Source = null;
                        await vm.StopDownloadingMovie();
                    }

                    // Unsubscribe events
                    vm.ConnectionError -= OnConnectionInError;
                    vm.MovieLoading -= OnMovieLoading;
                    vm.MovieStoppedDownloading -= OnMovieStoppedDownloading;
                    vm.MovieLoaded -= OnMovieSelected;
                    vm.MovieBuffered -= OnMovieBuffered;
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
                vm.MovieLoading += OnMovieLoading;
                vm.MovieStoppedDownloading += OnMovieStoppedDownloading;
                vm.MovieLoaded += OnMovieSelected;
                vm.MovieBuffered += OnMovieBuffered;
                vm.MovieLoadingProgress += OnMovieLoadingProgress;
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
             * Usefull for sliProgress
             */
            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
        }
        #endregion

        #region Method -> OnMovieLoadingProgress
        /// <summary>
        /// Report progress when a movie is loading and set to visible the progressbar, the cancel button and the LoadingText label
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieLoadingProgressEventArgs</param>
        private void OnMovieLoadingProgress(object sender, MovieLoadingProgressEventArgs e)
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

                // The percentage here is related to the buffering progress
                double percentage = Math.Round(e.Progress, 1) / Helpers.Constants.MinimumBufferingBeforeMoviePlaying * 100;

                if (percentage >= 100)
                {
                    percentage = 100;
                }

                if (e.DownloadRate >= 1000)
                {
                    LoadingText.Text = "Buffering : " + percentage + "%" + " ( " + e.DownloadRate / 1000 + " MB/s)";
                }
                else
                {
                    LoadingText.Text = "Buffering : " + percentage + "%" + " ( " + e.DownloadRate + " kB/s)";
                }
            });
        }
        #endregion

        #region Method -> OnMovieLoading
        /// <summary>
        /// Fade in the movie page's opacity when a movie is loading
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnMovieLoading(object sender, EventArgs e)
        {
            #region Fade in opacity
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
        #endregion

        #region Method -> OnMovieSelected
        /// <summary>
        /// Open the movie flyout when a movie is selected from the main interface
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        void OnMovieSelected(object sender, EventArgs e)
        {
            MovieContainer.Opacity = 1.0;
            MoviePage.IsOpen = true;
        }
        #endregion

        #region Method -> OnMovieBuffered
        /// <summary>
        /// Play the movie when buffered
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">MovieBufferedEventArgs</param>
        private void OnMovieBuffered(object sender, MovieBufferedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                #region Dispatcher Timer
                timer.Tick += Timer_Tick;
                timer.Start();
                #endregion

                // Open the player and play the movie
                MoviePlayer.IsOpen = true;
                mePlayer.Source = new Uri(e.PathToFile);
                mePlayer.Play();
                MediaPlayerIsPlaying = true;
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

        #region Method -> OnMovieStoppedDownloading
        /// <summary>
        /// Close the player and go back to the movie page when the downloading of the movie has stopped
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnMovieStoppedDownloading(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (MediaPlayerIsPlaying)
                {
                    mePlayer.Stop();
                    mePlayer.Close();
                    mePlayer.Source = null;
                    MediaPlayerIsPlaying = false;
                }

                #region Dispatcher Timer
                timer.Tick -= Timer_Tick;
                timer.Stop();
                #endregion

                ProgressBar.Visibility = Visibility.Collapsed;
                StopLoadMovieButton.Visibility = Visibility.Collapsed;
                LoadingText.Visibility = Visibility.Collapsed;
                ProgressBar.Value = 0.0;

                #region Fade out opacity

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
        #endregion

        #region Method -> Timer_Tick
        /// <summary>
        /// Report the playing progress on the timeline
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!UserIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
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
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
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
            mePlayer.Play();
            MediaPlayerIsPlaying = true;
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
            mePlayer.Pause();
            MediaPlayerIsPlaying = false;
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

        #region Method -> sliProgress_DragStarted
        /// <summary>
        /// Report when dragging is used
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">DragStartedEventArgs</param>
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            UserIsDraggingSlider = true;
        }
        #endregion

        #region Method -> sliProgress_DragCompleted
        /// <summary>
        /// Report when user has finished dragging
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">DragCompletedEventArgs</param>
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            UserIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }
        #endregion

        #region Method -> sliProgress_ValueChanged
        /// <summary>
        /// Report runtime when progress changed
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedPropertyChangedEventArgs</param>
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture) + " / " + TimeSpan.FromSeconds(vm.Movie.Runtime * 60).ToString(@"hh\:mm\:ss", CultureInfo.CurrentCulture);
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
            mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }
        #endregion

        #endregion
    }
}