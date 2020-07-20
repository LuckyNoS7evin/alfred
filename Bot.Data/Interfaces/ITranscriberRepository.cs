using Bot.Core;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Data.Interfaces
{
    public interface ITranscriberRepository
    {
        Task SaveAsync(Transcriber transcriber);
        // Task DeleteAsync(ulong guildId, ulong channelId);
        Task<Transcriber> GetAsync(ulong guildId, ulong channelId);
        Task<List<Transcriber>> GetAllAsync();
    }
}
