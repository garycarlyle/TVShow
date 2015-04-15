using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using TVShow.Model.Api;
using TVShow.Model.Movie;
using GalaSoft.MvvmLight.Command;

namespace TVShow.ViewModel
{
    /// <summary>
    /// ViewModel which takes care of movies' list (searching, retrieving from API and pagination)
    /// </summary>
    public class MoviesViewModel : ViewModelBase
    {
        #region Properties

        #region Property -> ApiService
        /// <summary>
        /// Service used to consume the API
        /// </summary>
        private IService ApiService { get; set; }
        #endregion

        #region Property -> Movies
        /// <summary>
        /// Movies loaded from the service and shown in the interface
        /// </summary>
        private ObservableCollection<MovieShortDetails> _movies = new ObservableCollection<MovieShortDetails>();
        public ObservableCollection<MovieShortDetails> Movies
        {
            get { return _movies; }
            set { Set(() => Movies, ref _movies, value, true); }
        }
        #endregion

        #region Property -> Pagination
        /// <summary>
        /// Current page number of loaded movies
        /// </summary>
        private int Pagination { get; set; }
        #endregion

        #region Property -> CancellationLoadMoviesInfosToken
        /// <summary>
        /// Token to cancel movie loading
        /// </summary>
        private CancellationTokenSource CancellationLoadingToken { get; set; }
        #endregion

        #region Property -> MaxMoviesPerPage
        /// <summary>
        /// Maximum movies number to load per page request
        /// </summary>
        public int MaxMoviesPerPage { private get; set; }
        #endregion

        #region Property -> SearchMessageToken
        /// <summary>
        /// Token for message subscription when searching movies
        /// </summary>
        private static readonly Guid SearchMessageToken = new Guid();
        #endregion

        #region Property -> SearchMoviesFilter
        /// <summary>
        /// The filter for searching movies
        /// </summary>
        private string _searchMoviesFilter = String.Empty;
        public string SearchMoviesFilter
        {
            get { return _searchMoviesFilter; }
            set
            {
                if (value != _searchMoviesFilter)
                {
                    string oldValue = _searchMoviesFilter;
                    _searchMoviesFilter = value;
                    Messenger.Default.Send(new PropertyChangedMessage<string>(oldValue, value, Helpers.Constants.SearchMoviesFilterPropertyName), SearchMessageToken);
                }
            }
        }
        #endregion

        #endregion

        #region Commands

        #region Command -> ReloadMovies
        /// <summary>
        /// Reload movies 
        /// </summary>
        public RelayCommand ReloadMovies
        {
            get;
            private set;
        }
        #endregion

        #endregion

        #region Constructors

        #region Constructor -> MoviesViewModel
        /// <summary>
        /// Initializes a new instance of the MoviesViewModel class.
        /// </summary>
        public MoviesViewModel()
            : this(new Service())
        {
        }
        #endregion

        #region Constructor -> MoviesViewModel
        /// <summary>
        /// Initializes a new instance of the MoviesViewModel class.
        /// </summary>
        /// <param name="apiService">apiService</param>
        private MoviesViewModel(IService apiService)
        {
            ApiService = apiService;

            // Set the CancellationToken for having the possibility to stop a task
            CancellationLoadingToken = new CancellationTokenSource();

            MaxMoviesPerPage = Helpers.Constants.MaxMoviesPerPage;

            ReloadMovies = new RelayCommand(async () =>
            {
                Messenger.Default.Send<bool>(false, Helpers.Constants.ConnectionErrorPropertyName);
                await LoadNextPage();
            });

            Messenger.Default.Register<PropertyChangedMessage<string>>(
                this, SearchMessageToken, async e => await SearchMovies(e.NewValue)
            );
        }
        #endregion

        #endregion

        #region Methods

        #region Method -> SearchMovies
        /// <summary>
        /// Search movies
        /// </summary>
        /// <param name="searchFilter">The parameter of the search</param>
        private async Task SearchMovies(string searchFilter)
        {
            // We stop any movie loading before searching action 
            StopLoadingMovies();

            CancellationLoadingToken = new CancellationTokenSource();
            
            // We start from scratch : clean everything to not interfer with the results
            Movies.Clear();
            Pagination = 0;

            if (!String.IsNullOrEmpty(searchFilter))
            {
                // Inform the subscribers we're actually searching for movies
                OnMoviesLoading(new EventArgs());

                // Let's do our search
                await LoadNextPage(searchFilter);
            }
            else
            {
                // Load the first set of movies
                await LoadNextPage();
            }
        }
        #endregion

        #region Method -> LoadNextPage
        /// <summary>
        /// Load next page with an optional search parameter
        /// </summary>
        /// <param name="searchFilter">An optional search parameter which is specified to the API</param>
        public async Task LoadNextPage(string searchFilter = null)
        {
            /* Check if we're searching with filter (if so, the method parameter and SearchMoviesFilter property should not be empty)
             * Otherwise, both should be empty
             * */
            if ((String.IsNullOrEmpty(searchFilter) && String.IsNullOrEmpty(SearchMoviesFilter)) ||
                (!String.IsNullOrEmpty(searchFilter) && !String.IsNullOrEmpty(SearchMoviesFilter)))
            {
                // We update the current pagination
                Pagination++;

                // Inform the subscribers we're actually loading movies
                OnMoviesLoading(new EventArgs());

                // The page to load is new, never met it before, so we load the new page via the service
                Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>> results =
                    await ApiService.GetMoviesAsync(searchFilter,
                        MaxMoviesPerPage,
                        Pagination,
                        CancellationLoadingToken);

                // These are the loaded movies
                IEnumerable<MovieShortDetails> movies = results.Item1;

                // Inform the subscribers we loaded movies
                OnMoviesLoaded(new EventArgs());

                // Check if we met any exception in the GetMoviesInfosAsync method
                if (HandleExceptions(results.Item2)) return;

                // Now we download the cover image for each movie
                foreach (var movie in movies)
                {
                    // The API filters on titles, actor's name and director's name. Here we just want to filter on title movie.
                    if (String.IsNullOrEmpty(searchFilter) || (!String.IsNullOrEmpty(searchFilter) && movie.Title.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        Movies.Add(movie);

                        // Download the cover image of the movie
                        Tuple<string, IEnumerable<Exception>> movieCover =
                            await ApiService.DownloadMovieCoverAsync(movie.ImdbCode,
                                movie.MediumCoverImage,
                                CancellationLoadingToken);

                        // Check if we met any exception
                        if (HandleExceptions(movieCover.Item2)) return;

                        // We associate the path of the cover image to each movie
                        foreach (var movieItem in Movies)
                        {
                            if (movieItem.ImdbCode == movie.ImdbCode)
                            {
                                movieItem.MediumCoverImageUri = movieCover.Item1;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Method -> HandleExceptions
        /// <summary>
        /// Handle list of exceptions
        /// </summary>
        /// <param name="exceptions">List of exceptions</param>
        private bool HandleExceptions(IEnumerable<Exception> exceptions)
        {
            foreach (var e in exceptions)
            {
                var taskCancelledException = e as TaskCanceledException;
                if (taskCancelledException != null)
                {
                    // Something as cancelled the loading. We go back.
                    Pagination--;
                    return true;
                }

                var webException = e as WebException;
                if (webException != null)
                {
                    // There's a connection error. Send the message and go back.
                    Messenger.Default.Send<bool>(true, Helpers.Constants.ConnectionErrorPropertyName);
                    Pagination--;
                    return true;
                }

                // Another exception has occured. Go back.
                Pagination--;
                return true;
            }
            return false;
        }
        #endregion

        #region Method -> StopLoadingMovies
        /// <summary>
        /// Cancel the loading of movies 
        /// </summary>
        public void StopLoadingMovies()
        {
            if (CancellationLoadingToken != null && CancellationLoadingToken.Token.CanBeCanceled)
            {
                CancellationLoadingToken.Cancel(true);
            }
        }
        #endregion

        #endregion

        #region Events

        #region Event -> MoviesLoading
        /// <summary>
        /// MoviesLoading event
        /// </summary>
        public event EventHandler<EventArgs> MoviesLoading;
        /// <summary>
        /// On loading movies
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
        protected virtual void OnMoviesLoading(EventArgs e)
        {
            EventHandler<EventArgs> handler = MoviesLoading;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> MoviesLoaded
        /// <summary>
        /// MoviesLoaded event
        /// </summary>
        public event EventHandler<EventArgs> MoviesLoaded;
        /// <summary>
        /// On finished loading movies
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
        protected virtual void OnMoviesLoaded(EventArgs e)
        {
            EventHandler<EventArgs> handler = MoviesLoaded;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #endregion

        public override void Cleanup()
        {
            Messenger.Default.Unregister<Tuple<int, string>>(this);
            Messenger.Default.Unregister<PropertyChangedMessage<string>>(this);
            base.Cleanup();
        }
    }
}
