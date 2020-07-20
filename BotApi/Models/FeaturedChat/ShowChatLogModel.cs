using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Models.FeaturedChat
{
    public class ShowChatLogModel : ChatLogModel
    {
        public string Transition { get; set; }
        public ThemeModel Theme { get; set; }
        public string Position { get; set; }
        public int DisplayTime { get; set; }
        public string AudioFile { get; set; }
        public double AudioVolume { get; set; }
        public UserModel User { get; set; }
    }
}
