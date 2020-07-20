using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Models.FeaturedChat
{
    public class ChatLogModel
    {
        public string Id { get; set; }
        public double CreatedAt { get; set; }
        public string ChannelId { get; set; }
        public string UserId { get; set; }
        public FirehoseMessageModel Message { get; set; }
    }
}
