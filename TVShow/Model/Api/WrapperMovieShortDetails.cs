using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace TVShow.Model.Api
{
    public class WrapperMovieShortDetails : ObservableObject
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_message")]
        public string StatusMessage { get; set; }

        [JsonProperty("data")]
        public DataMovieShortDetails Data { get; set; }
    }
}
