using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMDbLib.Objects.Movies;
using TVShow.Model.Movie;

namespace TVShow.Model.Api
{
    public interface IService
    {
        Task<Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>>> GetMoviesAsync(string searchParameter,
            int maxMoviesPerPage,
            int pageNumberToLoad, 
            CancellationTokenSource cancellationToken);

        Task<Tuple<MovieFullDetails, IEnumerable<Exception>>> GetMovieAsync(int movieId,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieBackgroundImageAsync(string imdbCode,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieCoverAsync(string imdbCode,
            string imageUrl, 
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadMoviePosterAsync(string imdbCode,
            string imageUrl,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadDirectorImageAsync(string name,
            string imageUrl,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadActorImageAsync(string name,
            string imageUrl,
            CancellationTokenSource cancellationToken);

        Tuple<Trailers, Exception> GetMovieTrailer(string imdbId);
    }
}
