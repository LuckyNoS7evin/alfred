using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotApi.Models.Twitch
{
    public class StreamsModel
    {
        [JsonPropertyName("data")]
        public List<StreamModel> Data { get; set; }
    }
}
