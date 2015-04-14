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
    /// The Main ViewModel of TVShow
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

        #region Property -> CancellationLoadMoviesToken
        /// <summary>
        /// Token to cancel the loading of movies
        /// </summary>
        private CancellationTokenSource CancellationLoadingToken { get; set; }
        #endregion

        #region Property -> IsDownloadingMovie
        private bool _isDownloadingMovie;
        /// <summary>
        /// Specify if a movie is downloading
        /// </summary>
        public bool IsDownloadingMovie
        {
            get { return _isDownloadingMovie; }
            set
            {
                if (_isDownloadingMovie != value)
                {
                    _isDownloadingMovie = value;
                    RaisePropertyChanged(Constants.IsDownloadingMoviePropertyName);
                }
            }
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

        #region Property -> TorrentSession
        /// <summary>
        /// The session of the current torrent
        /// </summary>
        private Session TorrentSession { get; set; }
        #endregion

        #region Property -> TorrentHandle
        /// <summary>
        /// The handle of the current torrent
        /// </summary>
        private TorrentHandle TorrentHandle { get; set; }
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
        public MainViewModel(IService apiService)
        {
            ApiService = apiService;
            Messenger.Default.Register<bool>(this, Constants.ConnectionErrorPropertyName, arg => OnConnectionError(new ConnectionErrorEventArgs(arg)));

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

            LoadMovieCommand = new RelayCommand<MovieShortDetails>(async movie =>
            {
                await LoadMovie(new Tuple<int, string>(movie.Id, movie.ImdbCode));
            });
        }
        #endregion

        #endregion

        #region Methods

        #region Method -> LoadMovie
        /// <summary>
        /// Load the requested movie
        /// </summary>
        /// <param name="movieCodes">The movieID and IMDb code</param>
        private async Task LoadMovie(Tuple<int, string> movieCodes)
        {
            if (Movie == null || !String.IsNullOrEmpty(Movie.Title))
            {
                Movie = new MovieFullDetails();
            }

            await GetMovie(movieCodes.Item1, movieCodes.Item2);
        }
        #endregion

        #region Method -> GetMovie
        /// <summary>
        /// Get the requested movie
        /// </summary>
        /// <param name="movieId">The movie ID</param>
        /// <param name="imdbCode">The IMDb code</param>
        private async Task GetMovie(int movieId, string imdbCode)
        {
            // Reset the CancellationToken for having the possibility to stop the process
            CancellationLoadingToken = new CancellationTokenSource();

            // Get the requested movie using the service
            Tuple<MovieFullDetails, IEnumerable<Exception>> movieInfosAsyncResults = await ApiService.GetMovieAsync(movieId,
                CancellationLoadingToken);

            // Check if we met any exception in the GetMoviesInfosAsync method
            foreach (Exception e in movieInfosAsyncResults.Item2)
            {
                HandleException(e);
                return;
            }

            // Our loaded movie is here
            Movie = movieInfosAsyncResults.Item1;

            // Inform we loaded the requested movie
            OnMovieLoaded(new EventArgs());

            // Reset the CancellationToken for having the possibility to stop downloading the movie poster
            CancellationLoadingToken = new CancellationTokenSource();

            // Download the movie poster
            Tuple<string, IEnumerable<Exception>> moviePosterAsyncResults = await ApiService.DownloadMoviePosterAsync(Movie.ImdbCode,
                Movie.Images.LargeCoverImage,
                CancellationLoadingToken);

            // Set the path to the poster image if no exception occured in the DownloadMoviePosterAsync method
            if (moviePosterAsyncResults.Item2.All(a => a == null))
            {
                Movie.PosterImage = moviePosterAsyncResults.Item1;
            }
            else
            {
                // We met an exception in DownloadMoviePosterAsync method
                foreach (Exception e in moviePosterAsyncResults.Item2)
                {
                    HandleException(e);
                    return;
                }
            }

            // Reset the CancellationToken for having the possibility to stop downloading directors images
            CancellationLoadingToken = new CancellationTokenSource();

            // For each director, we download its image
            foreach (Director director in Movie.Directors)
            {
                Tuple<string, IEnumerable<Exception>> directorsImagesAsyncResults = await ApiService.DownloadDirectorImageAsync(director.Name.Trim(),
                    director.SmallImage,
                    CancellationLoadingToken);

                // Set the path to the director image if no exception occured in the DownloadDirectorImageAsync method
                if (directorsImagesAsyncResults.Item2.All(a => a == null))
                {
                    director.SmallImagePath = directorsImagesAsyncResults.Item1;
                }
                else
                {
                    // We met an exception in DownloadDirectorImageAsync method
                    foreach (Exception e in directorsImagesAsyncResults.Item2)
                    {
                        HandleException(e);
                        return;
                    }
                }
            }

            // Reset the CancellationToken for having the possibility to stop downloading actors images
            CancellationLoadingToken = new CancellationTokenSource();

            // For each actor, we download its image
            foreach (Actor actor in Movie.Actors)
            {
                Tuple<string, IEnumerable<Exception>> actorsImagesAsyncResults = await ApiService.DownloadActorImageAsync(actor.Name.Trim(),
                    actor.SmallImage,
                    CancellationLoadingToken);

                // Set the path to the actor image if no exception occured in the DownloadActorImageAsync method
                if (actorsImagesAsyncResults.Item2.All(a => a == null))
                {
                    actor.SmallImagePath = actorsImagesAsyncResults.Item1;
                }
                else
                {
                    // We met an exception in DownloadActorImageAsync method
                    foreach (Exception e in actorsImagesAsyncResults.Item2)
                    {
                        HandleException(e);
                        return;
                    }
                }
            }

            // Reset the CancellationToken for having the possibility to stop downloading the movie background image
            CancellationLoadingToken = new CancellationTokenSource();

            Tuple<string, IEnumerable<Exception>> movieBackgroundImageResults = await ApiService.DownloadMovieBackgroundImageAsync(imdbCode, CancellationLoadingToken);

            // Set the path to the poster image if no exception occured in the DownloadMoviePosterAsync method
            if (movieBackgroundImageResults.Item2.All(a => a == null))
            {
                Movie.BackgroundImage = movieBackgroundImageResults.Item1;
            }
            else
            {
                // We met an exception in DownloadMovieBackgroundImageAsync method
                foreach (Exception e in movieBackgroundImageResults.Item2)
                {
                    HandleException(e);
                    return;
                }
            }
        }
        #endregion

        #region Method -> HandleException
        /// <summary>
        /// Handle the exception
        /// </summary>
        /// <param name="e">Exception</param>
        private static void HandleException(Exception e)
        {
            // There's a connection error. Send the message and go back.
            var we = e as WebException;
            if (we != null)
            {
                Messenger.Default.Send<bool>(true, Constants.ConnectionErrorPropertyName);
                return;
            }

            var ctException = e as TaskCanceledException;
            if (ctException != null)
            {
                // The user cancelled the loading
            }
        }
        #endregion

        #region Method -> DownloadMovie
        /// <summary>
        /// Download a movie
        /// </summary>
        /// <param name="movie">The movie to download</param>
        public async Task DownloadMovie(MovieFullDetails movie)
        {
            using (TorrentSession = new Session())
            {
                IsDownloadingMovie = true;

                // Inform subscribers we're actually loading a movie
                OnMovieLoading(new EventArgs());

                // Listening to a port which is randomly between 6881 and 6889
                TorrentSession.ListenOn(6881, 6889);

                var addParams = new AddTorrentParams
                {
                    // Where do we save the video file
                    SavePath = Constants.MovieDownloads,
                    // At this time, no quality selection is available in the interface, so we take the lowest
                    Url = movie.Torrents.Aggregate((i1, i2) => (i1.SizeBytes < i2.SizeBytes ? i1 : i2)).Url
                };

                TorrentHandle = TorrentSession.AddTorrent(addParams);
                // We have to download sequentially, so that we're able to play the movie without waiting
                TorrentHandle.SequentialDownload = true;

                bool alreadyBuffered = false;
                while (true)
                {
                    TorrentStatus status = TorrentHandle.QueryStatus();
                    double progress = status.Progress * 100.0;

                    if (status.IsFinished || !IsDownloadingMovie)
                    {
                        return;
                    }
                    
                    // Inform subscribers of our progress
                    OnMovieLoadingProgress(new MovieLoadingProgressEventArgs(progress, status.DownloadRate / 1024));

                    // We consider 2% of progress is enough to start playing
                    if (progress >= Constants.MinimumBufferingBeforeMoviePlaying && !alreadyBuffered)
                    {
                        try
                        {
                            // We're looking for video file
                            foreach (string directory in Directory.GetDirectories(Constants.MovieDownloads))
                            {
                                foreach (string filePath in Directory.GetFiles(directory, "*" + Constants.VideoFileExtension))
                                {
                                    // Inform subscribers we have finished buffering the movie
                                    OnMovieBuffered(new MovieBufferedEventArgs(filePath));
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
                    await Task.Delay(1000);
                }
            }
        }
        #endregion

        #region Method -> StopDownloadingMovie
        /// <summary>
        /// Cancel the download of a movie 
        /// </summary>
        public async Task StopDownloadingMovie()
        {
            await Task.Run(() =>
            {
                // Inform subscriber we have stopped downloading a movie
                OnMovieStoppedDownloading(new EventArgs());
                IsDownloadingMovie = false;
                // We remove the torrent (tell the tracker we're not client anymore), then remove the files
                TorrentSession.RemoveTorrent(TorrentHandle, true);
            });
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

        #region Event -> OnMovieLoadingProgress
        /// <summary>
        /// MovieLoadingProgress event
        /// </summary>
        public event EventHandler<MovieLoadingProgressEventArgs> MovieLoadingProgress;
        /// <summary>
        /// When movie is loading
        /// </summary>
        ///<param name="e">MovieLoadingProgressEventArgs parameter</param>
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
        /// MovieLoading event
        /// </summary>
        public event EventHandler<EventArgs> MovieLoading;
        /// <summary>
        /// When movie is loading
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
        protected virtual void OnMovieLoading(EventArgs e)
        {
            EventHandler<EventArgs> handler = MovieLoading;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

        #region Event -> OnMovieLoaded
        /// <summary>
        /// MovieSelected event
        /// </summary>
        public event EventHandler<EventArgs> MovieLoaded;
        /// <summary>
        /// When movie is selected
        /// </summary>
        ///<param name="e">e</param>
        protected virtual void OnMovieLoaded(EventArgs e)
        {
            EventHandler<EventArgs> handler = MovieLoaded;
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
        /// When movie is stopped downloading
        /// </summary>
        ///<param name="e">EventArgs parameter</param>
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
        /// When a movie is finished buffering
        /// </summary>
        ///<param name="e">MovieBufferedEventArgs parameter</param>
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