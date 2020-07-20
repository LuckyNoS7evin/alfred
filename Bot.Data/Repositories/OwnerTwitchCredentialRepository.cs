using Bot.Core;
using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Bot.Data.Repositories
{
    internal class OwnerTwitchCredentialRepository : IOwnerTwitchCredentialRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof(OwnerTwitchCredentials).ToString();
        public OwnerTwitchCredentialRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient.GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }

        public async Task<bool> SaveAsync(OwnerTwitchCredentials ownerTwitchCredentials)
        {
            var objectToSave = new CosmosObject<OwnerTwitchCredentials>
            {
                Id = $"{_objectType}::{ownerTwitchCredentials.GuildId}::{ownerTwitchCredentials.TwitchId}",
                Item = ownerTwitchCredentials
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));

            return true;
        }
    }
}
