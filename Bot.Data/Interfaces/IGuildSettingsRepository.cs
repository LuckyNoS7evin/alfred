using Bot.Core;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface IGuildSettingsRepository
    {
        Task SaveAsync(GuildSettings guildSettings);
        Task<GuildSettings> GetAsync(ulong guildId);
    }
}
