using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Models.FeaturedChat
{
    public class ThemeModel
    {
        public string Id { get; set; }
        public string Theme { get; set; }
        public string BackgroundColour { get; set; }
        public string BorderColour { get; set; }
        public string NameColour { get; set; }
        public string TextColour { get; set; }

        //Show sections
        public bool ShowBadges { get; set; }
        public bool ShowProfileImages { get; set; }

        /// <summary>
        /// This is for the "Custom"
        /// </summary>
        public bool IsCustom { get; set; }
        public string CSS { get; set; }
        public string Javascript { get; set; }
        public string Html { get; set; }
    }
}
