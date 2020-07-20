using Bot.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface ISingleStreamerSettingsRepository
    {
        Task SaveAsync(SingleStreamerSettings settings);
        Task<SingleStreamerSettings> GetAsync(ulong guildId);
        Task<List<SingleStreamerSettings>> GetAllAsync();
    }
}
