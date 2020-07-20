using System.Text.Json.Serialization;

namespace BotApi.Models.Twitch
{
    public class IdentityRefreshModel
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
