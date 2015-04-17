using System.Collections.Generic;
using TVShow.Model.Movie;

namespace TVShow.Comparers
{
    /// <summary>
    /// Compare two movies
    /// </summary>
    public class MovieComparer : IEqualityComparer<MovieShortDetails>
    {
        /// <summary>
        /// Compare two movies
        /// </summary>
        /// <param name="movie1">First movie</param>
        /// <param name="movie2">Second movie</param>
        public bool Equals(MovieShortDetails movie1, MovieShortDetails movie2)
        {
            if (movie1.Id == movie2.Id && movie1.DateUploadedUnix == movie2.DateUploadedUnix)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Define a unique hash code
        /// </summary>
        /// <param name="movie">A movie</param>
        public int GetHashCode(MovieShortDetails movie)
        {
            int hCode = movie.Id ^ movie.DateUploadedUnix;
            return hCode.GetHashCode();
        }
    }
}
