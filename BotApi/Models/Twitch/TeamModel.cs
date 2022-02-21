using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BotApi.Models.Twitch
{
    public class TeamModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }                
        [JsonPropertyName("team_name")]
        public string Name { get; set; }
        [JsonPropertyName("team_display_name")]
        public string DisplayName { get; set; }
        [JsonPropertyName("users")]
        public List<ChannelModel> Users { get; set; }
        
    }
}
