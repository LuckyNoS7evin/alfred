using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BotApi.Models.Twitch
{
    public class ChannelModel
    {
        [JsonPropertyName("user_id")]
        public string Id { get; set; }
        [JsonPropertyName("user_name")]
        public string UserName { get; set; }
        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; }
    }
}
