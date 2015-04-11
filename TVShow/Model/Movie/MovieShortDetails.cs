using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace TVShow.Model.Movie
{
    public class MovieShortDetails : ObservableObject
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("imdb_code")]
        public string ImdbCode { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_long")]
        public string TitleLong { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("runtime")]
        public int Runtime { get; set; }

        [JsonProperty("genres")]
        public IEnumerable<string> Genres { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("mpa_rating")]
        public string MpaRating { get; set; }

        [JsonProperty("small_cover_image")]
        public string SmallCoverImage { get; set; }

        [JsonProperty("medium_cover_image")]
        public string MediumCoverImage { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("torrents")]
        public ObservableCollection<Torrent.Torrent> Torrents { get; set; }

        [JsonProperty("date_uploaded")]
        public string DateUploaded { get; set; }

        [JsonProperty("date_uploaded_unix")]
        public int DateUploadedUnix { get; set; }

        [JsonProperty("server_time")]
        public int ServerTime { get; set; }

        [JsonProperty("server_timezone")]
        public string ServerTimezone { get; set; }

        [JsonProperty("api_version")]
        public int ApiVersion { get; set; }

        [JsonProperty("execution_time")]
        public string ExecutionTime { get; set; }

        private string _mediumCoverImageUri = String.Empty;
        public string MediumCoverImageUri
        {
            get { return _mediumCoverImageUri; }
            set { Set(() => MediumCoverImageUri, ref _mediumCoverImageUri, value); }
        }
    }
}
