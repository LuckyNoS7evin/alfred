using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Core
{
    public class GuildSettings
    {
        public string GuildId { get; set; }
        public List<string> ModRoles { get; set; }
        public string GitHubInstallationID { get; set; }
    }
}
