using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Models.FeaturedChat
{
    public class FirehoseMessageModel
    {
        public string Command { get; set; }
        public string Room { get; set; }
        public string Nick { get; set; }
        public string Target { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> Tags { get; set; }

    }
}
