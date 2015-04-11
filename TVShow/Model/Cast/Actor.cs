using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace TVShow.Model.Cast
{
    public class Actor : ObservableObject
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("character_name")]
        public string CharacterName { get; set; }

        [JsonProperty("small_image")]
        public string SmallImage { get; set; }

        [JsonProperty("medium_image")]
        public string MediumImage { get; set; }

        private string _smallImagePath = String.Empty;
        public string SmallImagePath
        {
            get { return _smallImagePath; }
            set { Set(() => SmallImagePath, ref _smallImagePath, value); }
        }
    }
}
