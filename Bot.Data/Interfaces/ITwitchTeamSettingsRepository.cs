using Bot.Core;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface ITwitchTeamSettingsRepository
    {
        Task SaveAsync(TwitchTeamSettings teamsettings);
        Task<TwitchTeamSettings> GetAsync(ulong guildId);
        Task<List<TwitchTeamSettings>> GetAllAsync();
    }
}
