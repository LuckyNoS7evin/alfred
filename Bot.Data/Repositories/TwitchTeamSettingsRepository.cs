using Bot.Core;
using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.Data.Repositories
{
    internal class TwitchTeamSettingsRepository : ITwitchTeamSettingsRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof(TwitchTeamSettings).ToString();
        public TwitchTeamSettingsRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient
                .GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }

        public async Task<List<TwitchTeamSettings>> GetAllAsync()
        {
            QueryDefinition query = new QueryDefinition("select * from s");
            FeedIterator<CosmosObject<TwitchTeamSettings>> resultSet =
                _cosmosContainer.GetItemQueryIterator<CosmosObject<TwitchTeamSettings>>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(_objectType)
                    }
                );


            var team = new List<TwitchTeamSettings>();
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

        public async Task<TwitchTeamSettings> GetAsync(ulong guildId)
        {
            var id = $"{_objectType}::{guildId}";
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<CosmosObject<TwitchTeamSettings>>(id, new PartitionKey(_objectType));
                return response.Resource.Item;
            } 
            catch
            {
                return null;
            }
        }

        public async Task SaveAsync(TwitchTeamSettings teamsettings)
        {
            var objectToSave = new CosmosObject<TwitchTeamSettings>
            {
                Id = $"{_objectType}::{teamsettings.GuildId}",
                Item = teamsettings
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));
        }
    }
}
