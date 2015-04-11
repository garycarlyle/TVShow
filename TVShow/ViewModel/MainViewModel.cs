using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Ragnar;
using TVShow.Helpers;
using TVShow.Model.Api;
using TVShow.Model.Cast;
using TVShow.Model.Movie;
using TVShow.Events;
using GalaSoft.MvvmLight.Command;

namespace TVShow.ViewModel
{
    /// <summary>
    /// The Main ViewModel of TVShow
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        #region Property -> ApiService
        /// <summary>
        /// Get or Set the ApiService
        /// </summary>
        private IService ApiService { get; set; }
        #endregion

        #region Property -> Movie
        /// <summary>
        /// The movie on witch be based future actions (playing, data display, ...)
        /// </summary>
        private MovieFullDetails _movie = new MovieFullDetails();
        public MovieFullDetails Movie
        {
            get { return _movie; }
            set { Set(() => Movie, ref _movie, value, true); }
        }
        #endregion

        #region Property -> CancellationLoadMoviesToken
        /// <summary>
        /// Token to cancel loading movies
        /// </summary>
        private CancellationTokenSource CancellationLoadMoviesToken { get; set; }
        #endregion

        #region Property -> IsDownloadingMovie

        public const string IsDownloadingMoviePropertyName = "IsDownloadingMovie";
        private bool _isDownloadingMovie;
        public bool IsDownloadingMovie
        {
            get { return _isDownloadingMovie; }
            set
            {
                if (_isDownloadingMovie != value)
                {
                    _isDownloadingMovie = value;
                    RaisePropertyChanged(IsDownloadingMoviePropertyName);
                }
            }
        }

        #endregion

        #region Property -> IsConnexionInError

        private bool _isConnexionInError;
        public bool IsConnexionInError
        {
            get { return _isConnexionInError; }
            set { Set(() => IsConnexionInError, ref _isConnexionInError, value, true); }
        }

        #endregion

        #region Property -> TorrentSession
        private Session TorrentSession { get; set; }
        #endregion

        #region Property -> TorrentHandleCurrentMovie
        private TorrentHandle TorrentHandleCurrentMovie { get; set; }
        #endregion

        #endregion

        #region Commands
        #region Command -> StopDownloadingMovieCommand

        public RelayCommand StopDownloadingMovieCommand
        {
            get; 
            private set;
        }
        #endregion

        #region Command -> DownloadMovieCommand

        public RelayCommand DownloadMovieCommand
        {
            get;
            private set;
        }
        #endregion

        #region Command -> OpenMovieFlyoutCommand

        public RelayCommand<MovieShortDetails> OpenMovieFlyoutCommand
        {
            get;
            private set;
        }
        #endregion
        #endregion

        #region Constructors

        #region Constructor -> MainViewModel
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
            : this(new Service())
        {
        }
        #endregion

        #region Constructor -> MainViewModel
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        /// <param name="apiService">apiService</param>
        public MainViewModel(IService apiService)
        {
            ApiService = apiService;
            Messenger.Default.Register<bool>(this, "IsConnexionInError", (e) => OnConnexionInError(new ConnexionErrorEventArgs(e)));

            StopDownloadingMovieCommand = new RelayCommand(async () =>
            {
                if (IsDownloadingMovie)
                {
                    await StopDownloadingMovie();
                }
            });

            DownloadMovieCommand = new RelayCommand(async () =>
            {
                if (Movie != null && !IsDownloadingMovie)
                {
                    await DownloadMovie(Movie);
                }
            });

            OpenMovieFlyoutCommand = new RelayCommand<MovieShortDetails>(async (s) =>
            {
                await OpenMovie(new Tuple<int, string>(s.Id, s.ImdbCode));
            });
        }
        #endregion

        #endregion

        #region Methods
        #region Method -> OpenMovie
        /// <summary>
        /// Loads the requested movie
        /// </summary>
        /// <param name="movieCodes">movieCodes</param>
        private async Task OpenMovie(Tuple<int, string> movieCodes)
        {
            if (Movie == null || !String.IsNullOrEmpty(Movie.Title))
            {
                Movie = new MovieFullDetails();
            }

            await GetMovieInfos(movieCodes.Item1, movieCodes.Item2);
        }
        #endregion

        #region Method -> GetMovieInfos
        /// <summary>
        /// Get the informations of the requested movie
        /// </summary>
        /// <param name="movieId">movieId</param>
        /// <param name="imdbCode">imdbCode</param>
        private async Task GetMovieInfos(int movieId, string imdbCode)
        {
            CancellationLoadMoviesToken = new CancellationTokenSource();
            Tuple<MovieFullDetails, IEnumerable<Exception>> MovieInfosAsyncResults = await ApiService.GetMovieInfosAsync(movieId,
                CancellationLoadMoviesToken);

            foreach (Exception e in MovieInfosAsyncResults.Item2)
            {
                var we = e as WebException;
                if (we != null)
                {
                    if (we.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        Messenger.Default.Send<bool>(true, "IsConnexionInError");
                        return;
                    }
                }

                var ctException = e as TaskCanceledException;
                if (ctException != null)
                {
                    return;
                }
            }

            Movie = MovieInfosAsyncResults.Item1;
            EventArgs args = new EventArgs();
            OnMovieSelected(args);

            CancellationLoadMoviesToken = new CancellationTokenSource();

            Tuple<string, IEnumerable<Exception>> moviePosterAsyncResults = await ApiService.DownloadMoviePosterAsync(Movie.ImdbCode,
                Movie.Images.LargeCoverImage,
                CancellationLoadMoviesToken);

            if (moviePosterAsyncResults.Item2.All(a => a == null))
            {
                Movie.PosterImage = Constants.PosterMovieDirectory +
                                                    Movie.ImdbCode +
                                                    ".jpg";
            }
            else
            {
                foreach (Exception e in moviePosterAsyncResults.Item2)
                {
                    var we = e as WebException;
                    if (we != null)
                    {
                        if (we.Status == WebExceptionStatus.NameResolutionFailure)
                        {
                            Messenger.Default.Send<bool>(true, "IsConnexionInError");
                            return;
                        }
                    }

                    var ctException = e as TaskCanceledException;
                    if (ctException != null)
                    {
                        return;
                    }
                }
            }

            CancellationLoadMoviesToken = new CancellationTokenSource();
            foreach (Director director in Movie.Directors)
            {
                Tuple<string, IEnumerable<Exception>> directorsImagesAsyncResults = await ApiService.DownloadDirectorsImagesAsync(director.Name.Trim(),
                    director.SmallImage,
                    CancellationLoadMoviesToken);

                if (directorsImagesAsyncResults.Item2.All(a => a == null))
                {
                    director.SmallImagePath = Constants.DirectorMovieDirectory +
                                              director.Name.Trim() +
                                              ".jpg";
                }
                else
                {
                    foreach (Exception e in directorsImagesAsyncResults.Item2)
                    {
                        var we = e as WebException;
                        if (we != null)
                        {
                            if (we.Status == WebExceptionStatus.NameResolutionFailure)
                            {
                                Messenger.Default.Send<bool>(true, "IsConnexionInError");
                                return;
                            }
                        }

                        var ctException = e as TaskCanceledException;
                        if (ctException != null)
                        {
                            return;
                        }
                    }
                }
            }

            CancellationLoadMoviesToken = new CancellationTokenSource();
            foreach (Actor actor in Movie.Actors)
            {
                Tuple<string, IEnumerable<Exception>> actorsImagesAsyncResults = await ApiService.DownloadActorsImagesAsync(actor.Name.Trim(),
                    actor.SmallImage,
                    CancellationLoadMoviesToken);

                if (actorsImagesAsyncResults.Item2.All(a => a == null))
                {
                    actor.SmallImagePath = Constants.ActorMovieDirectory +
                                              actor.Name.Trim() +
                                              ".jpg";
                }
                else
                {
                    foreach (Exception e in actorsImagesAsyncResults.Item2)
                    {
                        var we = e as WebException;
                        if (we != null)
                        {
                            if (we.Status == WebExceptionStatus.NameResolutionFailure)
                            {
                                Messenger.Default.Send<bool>(true, "IsConnexionInError");
                                return;
                            }
                        }

                        var ctException = e as TaskCanceledException;
                        if (ctException != null)
                        {
                            return;
                        }
                    }
                }
            }

            CancellationLoadMoviesToken = new CancellationTokenSource();
            Tuple<string, IEnumerable<Exception>> movieBackgroundImageResults = await ApiService.DownloadMovieBackgroundImage(imdbCode,
                CancellationLoadMoviesToken);

            foreach (Exception e in movieBackgroundImageResults.Item2)
            {
                var we = e as WebException;
                if (we != null)
                {
                    if (we.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        Messenger.Default.Send<bool>(true, "IsConnexionInError");
                        return;
                    }
                }

                var ctException = e as TaskCanceledException;
                if (ctException != null)
                {
                    return;
                }
            }

            Movie.BackgroundImage = movieBackgroundImageResults.Item1;
        }
        #endregion

        #region Method -> DownloadMovie
        /// <summary>
        /// Download a movie
        /// </summary>
        public async Task DownloadMovie(MovieFullDetails movie)
        {
            using (TorrentSession = new Session())
            {
                IsDownloadingMovie = true;
                EventArgs onMovieLoadingArgs = new EventArgs();
                OnMovieLoading(onMovieLoadingArgs);

                TorrentSession.ListenOn(6881, 6889);

                var addParams = new AddTorrentParams
                {
                    SavePath = Constants.MovieDownloads,
                    Url = movie.Torrents.Aggregate((i1, i2) => (i1.SizeBytes < i2.SizeBytes ? i1 : i2)).Url
                };

                TorrentHandleCurrentMovie = TorrentSession.AddTorrent(addParams);
                TorrentHandleCurrentMovie.SequentialDownload = true;

                bool alreadyBuffered = false;
                while (true)
                {
                    TorrentStatus status = TorrentHandleCurrentMovie.QueryStatus();

                    if (status.IsSeeding || !IsDownloadingMovie)
                    {
                        break;
                    }
                    // Print our progress and sleep for a bit.
                    MovieLoadingProgressEventArgs onMovieLoadingProgress =
                        new MovieLoadingProgressEventArgs(status.Progress*100, status.DownloadRate/1024);
                    OnMovieLoadingProgress(onMovieLoadingProgress);
                    if (status.Progress * 100 >= 2 && !alreadyBuffered)
                    {
                        try
                        {
                            foreach (string d in Directory.GetDirectories(Constants.MovieDownloads))
                            {
                                foreach (string f in Directory.GetFiles(d, "*.mp4"))
                                {
                                    MovieBufferedEventArgs args = new MovieBufferedEventArgs(f);
                                    OnMovieBuffered(args);
                                    alreadyBuffered = true;
                                }
                            }
                        }
                        catch (System.Exception excpt)
                        {
                            Console.WriteLine(excpt.Message);
                        }
                    }
                    await Task.Delay(1000);
                }
            }
        }
        #endregion

        #region Method -> StopDownloadingMovie

        /// <summary>
        /// Cancel loading movies 
        /// </summary>
        public async Task StopDownloadingMovie()
        {
            await Task.Run(() =>
            {
                EventArgs onMovieStoppedDownloading = new EventArgs();
                OnMovieStoppedDownloading(onMovieStoppedDownloading);
                IsDownloadingMovie = false;
                TorrentSession.RemoveTorrent(TorrentHandleCurrentMovie, true);
            });
        }

        #endregion

        #endregion        

        #region Events

        #region Event -> ConnexionInErrorEvent
        /// <summary>
        /// ConnexionInErrorEvent event
        /// </summary>
        public event EventHandler<ConnexionErrorEventArgs> ConnexionInErrorEvent;
        /// <summary>
        /// When ConnexionInErrorEvent event is fired
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnConnexionInError(ConnexionErrorEventArgs e)
        {
            EventHandler<ConnexionErrorEventArgs> handler = ConnexionInErrorEvent;
            if (handler != null)
            {
                if (e.IsInError)
                {
                    IsConnexionInError = true;
                }
                else
                {
                    IsConnexionInError = false;
                }
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnMovieLoadingProgress
        /// <summary>
        /// OnMovieLoadingProgress event
        /// </summary>
        public event EventHandler<MovieLoadingProgressEventArgs> MovieLoadingProgress;
        /// <summary>
        /// Advertise when movie is loading
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMovieLoadingProgress(MovieLoadingProgressEventArgs e)
        {
            EventHandler<MovieLoadingProgressEventArgs> handler = MovieLoadingProgress;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnMovieLoading
        /// <summary>
        /// OnMovieLoading event
        /// </summary>
        public event EventHandler<EventArgs> MovieLoading;
        /// <summary>
        /// Advertise when movie is loading
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMovieLoading(EventArgs e)
        {
            EventHandler<EventArgs> handler = MovieLoading;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnMovieSelected
        /// <summary>
        /// MovieSelected event
        /// </summary>
        public event EventHandler<EventArgs> MovieSelected;
        /// <summary>
        /// Advertise when movie is selected
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMovieSelected(EventArgs e)
        {
            EventHandler<EventArgs> handler = MovieSelected;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnMovieStoppedDownloading
        /// <summary>
        /// MovieStoppedDownloading event
        /// </summary>
        public event EventHandler<EventArgs> MovieStoppedDownloading;
        /// <summary>
        /// Advertise when movie is stopped downloading
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMovieStoppedDownloading(EventArgs e)
        {
            EventHandler<EventArgs> handler = MovieStoppedDownloading;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnMovieBuffered
        /// <summary>
        /// MovieBuffered event
        /// </summary>
        public event EventHandler<MovieBufferedEventArgs> MovieBuffered;
        /// <summary>
        /// Advertise when a movie is finished buffering
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMovieBuffered(MovieBufferedEventArgs e)
        {
            EventHandler<MovieBufferedEventArgs> handler = MovieBuffered;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #endregion

        public override void Cleanup()
        {
            Messenger.Default.Unregister<string>(this);
            base.Cleanup();
        }
    }
}