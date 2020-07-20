using Bot.Core;
using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Data.Repositories
{
    internal class FeaturedChatSettingsRepository : IFeaturedChatSettingsRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof( FeaturedChatSettings).ToString();
        public FeaturedChatSettingsRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient
                .GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }

        public async Task<List< FeaturedChatSettings>> GetAllAsync()
        {
            QueryDefinition query = new QueryDefinition("select * from s");
            FeedIterator<CosmosObject< FeaturedChatSettings>> resultSet =
                _cosmosContainer.GetItemQueryIterator<CosmosObject< FeaturedChatSettings>>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(_objectType)
                    }
                );


            var team = new List< FeaturedChatSettings>();
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

        public async Task< FeaturedChatSettings> GetAsync(ulong guildId)
        {
            var id = $"{_objectType}::{guildId}";
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<CosmosObject< FeaturedChatSettings>>(id, new PartitionKey(_objectType));
                return response.Resource.Item;
            } 
            catch
            {
                return null;
            }
        }

        public async Task SaveAsync( FeaturedChatSettings teamsettings)
        {
            var objectToSave = new CosmosObject< FeaturedChatSettings>
            {
                Id = $"{_objectType}::{teamsettings.GuildId}",
                Item = teamsettings
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));
        }
    }
}
