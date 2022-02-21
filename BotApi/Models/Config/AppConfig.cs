namespace BotApi.Models.Config
{
    public class AppConfig
    {
        public string ApplicationInsights { get; set; }
        public GitHub GitHub { get; set; }
        public string TwitchClientId { get; set; }
        public string TwitchSecret { get; set; }
        public string TwitchRedirectUrl { get; set; }
        public string EventSubSecret { get; set; }
        public Bot[] Bots { get; set; }
    }
}
