using Bot.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface IFeaturedChatSettingsRepository
    {
        Task SaveAsync(FeaturedChatSettings teamsettings);
        Task<FeaturedChatSettings> GetAsync(ulong guildId);
        Task<List<FeaturedChatSettings>> GetAllAsync();
    }
}
