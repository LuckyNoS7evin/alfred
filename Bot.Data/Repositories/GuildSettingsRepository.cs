using Bot.Core;
using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Data.Repositories
{
    internal class GuildSettingsRepository : IGuildSettingsRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof(GuildSettings).ToString();
        public GuildSettingsRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient
                .GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }

        public async Task<GuildSettings> GetAsync(ulong guildId)
        {
            var id = $"{_objectType}::{guildId}";
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<CosmosObject<GuildSettings>>(id, new PartitionKey(_objectType));
                return response.Resource.Item;
            } 
            catch
            {
                return new GuildSettings
                {
                    GuildId = guildId.ToString(),
                    ModRoles = new List<string>()
                };
            }
        }

        public async Task SaveAsync(GuildSettings teamsettings)
        {
            var objectToSave = new CosmosObject<GuildSettings>
            {
                Id = $"{_objectType}::{teamsettings.GuildId}",
                Item = teamsettings
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));
        }
    }
}
