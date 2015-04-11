using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using TVShow.Model.Cast;

namespace TVShow.Model.Movie
{
    public class MovieFullDetails : ObservableObject
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

        [JsonProperty("download_count")]
        public string DownloadCount { get; set; }

        [JsonProperty("like_count")]
        public string LikeCount { get; set; }

        [JsonProperty("rt_critics_score")]
        public string RtCrtiticsScore { get; set; }

        [JsonProperty("rt_critics_rating")]
        public string RtCriticsRating { get; set; }

        [JsonProperty("rt_audience_score")]
        public string RtAudienceScore { get; set; }

        [JsonProperty("rt_audience_rating")]
        public string RtAudienceRating { get; set; }

        [JsonProperty("description_intro")]
        public string DescriptionIntro { get; set; }

        [JsonProperty("description_full")]
        public string DescriptionFull { get; set; }

        [JsonProperty("yt_trailer_code")]
        public string YtTrailerCode { get; set; }

        [JsonProperty("images")]
        public MovieImages Images { get; set; }

        [JsonProperty("directors")]
        public ObservableCollection<Director> Directors { get; set; }

        [JsonProperty("actors")]
        public ObservableCollection<Actor> Actors { get; set; }

        [JsonProperty("torrents")]
        public ObservableCollection<Torrent.Torrent> Torrents { get; set; }

        [JsonProperty("date_uploaded")]
        public string DateUploaded { get; set; }

        [JsonProperty("date_uploaded_unix")]
        public int DateUploadedUnix { get; set; }

        private string _backgroundImage = String.Empty;
        public string BackgroundImage
        {
            get { return _backgroundImage; }
            set { Set(() => BackgroundImage, ref _backgroundImage, value); }
        }

        private string _posterImage = String.Empty;
        public string PosterImage
        {
            get { return _posterImage; }
            set { Set(() => PosterImage, ref _posterImage, value); }
        }
    }
}
