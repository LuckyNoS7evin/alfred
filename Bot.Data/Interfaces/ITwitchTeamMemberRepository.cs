using Bot.Core;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface ITwitchTeamMemberRepository
    {
        Task SaveAsync(TwitchTeamMember teamMember);
        Task DeleteAsync(ulong guildId, string twitchId);
        Task<TwitchTeamMember> GetAsync(ulong guildId, string twitchId);
        Task<List<TwitchTeamMember>> GetTeamAsync(ulong guildId);
        Task<List<TwitchTeamMember>> GetAllTeamsAsync();
    }
}
