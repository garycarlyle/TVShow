using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TVShow.Model.Movie;

namespace TVShow.Model.Api
{
    public interface IService
    {
        Task<Tuple<IEnumerable<MovieShortDetails>, IEnumerable<Exception>>> GetMoviesInfosAsync(string searchParameter,
            int maxMoviesPerPage,
            int pageNumberToLoad, 
            CancellationTokenSource cancellationToken);

        Task<Tuple<MovieFullDetails, IEnumerable<Exception>>> GetMovieInfosAsync(int movieId,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieBackgroundImage(string imdbCode,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieCoverAsync(string imdbCode,
            string imageUrl, 
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadMoviePosterAsync(string imdbCode,
            string imageUrl,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadDirectorsImagesAsync(string name,
            string imageUrl,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadActorsImagesAsync(string name,
            string imageUrl,
            CancellationTokenSource cancellationToken);

        Task<Tuple<string, IEnumerable<Exception>>> DownloadMovieTorrent(string imdbCode, 
            string torentUrl,
            CancellationTokenSource cancellationToken);
    }
}
