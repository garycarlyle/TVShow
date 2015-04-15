using System.Collections.Generic;
using TVShow.Model.Movie;

namespace TVShow.Comparers
{    
    public class MovieComparer : IEqualityComparer<MovieShortDetails>
    {
        public bool Equals(MovieShortDetails movie1, MovieShortDetails movie2)
        {
            if (movie1.Id == movie2.Id && movie1.DateUploadedUnix == movie2.DateUploadedUnix)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(MovieShortDetails movie)
        {
            int hCode = movie.Id ^ movie.DateUploadedUnix;
            return hCode.GetHashCode();
        }
    }
}
