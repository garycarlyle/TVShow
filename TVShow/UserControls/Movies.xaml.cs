using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TVShow.ViewModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;
using TVShow.CustomPanels;
using TVShow.Events;

namespace TVShow.UserControls
{
    /// <summary>
    /// Interaction logic for Movies.xaml
    /// </summary>
    public partial class Movies : UserControl
    {
        #region Constructor
        public Movies()
        {
            InitializeComponent();
            Loaded += async (s, e) =>
            {
                await OnMoviesUcLoaded(s, e);
            };
        }
        #endregion

        #region Methods

        #region Method -> OnMoviesUcLoaded
        /// <summary>
        /// Subscribe to events and load first page
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedEventArgs</param>
        private async Task OnMoviesUcLoaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MoviesViewModel;
            if (vm != null)
            {
                vm.MoviesLoaded += OnMoviesLoaded;
                vm.MoviesLoading += OnMoviesLoading;

                // At first load, we load the first page of movies
                await vm.LoadNextPage();
            }
        }
        #endregion

        #region Method -> OnMoviesLoading
        /// <summary>
        /// Fade in opacity of the window, let the progress ring appear and collapse the NoMouvieFound label when loading movies
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnMoviesLoading(object sender, EventArgs e)
        {
            // We have to deal with the DispatcherHelper, otherwise we're having the classic cross-thread access exception
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ProgressRing.IsActive = true;

                #region Fade in opacity
                DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                PowerEase opacityEasingFunction = new PowerEase();
                opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0));
                EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(0.2, KeyTime.FromPercent(1.0),
                    opacityEasingFunction);
                opacityAnimation.KeyFrames.Add(startOpacityEasing);
                opacityAnimation.KeyFrames.Add(endOpacityEasing);
                ItemsList.BeginAnimation(OpacityProperty, opacityAnimation);
                #endregion

                if (NoMouvieFound.Visibility == Visibility.Visible)
                {
                    NoMouvieFound.Visibility = Visibility.Collapsed;
                }
            });
        }
        #endregion

        #region Method -> OnMoviesLoaded
        /// <summary>
        /// Fade out opacity of the window, let the progress ring disappear and set to visible the NoMouvieFound label when movies are loaded
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs</param>
        private void OnMoviesLoaded(object sender, NumberOfLoadedMoviesEventArgs e)
        {
            // We have to deal with the DispatcherHelper, otherwise we're having the classic cross-thread access exception
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                // We exclude exceptions like TaskCancelled when getting back results
                if (e != null && (e.NumberOfMovies == 0 && !e.IsExceptionThrown) || (e.NumberOfMovies != 0 && !e.IsExceptionThrown))
                {
                    ProgressRing.IsActive = false;

                    #region Fade out opacity
                    DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
                    opacityAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                    PowerEase opacityEasingFunction = new PowerEase();
                    opacityEasingFunction.EasingMode = EasingMode.EaseInOut;
                    EasingDoubleKeyFrame startOpacityEasing = new EasingDoubleKeyFrame(0.2, KeyTime.FromPercent(0));
                    EasingDoubleKeyFrame endOpacityEasing = new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1.0),
                        opacityEasingFunction);
                    opacityAnimation.KeyFrames.Add(startOpacityEasing);
                    opacityAnimation.KeyFrames.Add(endOpacityEasing);
                    ItemsList.BeginAnimation(OpacityProperty, opacityAnimation);
                    #endregion
                }

                var vm = DataContext as MoviesViewModel;
                if (vm != null)
                {
                    // If we searched movies and there's no result, display the NoMovieFound label (we exclude exceptions like TaskCancelled when getting back results)
                    if (e != null && (!vm.Movies.Any() && e.NumberOfMovies == 0 && !e.IsExceptionThrown))
                    {
                        NoMouvieFound.Visibility = Visibility.Visible;
                    }
                }
            });
        }
        #endregion

        #region Method -> ScrollViewer_ScrollChanged
        /// <summary>
        /// Decide if we have to load previous or next page regarding the scroll position
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">ScrollChangedEventArgs</param>
        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double totalHeight = e.VerticalOffset + e.ViewportHeight;

            if (totalHeight.Equals(e.ExtentHeight))
            {
                // We are at the bottom position of the window: we have to load the next movies
                var vm = DataContext as MoviesViewModel;

                // We check if no loading is occuring
                if (vm != null && !ProgressRing.IsActive)
                {
                    // We check if the searching form is empty or not
                    if (String.IsNullOrEmpty(vm.SearchMoviesFilter))
                    {
                        await vm.LoadNextPage();
                    }
                    else
                    {
                        await vm.LoadNextPage(vm.SearchMoviesFilter);
                    }
                }
            }
        }
        #endregion

        #region Method -> ElasticWrapPanel_Loaded
        /// <summary>
        /// Subscribe NumberOfColumnsChanged to the NumberOfColumnsChanged event of the ElasticWrapPanel
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">RoutedEventArgs</param>
        private void ElasticWrapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var elasticWrapPanel = sender as ElasticWrapPanel;
            if (elasticWrapPanel != null)
            {
                elasticWrapPanel.NumberOfColumnsChanged += NumberOfColumnsChanged;
            }
        }
        #endregion

        #region Method -> NumberOfColumnsChanged
        /// <summary>
        /// When the column's number of the ElasticWrapPanel has changed, reset the MaxMoviesPerPage property to a value so that there's enough content to be able to scroll
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">NumberOfColumnChangedEventArgs</param>
        private void NumberOfColumnsChanged(object sender, NumberOfColumnChangedEventArgs e)
        {
            var vm = DataContext as MoviesViewModel;
            if (vm != null)
            {
                vm.MaxMoviesPerPage = e.NumberOfColumns * Helpers.Constants.NumberOfRowsPerPage;
            }
        }
        #endregion

        #endregion
    }
}
