
using Discord.WebSocket;

namespace BotApi.DiscordNet
{
    public class BotDiscordSocketClient : DiscordSocketClient
    {
        public string InstanceName { get; set; }
    }
}
