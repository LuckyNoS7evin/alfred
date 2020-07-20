using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Core
{
    public class OwnerTwitchCredentials
    {
        public string TwitchId { get; set; }
        public string GuildId { get; set; }
        public string RefreshToken { get; set; }
        public int Expiry { get; set; }
    }
}
