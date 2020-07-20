using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BotApi.Models.Twitch
{
    public class UsersModel
    {
        [JsonPropertyName("data")]
        public List<UserModel> Data { get; set; }
    }
}
