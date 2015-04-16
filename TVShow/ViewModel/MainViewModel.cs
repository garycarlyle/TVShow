using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    /// ViewModel which takes care of a movie (retrieving infos from API and torrent downloading)
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        #region Property -> ApiService
        /// <summary>
        /// The service used to consume APIs
        /// </summary>
        private IService ApiService { get; set; }
        #endregion

        #region Property -> Movie
        /// <summary>
        /// The movie to play, retrieved from YTS API
        /// </summary>
        private MovieFullDetails _movie = new MovieFullDetails();
        public MovieFullDetails Movie
        {
            get { return _movie; }
            set { Set(() => Movie, ref _movie, value, true); }
        }
        #endregion

        #region Property -> CancellationLoadingToken
        /// <summary>
        /// Token to cancel the loading of movies
        /// </summary>
        private CancellationTokenSource CancellationLoadingToken { get; set; }
        #endregion

        #region Property -> CancellationDownloadingToken
        /// <summary>
        /// Token to cancel the downloading of a movie
        /// </summary>
        public CancellationTokenSource CancellationDownloadingToken { get; set; }
        #endregion

        #region Property -> IsDownloadingMovie
        private bool _isDownloadingMovie;
        /// <summary>
        /// Specify if a movie is downloading
        /// </summary>
        public bool IsDownloadingMovie
        {
            get { return _isDownloadingMovie; }
            set { Set(() => IsDownloadingMovie, ref _isDownloadingMovie, value, true); }
        }
        #endregion

        #region Property -> IsConnectionInError
        private bool _isConnectionInError;
        /// <summary>
        /// Specify if a connection error has occured
        /// </summary>
        public bool IsConnectionInError
        {
            get { return _isConnectionInError; }
            set { Set(() => IsConnectionInError, ref _isConnectionInError, value, true); }
        }

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

        #region Command -> LoadMovieCommand
        public RelayCommand<MovieShortDetails> LoadMovieCommand
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
        /// <param name="apiService">The service which will be used</param>
        private MainViewModel(IService apiService)
        {
            ApiService = apiService;

            // Set the CancellationToken for having the possibility to stop a task
            CancellationLoadingToken = new CancellationTokenSource();

            CancellationDownloadingToken = new CancellationTokenSource();

            Messenger.Default.Register<bool>(this, Constants.ConnectionErrorPropertyName, arg => OnConnectionError(new ConnectionErrorEventArgs(arg)));

            StopDownloadingMovieCommand = new RelayCommand(() =>
            {
                StopDownloadingMovie();
            });

            DownloadMovieCommand = new RelayCommand(async () =>
            {
                if (Movie != null && !IsDownloadingMovie)
                {
                    CancellationDownloadingToken = new CancellationTokenSource();
                    await DownloadMovie(Movie);
                }
            });

            LoadMovieCommand = new RelayCommand<MovieShortDetails>(async movie =>
            {
                await LoadMovie(movie.Id, movie.ImdbCode);
            });
        }
        #endregion

        #endregion

        #region Methods

        #region Method -> LoadMovie
        /// <summary>
        /// Get the requested movie
        /// </summary>
        /// <param name="movieId">The movie ID</param>
        /// <param name="imdbCode">The IMDb code</param>
        private async Task LoadMovie(int movieId, string imdbCode)
        {
            Movie = new MovieFullDetails();
            OnLoadingMovie(new EventArgs());

            // Get the requested movie using the service
            Tuple<MovieFullDetails, IEnumerable<Exception>> movie =
                await ApiService.GetMovieAsync(movieId,
                    CancellationLoadingToken);

            // Check if we met any exception in the GetMoviesInfosAsync method
            if (HandleExceptions(movie.Item2))
            {
                // Inform we loaded the requested movie
                OnLoadedMovie(new EventArgs());
                return;
            }

            // Our loaded movie is here
            Movie = movie.Item1;

            // Inform we loaded the requested movie
            OnLoadedMovie(new EventArgs());

            // Download the movie poster
            Tuple<string, IEnumerable<Exception>> moviePosterAsyncResults =
                await ApiService.DownloadMoviePosterAsync(Movie.ImdbCode,
                    Movie.Images.LargeCoverImage,
                    CancellationLoadingToken);

            // Set the path to the poster image if no exception occured in the DownloadMoviePosterAsync method

            if (!HandleExceptions(moviePosterAsyncResults.Item2))
                Movie.PosterImage = moviePosterAsyncResults.Item1;

            // For each director, we download its image
            foreach (Director director in Movie.Directors)
            {
                Tuple<string, IEnumerable<Exception>> directorsImagesAsyncResults =
                    await ApiService.DownloadDirectorImageAsync(director.Name.Trim(),
                        director.SmallImage,
                        CancellationLoadingToken);

                // Set the path to the director image if no exception occured in the DownloadDirectorImageAsync method
                if (!HandleExceptions(directorsImagesAsyncResults.Item2))
                    director.SmallImagePath = directorsImagesAsyncResults.Item1;
            }

            // For each actor, we download its image
            foreach (Actor actor in Movie.Actors)
            {
                Tuple<string, IEnumerable<Exception>> actorsImagesAsyncResults =
                    await ApiService.DownloadActorImageAsync(actor.Name.Trim(),
                        actor.SmallImage,
                        CancellationLoadingToken);

                // Set the path to the actor image if no exception occured in the DownloadActorImageAsync method
                if (!HandleExceptions(actorsImagesAsyncResults.Item2))
                    actor.SmallImagePath = actorsImagesAsyncResults.Item1;
            }

            Tuple<string, IEnumerable<Exception>> movieBackgroundImageResults =
                await ApiService.DownloadMovieBackgroundImageAsync(imdbCode, CancellationLoadingToken);

            // Set the path to the poster image if no exception occured in the DownloadMoviePosterAsync method
            if (!HandleExceptions(movieBackgroundImageResults.Item2))
                Movie.BackgroundImage = movieBackgroundImageResults.Item1;
        }
        #endregion

        #region Method -> HandleExceptions
        /// <summary>
        /// Handle list of exceptions
        /// </summary>
        /// <param name="exceptions">List of exceptions</param>
        private static bool HandleExceptions(IEnumerable<Exception> exceptions)
        {
            foreach (var e in exceptions)
            {
                var taskCancelledException = e as TaskCanceledException;
                if (taskCancelledException != null)
                {
                    // Something as cancelled the loading. We go back.
                    return true;
                }

                var webException = e as WebException;
                if (webException != null)
                {
                    if (webException.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        // There's a connection error.
                        Messenger.Default.Send<bool>(true, Constants.ConnectionErrorPropertyName);
                        return true;
                    }
                }

                // Another exception has occured. Go back.
                return true;
            }
            return false;
        }
        #endregion

        #region Method -> DownloadMovie
        /// <summary>
        /// Download a movie
        /// </summary>
        /// <param name="movie">The movie to download</param>
        private async Task DownloadMovie(MovieFullDetails movie)
        {
            using (Session session = new Session())
            {
                IsDownloadingMovie = true;

                // Inform subscribers we're actually loading a movie
                OnDownloadingMovie(new EventArgs());

                // Listening to a port which is randomly between 6881 and 6889
                session.ListenOn(6881, 6889);

                var addParams = new AddTorrentParams
                {
                    // Where do we save the video file
                    SavePath = Constants.MovieDownloads,
                    // At this time, no quality selection is available in the interface, so we take the lowest
                    Url = movie.Torrents.Aggregate((i1, i2) => (i1.SizeBytes < i2.SizeBytes ? i1 : i2)).Url
                };

                TorrentHandle handle = session.AddTorrent(addParams);
                // We have to download sequentially, so that we're able to play the movie without waiting
                handle.SequentialDownload = true;

                bool alreadyBuffered = false;

                while (IsDownloadingMovie)
                {
                    TorrentStatus status = handle.QueryStatus();
                    double progress = status.Progress*100.0;

                    // Inform subscribers of our progress
                    OnLoadingMovieProgress(new MovieLoadingProgressEventArgs(progress, status.DownloadRate/1024));

                    // We consider 2% of progress is enough to start playing
                    if (progress >= Constants.MinimumBufferingBeforeMoviePlaying && !alreadyBuffered)
                    {
                        try
                        {
                            // We're looking for video file
                            foreach (string directory in Directory.GetDirectories(Constants.MovieDownloads))
                            {
                                foreach (
                                    string filePath in
                                        Directory.GetFiles(directory, "*" + Constants.VideoFileExtension)
                                    )
                                {
                                    // Inform subscribers we have finished buffering the movie
                                    OnBufferedMovie(new MovieBufferedEventArgs(filePath));
                                    alreadyBuffered = true;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    // Let sleep for a second before updating the torrent status
                    await Task.Delay(1000, CancellationDownloadingToken.Token).ContinueWith(_ =>
                    {
                        if (CancellationDownloadingToken.IsCancellationRequested && session != null)
                        {
                            OnStoppedDownloadingMovie(new EventArgs());
                            IsDownloadingMovie = false;
                            session.RemoveTorrent(handle, true);
                        }
                    }).ConfigureAwait(false);
                }
            }
        }
        #endregion

        #region Method -> StopDownloadingMovie
        /// <summary>
        /// Stop downloading a movie
        /// </summary>
        public void StopDownloadingMovie()
        {
            if (CancellationDownloadingToken != null)
            {
                CancellationDownloadingToken.Cancel();
            }
        }
        #endregion  
      
        #endregion

        #region Events

        #region Event -> OnConnectionError
        /// <summary>
        /// ConnectionErrorEvent event
        /// </summary>
        public event EventHandler<ConnectionErrorEventArgs> ConnectionError;
        /// <summary>
        /// On connection error
        /// </summary>
        ///<param name="e">ConnectionErrorEventArgs parameter</param>
        protected virtual void OnConnectionError(ConnectionErrorEventArgs e)
        {
            EventHandler<ConnectionErrorEventArgs> handler = ConnectionError;
            if (handler != null)
            {
                if (e != null && e.IsInError)
                {
                    IsConnectionInError = true;
                }
                else
                {
                    IsConnectionInError = false;
                }
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnLoadingMovieProgress
        /// <summary>
        /// MovieLoadingProgress event
        /// </summary>
        public event EventHandler<MovieLoadingProgressEventArgs> LoadingMovieProgress;
        /// <summary>
        /// When movie is loading
        /// </summary>
        ///<param name="e">MovieLoadingProgressEventArgs parameter</param>
        protected virtual void OnLoadingMovieProgress(MovieLoadingProgressEventArgs e)
        {
            EventHandler<MovieLoadingProgressEventArgs> handler = LoadingMovieProgress;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnDownloadingMovie
        /// <summary>
        /// DownloadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> DownloadingMovie;
        /// <summary>
        /// When movie is downloading
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
        protected virtual void OnDownloadingMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = DownloadingMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnLoadingMovie
        /// <summary>
        /// LoadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> LoadingMovie;
        /// <summary>
        /// When movie is selected
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnLoadingMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = LoadingMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnLoadedMovie
        /// <summary>
        /// LoadedMovie event
        /// </summary>
        public event EventHandler<EventArgs> LoadedMovie;
        /// <summary>
        /// When movie is selected
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnLoadedMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = LoadedMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnStoppedDownloadingMovie
        /// <summary>
        /// StoppedDownloadingMovie event
        /// </summary>
        public event EventHandler<EventArgs> StoppedDownloadingMovie;
        /// <summary>
        /// When movie is stopped downloading
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
        protected virtual void OnStoppedDownloadingMovie(EventArgs e)
        {
            EventHandler<EventArgs> handler = StoppedDownloadingMovie;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnBufferedMovie
        /// <summary>
        /// BufferedMovie event
        /// </summary>
        public event EventHandler<MovieBufferedEventArgs> BufferedMovie;
        /// <summary>
        /// When a movie is finished buffering
        /// </summary>
        ///<param name="e">MovieBufferedEventArgs parameter</param>
        protected virtual void OnBufferedMovie(MovieBufferedEventArgs e)
        {
            EventHandler<MovieBufferedEventArgs> handler = BufferedMovie;
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