using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using TVShow.Model.Movie;

namespace TVShow.Model.Api
{
    public class DataMovieShortDetails : ObservableObject 
    {
        [JsonProperty("movie_count")]
        public int MovieCount { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("page_nombre")]
        public int PageNumber { get; set; }

        [JsonProperty("movies")]
        public ObservableCollection<MovieShortDetails> Movies { get; set; }
    }
}
