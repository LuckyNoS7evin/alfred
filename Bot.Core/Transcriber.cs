using System;

namespace Bot.Core
{
    public class Transcriber
    {
        public string GuildId { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
    }
}
