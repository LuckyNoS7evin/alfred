using Bot.Core;
using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Data.Repositories
{
    internal class SingleStreamerSettingsRepository : ISingleStreamerSettingsRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof(SingleStreamerSettings).ToString();
        public SingleStreamerSettingsRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient
                .GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }

        public async Task<List<SingleStreamerSettings>> GetAllAsync()
        {
            QueryDefinition query = new QueryDefinition("select * from s");
            FeedIterator<CosmosObject<SingleStreamerSettings>> resultSet =
                _cosmosContainer.GetItemQueryIterator<CosmosObject<SingleStreamerSettings>>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(_objectType)
                    }
                );


            var team = new List<SingleStreamerSettings>();
            while (resultSet.HasMoreResults)
            {
                var currentResultSet = await resultSet.ReadNextAsync();
                foreach (var member in currentResultSet)
                {
                    team.Add(member.Item);
                }
            }
            return team;
        }

        public async Task<SingleStreamerSettings> GetAsync(ulong guildId)
        {
            var id = $"{_objectType}::{guildId}";
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<CosmosObject<SingleStreamerSettings>>(id, new PartitionKey(_objectType));
                return response.Resource.Item;
            } 
            catch
            {
                return null;
            }
        }

        public async Task SaveAsync(SingleStreamerSettings teamsettings)
        {
            var objectToSave = new CosmosObject<SingleStreamerSettings>
            {
                Id = $"{_objectType}::{teamsettings.GuildId}",
                Item = teamsettings
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));
        }
    }
}
