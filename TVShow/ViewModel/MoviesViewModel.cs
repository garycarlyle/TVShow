using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using TVShow.Model.Api;
using TVShow.Model.Movie;
using TVShow.Events;
using TVShow.Helpers;
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
        /// Saved movies per page for fast pagination in the interface
        /// </summary>
        public Dictionary<int, List<MovieShortDetails>> SavedMoviesDictionary = new Dictionary<int, List<MovieShortDetails>>();
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
        private CancellationTokenSource CancellationLoadMoviesInfosToken { get; set; }
        #endregion

        #region Property -> MaxMoviesPerPage
        /// <summary>
        /// Maximum movies number to load per page request
        /// </summary>
        public int MaxMoviesPerPage { get; set; }
        #endregion

        #region
        /// <summary>
        /// Token for message subscription when searching movies
        /// </summary>
        public static readonly Guid SearchMessageToken = new Guid();
        #endregion

        #region Property -> SearchMovies
        /// <summary>
        /// The filter for searching movies if any
        /// </summary>
        private string _searchMovies = String.Empty;
        public string SearchMovies
        {
            get { return _searchMovies; }
            set
            {
                if (value != _searchMovies)
                {
                    string oldValue = _searchMovies;
                    _searchMovies = value;
                    Messenger.Default.Send(new PropertyChangedMessage<string>(oldValue, value, "SearchMovies"), SearchMessageToken);
                }
            }
        }
        #endregion

        #region
        /// <summary>
        /// The max number page for loaded movies
        /// </summary>
        public int PaginationLimit = Int32.MaxValue;
        #endregion

        #endregion

        #region Commands
        #region Command -> ReloadMoviesAfterConnexionInError

        public RelayCommand ReloadMoviesAfterConnexionInError
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

            ReloadMoviesAfterConnexionInError = new RelayCommand(async () =>
            {
                Messenger.Default.Send<bool>(false, "IsConnexionInError");
                await LoadNextMoviesInfos();
            });

            Messenger.Default.Register<PropertyChangedMessage<string>>(
                this, SearchMessageToken, async (e) => await OnSearchMovies(e.NewValue)
            );
        }
        #endregion

        #endregion

        #region Methods

        #region Method -> OnSearchMovies
        /// <summary>
        /// When a movie search is processing
        /// </summary>
        /// <param name="param">param</param>
        private async Task OnSearchMovies(string param)
        {
            await StopLoadingMoviesInfos();
            PaginationLimit = Int32.MaxValue;
            if (!String.IsNullOrEmpty(param))
            {
                SavedMoviesDictionary.Clear();
                Movies.Clear();
                Pagination = 0;
                try
                {
                    EventArgs onMoviesLoadingArgs = new EventArgs();
                    OnMoviesLoading(onMoviesLoadingArgs);

                    CancellationLoadMoviesInfosToken = new CancellationTokenSource();
                    await Task.Delay(1000, CancellationLoadMoviesInfosToken.Token);
                    await LoadNextMoviesInfos(param);
                }
                catch (TaskCanceledException)
                {
                    
                }
            }
            else
            {
                SavedMoviesDictionary.Clear();
                Movies.Clear();
                Pagination = 0;
                await LoadNextMoviesInfos();
            }
        }
        #endregion

        #region Method -> LoadPreviousMovies
        /// <summary>
        /// Load previous movies 
        /// </summary>
        public void LoadPreviousMovies()
        {
            EventArgs onMoviesLoadingArgs = new EventArgs();
            OnMoviesLoading(onMoviesLoadingArgs);

            // We want to load the previous movies only if there is enough content to load before
            if (Pagination >= 3)
            {
                List<MovieShortDetails> MoviesToAdd = new List<MovieShortDetails>();
                // We want to load the previous movies page (the one which is on 2 top level of the current one)
                SavedMoviesDictionary.TryGetValue(Pagination - 2, out MoviesToAdd);
                if (MoviesToAdd != null)
                {
                    List<MovieShortDetails> temp = MoviesToAdd.ToList();
                    // We have to reverse the movies list to respect the previous order
                    temp.Reverse();
                    foreach (MovieShortDetails item in temp)
                    {
                        Movies.Insert(0, item);
                    }

                    List<MovieShortDetails> moviesToDelete = new List<MovieShortDetails>();
                    SavedMoviesDictionary.TryGetValue(Pagination, out moviesToDelete);
                    if (moviesToDelete != null)
                    {
                        foreach (MovieShortDetails item in moviesToDelete)
                        {
                            Movies.Remove(item);
                        }
                    }
                    Pagination -= 1;
                }
            }

            EventArgs onMoviesInfosLoadedArgs = new EventArgs();
            OnMoviesInfosLoaded(onMoviesInfosLoadedArgs);
        }

        #endregion

        #region Method -> LoadNextMoviesInfos
        /// <summary>
        /// Load next movies 
        /// </summary>
        public async Task LoadNextMoviesInfos(string retrieveParam = null)
        {
            if (PaginationLimit == Pagination)
            {
                OnMoviesInfosLoaded(new EventArgs());
                return;
            }
            if ((String.IsNullOrEmpty(retrieveParam) && String.IsNullOrEmpty(SearchMovies)) ||
                (!String.IsNullOrEmpty(retrieveParam) && !String.IsNullOrEmpty(SearchMovies)))
            {
                CancellationLoadMoviesInfosToken = new CancellationTokenSource();
                Pagination++;
                OnMoviesLoading(new EventArgs());

                List<MovieShortDetails> pageAlreadyProcessed = new List<MovieShortDetails>();
                SavedMoviesDictionary.TryGetValue(Pagination, out pageAlreadyProcessed);
                if (pageAlreadyProcessed != null)
                {
                    if (Pagination >= 3)
                    {
                        List<MovieShortDetails> moviesToDelete = new List<MovieShortDetails>();
                        SavedMoviesDictionary.TryGetValue(Pagination - 2, out moviesToDelete);
                        if (moviesToDelete != null)
                        {
                            foreach (MovieShortDetails item in moviesToDelete)
                            {
                                Movies.Remove(item);
                            }
                        }
                    }
                    foreach (MovieShortDetails item in pageAlreadyProcessed)
                    {
                        Movies.Add(item);
                    }
                    OnMoviesInfosLoaded(new EventArgs());
                }
                else
                {
                    Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>> results = await ApiService.GetMoviesInfosAsync(retrieveParam,
                        MaxMoviesPerPage,
                        Pagination,
                        CancellationLoadMoviesInfosToken);

                    IEnumerable<MovieShortDetails> movies = results.Item1;
                    foreach (var e in results.Item2)
                    {
                        var ctException = e as TaskCanceledException;
                        if (ctException != null)
                        {
                            Pagination--;
                            OnMoviesInfosLoaded(new EventArgs());
                            return;
                        }

                        Messenger.Default.Send<bool>(true, "IsConnexionInError");
                        Pagination--;
                        OnMoviesInfosLoaded(new EventArgs());
                        return;
                    }

                    if (movies.Count() < MaxMoviesPerPage)
                    {
                        PaginationLimit = Pagination;
                    }

                    if (!String.IsNullOrEmpty(retrieveParam))
                    {
                        movies =
                            results.Item1.Where(
                                a => a.Title.IndexOf(retrieveParam, StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    #region Permet de stocker les "movies" dans le dictionnaire

                    List<MovieShortDetails> actualValues = new List<MovieShortDetails>();
                    SavedMoviesDictionary.TryGetValue(Pagination, out actualValues);
                    if (actualValues == null)
                    {
                        List<MovieShortDetails> storeMoviesValues = new List<MovieShortDetails>();
                        foreach (var item in movies)
                        {
                            storeMoviesValues.Add(item);
                        }
                        SavedMoviesDictionary.Add(Pagination, storeMoviesValues);
                    }

                    #endregion

                    if (Pagination >= 3)
                    {
                        List<MovieShortDetails> moviesToDelete = new List<MovieShortDetails>();
                        SavedMoviesDictionary.TryGetValue(Pagination - 2, out moviesToDelete);
                        if (moviesToDelete != null)
                        {
                            foreach (MovieShortDetails item in moviesToDelete)
                            {
                                Movies.Remove(item);
                            }
                        }
                    }

                    foreach (var movie in movies)
                    {
                        Movies.Add(movie);
                    }

                    OnMoviesInfosLoaded(new EventArgs());

                    foreach (var movie in movies)
                    {
                        Tuple<string, IEnumerable<Exception>> movieCover = await ApiService.DownloadMovieCoverAsync(movie.ImdbCode,
                            movie.MediumCoverImage,
                            CancellationLoadMoviesInfosToken);
                        foreach (var movieCoverException in movieCover.Item2)
                        {
                            var movieCoverWebException =
                                movieCoverException as WebException;
                            if (movieCoverWebException != null)
                            {
                                if (movieCoverWebException.Status ==
                                    WebExceptionStatus.NameResolutionFailure)
                                {
                                    Messenger.Default.Send<bool>(true,
                                        "IsConnexionInError");
                                    Pagination--;
                                    return;
                                }
                            }

                            var ctException = movieCoverException as TaskCanceledException;
                            if (ctException != null)
                            {
                                Pagination--;
                                return;
                            }
                        }

                        foreach (var movieItem in SavedMoviesDictionary.SelectMany(x => x.Value))
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

        #region Method -> StopLoadingMoviesInfos
        /// <summary>
        /// Cancel loading movies 
        /// </summary>
        public async Task StopLoadingMoviesInfos()
        {
            await Task.Run(() =>
            {
                if (CancellationLoadMoviesInfosToken != null)
                {
                    CancellationLoadMoviesInfosToken.Cancel(true);
                }
            });
        }
        #endregion

        #endregion

        #region Events

        #region Event -> MoviesInfosLoading
        /// <summary>
        /// MoviesInfosLoaded event
        /// </summary>
        public event EventHandler<EventArgs> MoviesInfosLoading;
        /// <summary>
        /// Advertise when information's movies are loading
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMoviesLoading(EventArgs e)
        {
            EventHandler<EventArgs> handler = MoviesInfosLoading;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> MoviesInfosLoaded
        /// <summary>
        /// MoviesInfosLoaded event
        /// </summary>
        public event EventHandler<EventArgs> MoviesInfosLoaded;
        /// <summary>
        /// Advertise when information's movies are finished loading
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMoviesInfosLoaded(EventArgs e)
        {
            EventHandler<EventArgs> handler = MoviesInfosLoaded;
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
