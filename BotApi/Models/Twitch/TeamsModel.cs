using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotApi.Models.Twitch
{
    public class TeamsModel
    {
        [JsonPropertyName("data")]
        public List<TeamModel> Data { get; set; }
    }
}
