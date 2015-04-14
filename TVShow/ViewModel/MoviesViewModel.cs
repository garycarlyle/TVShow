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
    /// ViewModel to interact with the Rest API model
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

        #region Property -> SavedMoviesDictionary
        /// <summary>
        /// Saved movies for fast pagination in the interface
        /// </summary>
        public Dictionary<int, List<MovieShortDetails>> SavedMovies = new Dictionary<int, List<MovieShortDetails>>();
        #endregion

        #region Property -> Pagination
        /// <summary>
        /// Current page number of loaded movies
        /// </summary>
        public int Pagination { get; set; }
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
        public int MaxMoviesPerPage { get; set; }
        #endregion

        #region Property -> SearchMessageToken
        /// <summary>
        /// Token for message subscription when searching movies
        /// </summary>
        public static readonly Guid SearchMessageToken = new Guid();
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

        #region Property -> PaginationLimit
        /// <summary>
        /// The max number page for loaded movies
        /// </summary>
        public int PaginationLimit = Int32.MaxValue;
        #endregion

        #endregion

        #region Commands

        #region Command -> ReloadMoviesAfterConnectionInError

        public RelayCommand ReloadMoviesAfterConnectionInError
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
        public MoviesViewModel(IService apiService)
        {
            ApiService = apiService;

            ReloadMoviesAfterConnectionInError = new RelayCommand(async () =>
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
            await StopLoadingMovies();

            // Set the pagination limit to the default : we don't know yet the limit of the response
            PaginationLimit = Int32.MaxValue;

            if (!String.IsNullOrEmpty(searchFilter))
            {
                // We start from scratch : clean everything to not interfer with the results
                SavedMovies.Clear();
                Movies.Clear();
                Pagination = 0;

                try
                {
                    // Inform the subscribers we're actually searching for movies
                    OnMoviesLoading(new EventArgs());

                    // Reset the CancellationToken for having the possibility to stop the search
                    CancellationLoadingToken = new CancellationTokenSource();

                    // Impose a delay before each search request (otherwise we don't have time to clean things). We can also stop the search instantly with the token
                    await Task.Delay(1000, CancellationLoadingToken.Token);

                    // Let's do our search
                    await LoadNextPage(searchFilter);
                }
                catch (TaskCanceledException)
                {
                    throw new TaskCanceledException();   
                }
            }
            else
            {
                // We cleaned the search filter : let's start cleaning data
                SavedMovies.Clear();
                Movies.Clear();
                Pagination = 0;

                // Load the first set of movies
                await LoadNextPage();
            }
        }
        #endregion

        #region Method -> LoadPreviousPage
        /// <summary>
        /// Load previous page 
        /// </summary>
        public void LoadPreviousPage()
        {
            // Inform the subscribers we're actually loading movies
            OnMoviesLoading(new EventArgs());

            // We want to load the previous movies only if there is enough content to load before (lazy pagination)
            if (Pagination >= 3)
            {
                List<MovieShortDetails> moviesToRecover = new List<MovieShortDetails>();

                /* We want to load the previous page (the one which is on 2-top level of the current one)
                 * Actually, the pagination system is pretty simple: there's constantly only 2 loaded pages in the interface (except for the first load, when there's only one page)
                 * So, each time we load the next page, we clean the page which is on 2-top level
                 * Each time we load the previous page, we clean the page which is on 2-bottom level
                 * Anyway, the cleaned pages are stored in the savedMovies dictionnary which index each page (and for each page, we have our list of movies) so that it can be requested easily.
                 * Why this pagination system ? 'cause UI is overloaded and slowed down dramatically after 40-50 pages on Core i5 (yup, WPF is not really super efficient in terms of performance)
                 * */
                SavedMovies.TryGetValue(Pagination - 2, out moviesToRecover);

                if (moviesToRecover != null)
                {
                    List<MovieShortDetails> temp = moviesToRecover.ToList();

                    // We have to reverse the movies list to respect the order of the page, because we stored it using a reverse order. Yep, things to improve here.
                    temp.Reverse();
                    foreach (MovieShortDetails item in temp)
                    {
                        Movies.Insert(0, item);
                    }

                    // We remove the movies of the page on 2-bottom level
                    DropMoviesByPage(Pagination);
                    
                    // We update our pagination
                    Pagination -= 1;
                }
            }

            // Inform the subscribers we loaded movies
            OnMoviesLoaded(new EventArgs());
        }

        #endregion

        #region Method -> LoadNextPage
        /// <summary>
        /// Load next page with an optional search parameter
        /// </summary>
        /// <param name="searchFilter">An optional search parameter which is specified to the API</param>
        public async Task LoadNextPage(string searchFilter = null)
        {
            if (PaginationLimit == Pagination)
            {
                // We reached the maximum pages of the requested movies. We inform subscribers movies has been loaded (even if there's not, nevermind)
                OnMoviesLoaded(new EventArgs());
                return;
            }

            /* Check if we're searching with filter (if so, the method parameter and SearchMoviesFilter property should not be empty)
             * Otherwise, both should be empty
             * */
            if ((String.IsNullOrEmpty(searchFilter) && String.IsNullOrEmpty(SearchMoviesFilter)) ||
                (!String.IsNullOrEmpty(searchFilter) && !String.IsNullOrEmpty(SearchMoviesFilter)))
            {
                // Reset the CancellationToken for having the possibility to stop loading
                CancellationLoadingToken = new CancellationTokenSource();

                // We update the current pagination
                Pagination++;

                // Inform the subscribers we're actually loading movies
                OnMoviesLoading(new EventArgs());

                // We check if the page to load is already saved into the SavedMovies dictionnary (ie. we already loaded it)
                List<MovieShortDetails> pageAlreadyProcessed = new List<MovieShortDetails>();
                SavedMovies.TryGetValue(Pagination, out pageAlreadyProcessed);
                if (pageAlreadyProcessed != null)
                {
                    // Our page is already loaded in SavedMovies
                    if (Pagination >= 3)
                    {
                        // We have at least 2 pages behind the current one. As explained in the LoadPreviousPage method about lazy pagination, we drop the page on 2-bottom level
                        DropMoviesByPage(Pagination - 2);
                    }

                    // We add our new page in the Movies collection
                    foreach (MovieShortDetails item in pageAlreadyProcessed)
                    {
                        Movies.Add(item);
                    }

                    // Inform the subscribers we loaded movies
                    OnMoviesLoaded(new EventArgs());
                }
                else
                {
                    // The page to load is new, never met it before, so we load the new page via the service
                    Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>> results = await ApiService.GetMoviesAsync(searchFilter,
                        MaxMoviesPerPage,
                        Pagination,
                        CancellationLoadingToken);

                    // These are the loaded movies
                    IEnumerable<MovieShortDetails> movies = results.Item1;

                    // Check if we met any exception in the GetMoviesInfosAsync method
                    foreach (var e in results.Item2)
                    {
                        var taskCancelledException = e as TaskCanceledException;
                        if (taskCancelledException != null)
                        {
                            // Something as cancelled the loading. We go back.
                            Pagination--;
                            OnMoviesLoaded(new EventArgs());
                            return;
                        }

                        var webException = e as WebException;
                        if (webException != null)
                        {
                            // There's a connection error. Send the message and go back.
                            Messenger.Default.Send<bool>(true, Helpers.Constants.ConnectionErrorPropertyName);
                            Pagination--;
                            OnMoviesLoaded(new EventArgs());
                            return;
                        }

                        // Another exception has occured. Go back.
                        Pagination--;
                        OnMoviesLoaded(new EventArgs());
                        return;
                    }

                    if (movies.Count() < MaxMoviesPerPage)
                    {
                        // We reached the limit of the request
                        PaginationLimit = Pagination;
                    }

                    if (!String.IsNullOrEmpty(searchFilter))
                    {
                        /*
                         * Well, the API filters on titles, actor's name and director's name. Here we just want to filter on title movie.
                         * */
                        movies =
                            results.Item1.Where(
                                a => a.Title.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    // We "backup" our page into the SavedMovies dictionnary (see the pagination system explanation in the LoadPreviousPage method)
                    SaveMovies(movies);

                    if (Pagination >= 3)
                    {
                        // We have at least 2 pages behind the current one. As explained in the LoadPreviousPage method about lazy pagination, we drop the page on 2-bottom level
                        DropMoviesByPage(Pagination - 2);
                    }

                    // Here we add the movies of the loaded page
                    foreach (var movie in movies)
                    {
                        Movies.Add(movie);
                    }

                    // Inform the subscribers we loaded movies
                    OnMoviesLoaded(new EventArgs());

                    // Now we download the cover image for each movie
                    foreach (var movie in movies)
                    {
                        // Download the cover image of the movie
                        Tuple<string, IEnumerable<Exception>> movieCover = await ApiService.DownloadMovieCoverAsync(movie.ImdbCode,
                            movie.MediumCoverImage,
                            CancellationLoadingToken);

                        // Check if we met any exception
                        foreach (var movieCoverException in movieCover.Item2)
                        {
                            var taskCancelledException = movieCoverException as TaskCanceledException;
                            if (taskCancelledException != null)
                            {
                                // Something as cancelled the loading. We go back.
                                DropMoviesByPage(Pagination);
                                Pagination--;
                                return;
                            }

                            var webException = movieCoverException as WebException;
                            if (webException != null)
                            {
                                // There's a connection error. Send the message and go back.
                                DropMoviesByPage(Pagination);
                                Messenger.Default.Send<bool>(true, Helpers.Constants.ConnectionErrorPropertyName);
                                Pagination--;
                                return;
                            }

                            // Another exception has occured. Go back.
                            DropMoviesByPage(Pagination);
                            Pagination--;
                            return;
                        }

                        // We associate the path of the cover image to each movie
                        foreach (var movieItem in SavedMovies.SelectMany(x => x.Value))
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

        #region Method -> SaveMovies
        /// <summary>
        /// Save the movies of a given page 
        /// </summary>
        private void SaveMovies(IEnumerable<MovieShortDetails> movies)
        {
            List<MovieShortDetails> actualValues = new List<MovieShortDetails>();
            SavedMovies.TryGetValue(Pagination, out actualValues);
            if (actualValues == null)
            {
                List<MovieShortDetails> storeMoviesValues = new List<MovieShortDetails>();
                foreach (var item in movies)
                {
                    storeMoviesValues.Add(item);
                }
                SavedMovies.Add(Pagination, storeMoviesValues);
            }
        }
        #endregion

        #region Method -> DropMoviesByPage
        /// <summary>
        /// Drop the movies of a given page 
        /// </summary>
        private void DropMoviesByPage(int page)
        {
            List<MovieShortDetails> moviesToDrop = new List<MovieShortDetails>();
            SavedMovies.TryGetValue(page, out moviesToDrop);
            if (moviesToDrop != null)
            {
                foreach (MovieShortDetails item in moviesToDrop)
                {
                    Movies.Remove(item);
                }
            }
        }
        #endregion

        #region Method -> StopLoadingMovies
        /// <summary>
        /// Cancel the loading of movies 
        /// </summary>
        public async Task StopLoadingMovies()
        {
            await Task.Run(() =>
            {
                if (CancellationLoadingToken != null)
                {
                    CancellationLoadingToken.Cancel(true);
                }
            });
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
