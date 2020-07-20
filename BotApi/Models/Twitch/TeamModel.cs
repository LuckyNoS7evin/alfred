using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BotApi.Models.Twitch
{
    public class TeamModel
    {
        [JsonPropertyName("_id")]
        public long Id { get; set; }           
        [JsonPropertyName("background")]
        public string Background { get; set; }           
        [JsonPropertyName("banner")]
        public string Banner { get; set; }        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
        [JsonPropertyName("info")]
        public string Info { get; set; }
        [JsonPropertyName("logo")]
        public string Logo { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("users")]
        public List<ChannelModel> Users { get; set; }
        
    }
}
