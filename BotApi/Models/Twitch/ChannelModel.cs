using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BotApi.Models.Twitch
{
    public class ChannelModel
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("broadcaster_language")]
        public string BroadcasterLanguage { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        [JsonPropertyName("followers")]
        public int Followers { get; set; }
        [JsonPropertyName("broadcaster_type")]
        public string BroadcasterType { get; set; }
        [JsonPropertyName("game")]
        public string Game { get; set; }
        [JsonPropertyName("language")]
        public string Language { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("mature")]
        public bool Mature { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("partner")]
        public bool Partner { get; set; }
        [JsonPropertyName("profile_banner")]
        public string ProfileBanner { get; set; }
        [JsonPropertyName("profile_banner_background_color")]
        public string ProfileBannerBackgroundColor { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("video_banner")]
        public string VideoBanner { get; set; }
        [JsonPropertyName("views")]
        public int Views { get; set; }
    }
}
