using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using TVShow.Helpers;
using TVShow.Model.Movie;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace TVShow.Model.Api
{
    /// <summary>
    /// Request data from YTS API
    /// </summary>
    public class Service : IService
    {
        #region Methods

        #region Method -> GetMoviesAsync
        /// <summary>
        /// Get list of movies regarding an optional search parameter, a maximum number of movies per page, a page number (for pagination) and a cancellation token
        /// </summary>
        /// <param name="searchParameter">Search parameter</param>
        /// <param name="maxMoviesPerPage">MaxMoviesPerPage</param>
        /// <param name="pageNumberToLoad">Page number to load</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>>> GetMoviesAsync(
            string searchParameter,
            int maxMoviesPerPage,
            int pageNumberToLoad,
            CancellationTokenSource cancellationToken)
        {
            var client = new RestClient(Constants.YtsApiEndpoint);
            var request = new RestRequest("/{segment}", Method.GET);
            request.AddUrlSegment("segment", "list_movies.json");

            request.AddParameter("limit", maxMoviesPerPage);
            request.AddParameter("page", pageNumberToLoad);

            if (String.IsNullOrEmpty(searchParameter))
            {
                request.AddParameter("sort_by", "like_count");
            }
            else
            {
                request.AddParameter("query_term", searchParameter);
            }

            List<Exception> ex = new List<Exception>();
            WrapperMovieShortDetails results = new WrapperMovieShortDetails();
            try
            {
                IRestResponse response = await client.ExecuteTaskAsync(request, cancellationToken.Token).ConfigureAwait(false);
                if (response != null)
                {
                    results =
                        JsonConvert.DeserializeObject<WrapperMovieShortDetails>(response.Content);
                }
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }
            catch (WebException e)
            {
                ex.Add(e);
            }
            catch (Exception e)
            {
                ex.Add(e);
            }

            if (results != null && results.Data != null && results.Data.Movies != null)
            {
                return new Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>>(results.Data.Movies, ex);
            }
            return new Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>>(null, ex);
        }
        #endregion

        #region Method -> GetMovieAsync
        /// <summary>
        /// Get the data from a movie (Torrent file url, images url, ...)
        /// </summary>
        /// <param name="movieId">The unique identifier of a movie</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<MovieFullDetails, IEnumerable<Exception>>> GetMovieAsync(int movieId,
            CancellationTokenSource cancellationToken)
        {
            var restClient = new RestClient(Constants.YtsApiEndpoint);
            var request = new RestRequest("/{segment}", Method.GET);
            request.AddUrlSegment("segment", "movie_details.json");

            request.AddParameter("movie_id", movieId);
            request.AddParameter("with_images", true);
            request.AddParameter("with_cast", true);

            List<Exception> ex = new List<Exception>();
            WrapperMovieFullDetails responseWrapper = new WrapperMovieFullDetails();
            try
            {
                IRestResponse response = await restClient.ExecuteTaskAsync(request, cancellationToken.Token).ConfigureAwait(false);
                if (response != null)
                {
                    responseWrapper =
                        JsonConvert.DeserializeObject<WrapperMovieFullDetails>(response.Content);
                }

            }
            catch (WebException webException)
            {
                ex.Add(webException);
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }

            if (responseWrapper != null && responseWrapper.Movie != null)
            {
                return new Tuple<MovieFullDetails, IEnumerable<Exception>>(responseWrapper.Movie, ex);
            }
            return new Tuple<MovieFullDetails, IEnumerable<Exception>>(null, ex);
        }
        #endregion

        #region Method -> DownloadMovieTorrentAsync
        /// <summary>
        /// Download the torrent file of a movie
        /// </summary>
        /// <param name="imdbCode">The unique identifier of a movie</param>
        /// <param name="torentUrl">The torrent URL</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieTorrentAsync(string imdbCode,
            string torentUrl,
            CancellationTokenSource cancellationToken)
        {
            List<Exception> ex = new List<Exception>();
            Tuple<string, Exception> torrentFile = new Tuple<string, Exception>(String.Empty, new Exception());

            try
            {
                torrentFile =
                    await
                        DownloadFileAsync(imdbCode,
                            new Uri(torentUrl), Constants.FileType.TorrentFile,
                            cancellationToken.Token).ConfigureAwait(false);

                if (torrentFile != null)
                {
                    if (torrentFile.Item2 == null)
                    {
                        torrentFile = new Tuple<string, Exception>(Constants.TorrentDirectory +
                                                                   imdbCode +
                                                                   ".torrent", new Exception());
                    }
                    else
                    {
                        ex.Add(torrentFile.Item2);
                    }
                }
                else
                {
                    ex.Add(new Exception());
                }
            }
            catch (WebException webException)
            {
                ex.Add(webException);
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }

            if (torrentFile != null && torrentFile.Item1 != null)
            {
                return new Tuple<string, IEnumerable<Exception>>(torrentFile.Item1, ex);
            }
            return new Tuple<string, IEnumerable<Exception>>(null, ex);
        }
        #endregion

        #region Method -> DownloadMovieCoverAsync
        /// <summary>
        /// Download the movie cover
        /// </summary>
        /// <param name="imdbCode">The unique identifier of a movie</param>
        /// <param name="imageUrl">The image URL</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieCoverAsync(string imdbCode,
            string imageUrl,
            CancellationTokenSource cancellationToken)
        {
            List<Exception> ex = new List<Exception>();
            Tuple<string, Exception> coverImage = new Tuple<string, Exception>(String.Empty, new Exception());

            try
            {
                coverImage =
                    await
                        DownloadFileAsync(imdbCode,
                            new Uri(imageUrl), Constants.FileType.CoverImage,
                            cancellationToken.Token).ConfigureAwait(false);

                if (coverImage != null)
                {
                    if (coverImage.Item2 == null)
                    {
                        coverImage = new Tuple<string, Exception>(Constants.CoverMoviesDirectory +
                                                                  imdbCode +
                                                                  Constants.ImageFileExtension, new Exception());
                    }
                    else
                    {
                        ex.Add(coverImage.Item2);
                    }
                }
                else
                {
                    ex.Add(new Exception());
                }
            }
            catch (WebException webException)
            {
                ex.Add(webException);
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }

            if (coverImage != null && coverImage.Item1 != null)
            {
                return new Tuple<string, IEnumerable<Exception>>(coverImage.Item1, ex);
            }
            return new Tuple<string, IEnumerable<Exception>>(null, ex);
        }
        #endregion

        #region Method -> DownloadMoviePosterAsync
        /// <summary>
        /// Download the movie poster
        /// </summary>
        /// <param name="imdbCode">The unique identifier of a movie</param>
        /// <param name="imageUrl">The image URL</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<string, IEnumerable<Exception>>> DownloadMoviePosterAsync(string imdbCode,
            string imageUrl,
            CancellationTokenSource cancellationToken)
        {
            List<Exception> ex = new List<Exception>();
            Tuple<string, Exception> posterImage = new Tuple<string, Exception>(String.Empty, new Exception());

            try
            {
                posterImage =
                    await
                        DownloadFileAsync(imdbCode,
                            new Uri(imageUrl), Constants.FileType.PosterImage,
                            cancellationToken.Token).ConfigureAwait(false);

                if (posterImage != null)
                {
                    if (posterImage.Item2 == null)
                    {
                        posterImage = new Tuple<string, Exception>(Constants.PosterMovieDirectory +
                                                                   imdbCode +
                                                                   Constants.ImageFileExtension, new Exception());
                    }
                    else
                    {
                        ex.Add(posterImage.Item2);
                    }
                }
                else
                {
                    ex.Add(new Exception());
                }
            }
            catch (WebException webException)
            {
                ex.Add(webException);
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }

            if (posterImage != null && posterImage.Item1 != null)
            {
                return new Tuple<string, IEnumerable<Exception>>(posterImage.Item1, ex);
            }
            return new Tuple<string, IEnumerable<Exception>>(null, ex);
        }
        #endregion

        #region Method -> DownloadDirectorImageAsync
        /// <summary>
        /// Download the directors images
        /// </summary>
        /// <param name="name">The director name</param>
        /// <param name="imageUrl">The image URL</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<string, IEnumerable<Exception>>> DownloadDirectorImageAsync(string name,
            string imageUrl,
            CancellationTokenSource cancellationToken)
        {
            List<Exception> ex = new List<Exception>();
            Tuple<string, Exception> directorImage = new Tuple<string, Exception>(String.Empty, new Exception());

            try
            {
                directorImage =
                    await
                        DownloadFileAsync(name,
                            new Uri(imageUrl), Constants.FileType.DirectorImage,
                            cancellationToken.Token).ConfigureAwait(false);

                if (directorImage != null)
                {
                    if (directorImage.Item2 == null)
                    {
                        directorImage = new Tuple<string, Exception>(Constants.DirectorMovieDirectory +
                                                                     name +
                                                                     Constants.ImageFileExtension, new Exception());
                    }
                    else
                    {
                        ex.Add(directorImage.Item2);
                    }
                }
                else
                {
                    ex.Add(new Exception());
                }
            }
            catch (WebException webException)
            {
                ex.Add(webException);
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }

            if (directorImage != null && directorImage.Item1 != null)
            {
                return new Tuple<string, IEnumerable<Exception>>(directorImage.Item1, ex);
            }
            return new Tuple<string, IEnumerable<Exception>>(null, ex);
        }
        #endregion

        #region Method -> DownloadActorImageAsync
        /// <summary>
        /// Download the actors images
        /// </summary>
        /// <param name="name">The actor name</param>
        /// <param name="imageUrl">The image URL</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<string, IEnumerable<Exception>>> DownloadActorImageAsync(string name,
            string imageUrl,
            CancellationTokenSource cancellationToken)
        {
            List<Exception> ex = new List<Exception>();
            Tuple<string, Exception> actorImage = new Tuple<string, Exception>(String.Empty, new Exception());

            try
            {
                actorImage =
                    await
                        DownloadFileAsync(name,
                            new Uri(imageUrl), Constants.FileType.ActorImage,
                            cancellationToken.Token).ConfigureAwait(false);

                if (actorImage != null)
                {
                    if (actorImage.Item2 == null)
                    {
                        actorImage = new Tuple<string, Exception>(Constants.ActorMovieDirectory +
                                                                  name +
                                                                  Constants.ImageFileExtension, new Exception());
                    }
                    else
                    {
                        ex.Add(actorImage.Item2);
                    }
                }
                else
                {
                    ex.Add(new Exception());
                }
            }
            catch (WebException webException)
            {
                ex.Add(webException);
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }

            if (actorImage != null && actorImage.Item1 != null)
            {
                return new Tuple<string, IEnumerable<Exception>>(actorImage.Item1, ex);
            }
            return new Tuple<string, IEnumerable<Exception>>(null, ex);
        }
        #endregion

        #region Method -> DownloadMovieBackgroundImageAsync
        /// <summary>
        /// Download the movie background image
        /// </summary>
        /// <param name="imdbCode">The unique identifier of a movie</param>
        /// <param name="cancellationToken">cancellationToken</param>
        public async Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieBackgroundImageAsync(string imdbCode,
            CancellationTokenSource cancellationToken)
        {
            TMDbClient tmDbclient = new TMDbClient("52db02421219a8b6b4a8eed1df0b8bd8");
            tmDbclient.GetConfig();
            TMDbLib.Objects.Movies.Movie movie = tmDbclient.GetMovie(imdbCode, MovieMethods.Images);
            Uri imageUri = tmDbclient.GetImageUrl("original",
                movie.Images.Backdrops.Aggregate((i1, i2) => i1.VoteAverage > i2.VoteAverage ? i1 : i2).FilePath);

            List<Exception> ex = new List<Exception>();
            string backgroundImage = String.Empty;
            try
            {
                Tuple<string, Exception> res =
                    await DownloadFileAsync(imdbCode, imageUri, Constants.FileType.BackgroundImage, cancellationToken.Token).ConfigureAwait(false);

                if (res != null)
                {
                    if (res.Item2 == null)
                    {
                        backgroundImage = Constants.BackgroundMovieDirectory + imdbCode + Constants.ImageFileExtension;
                    }
                    else
                    {
                        ex.Add(res.Item2);
                    }
                }
                else
                {
                    ex.Add(new Exception());
                }
            }
            catch (WebException webException)
            {
                ex.Add(webException);
            }
            catch (TaskCanceledException e)
            {
                ex.Add(e);
            }
            return new Tuple<string,IEnumerable<Exception>>(backgroundImage, ex);
        }
        #endregion

        #region Method -> DownloadFileAsync
        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="fileName">The name of the file to download</param>
        /// <param name="fileUri">Path to the file</param>
        /// <param name="fileType">The filetype</param>
        /// <param name="ct">The cancellation token</param>
        private static async Task<Tuple<string, Exception>> DownloadFileAsync(string fileName, Uri fileUri,
            Constants.FileType fileType, CancellationToken ct)
        {
            string pathDirectory = String.Empty;
            string extension = String.Empty;
            switch (fileType)
            {
                case Constants.FileType.BackgroundImage:
                    pathDirectory = Constants.BackgroundMovieDirectory;
                    extension = Constants.ImageFileExtension;
                    break;
                case Constants.FileType.CoverImage:
                    pathDirectory = Constants.CoverMoviesDirectory;
                    extension = Constants.ImageFileExtension;
                    break;
                case Constants.FileType.PosterImage:
                    pathDirectory = Constants.PosterMovieDirectory;
                    extension = Constants.ImageFileExtension;
                    break;
                case Constants.FileType.DirectorImage:
                    pathDirectory = Constants.DirectorMovieDirectory;
                    extension = Constants.ImageFileExtension;
                    break;
                case Constants.FileType.ActorImage:
                    pathDirectory = Constants.ActorMovieDirectory;
                    extension = Constants.ImageFileExtension;
                    break;
                case Constants.FileType.TorrentFile:
                    pathDirectory = Constants.TorrentDirectory;
                    extension = Constants.TorrentFileExtension;
                    break;
                default:
                    return new Tuple<string, Exception>(fileName, new Exception());
            }
            string downloadToDirectory = pathDirectory + fileName + extension;


            if (!Directory.Exists(pathDirectory))
            {
                try
                {
                    Directory.CreateDirectory(pathDirectory);
                }
                catch (Exception e)
                {
                    return new Tuple<string, Exception>(fileName, e);
                }
            }

            using (var webClient = new NoKeepAliveWebClient())
            {
                ct.Register(webClient.CancelAsync);
                if (!File.Exists(downloadToDirectory))
                {
                    try
                    {
                        await webClient.DownloadFileTaskAsync(fileUri,
                            @downloadToDirectory).ConfigureAwait(false);

                        try
                        {
                            FileInfo fi = new FileInfo(downloadToDirectory);
                            if (fi.Length == 0)
                            {
                                return new Tuple<string, Exception>(fileName, new Exception());
                            }
                        }
                        catch (Exception e)
                        {
                            return new Tuple<string, Exception>(fileName, e);
                        }

                    }
                    catch (WebException e)
                    {
                        return new Tuple<string, Exception>(fileName, e);
                    }
                    catch (Exception e)
                    {
                        return new Tuple<string, Exception>(fileName, e);
                    }
                }
                else
                {
                    try
                    {
                        FileInfo fi = new FileInfo(downloadToDirectory);
                        if (fi.Length == 0)
                        {
                            try
                            {
                                File.Delete(downloadToDirectory);
                                try
                                {
                                    await webClient.DownloadFileTaskAsync(fileUri, @downloadToDirectory).ConfigureAwait(false);

                                    FileInfo newfi = new FileInfo(downloadToDirectory);
                                    if (newfi.Length == 0)
                                    {
                                        return new Tuple<string, Exception>(fileName, new Exception());
                                    }
                                }
                                catch (WebException e)
                                {
                                    return new Tuple<string, Exception>(fileName, e);
                                }
                                catch (Exception e)
                                {
                                    return new Tuple<string, Exception>(fileName, e);
                                }
                            }
                            catch (Exception e)
                            {
                                return new Tuple<string, Exception>(fileName, e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return new Tuple<string, Exception>(fileName, e);
                    }
                }
            }

            return new Tuple<string, Exception>(fileName, null);
        }
        #endregion

        #endregion
    }    
}
