using Bot.Core;
using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Data.Repositories
{
    internal class TwitchTeamMemberRepository : ITwitchTeamMemberRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof(TwitchTeamMember).ToString();
        public TwitchTeamMemberRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient
                .GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }

        public async Task DeleteAsync(ulong guildId, string twitchId)
        {
            var id = $"{_objectType}::{guildId}::{twitchId}";

            await _cosmosContainer.DeleteItemAsync<CosmosObject<TwitchTeamMember>>(id, new PartitionKey(_objectType));
        }

        public async Task<List<TwitchTeamMember>> GetAllTeamsAsync()
        {
            QueryDefinition query = new QueryDefinition("select * from s");
            FeedIterator<CosmosObject<TwitchTeamMember>> resultSet =
                _cosmosContainer.GetItemQueryIterator<CosmosObject<TwitchTeamMember>>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(_objectType)
                    }
                );


            var team = new List<TwitchTeamMember>();
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

        public async Task<TwitchTeamMember> GetAsync(ulong guildId, string twitchId)
        {
            var id = $"{_objectType}::{guildId}::{twitchId}";
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<CosmosObject<TwitchTeamMember>>(id, new PartitionKey(_objectType));
                return response.Resource.Item;
            }
            catch
            {
                return null;
            }
        }

        public async  Task<List<TwitchTeamMember>> GetTeamAsync(ulong guildId)
        {
            QueryDefinition query = new QueryDefinition(
               "select * from s where s.Item.GuildId = @GuildId ")
               .WithParameter("@GuildId", guildId);
            FeedIterator<CosmosObject<TwitchTeamMember>> resultSet = 
                _cosmosContainer.GetItemQueryIterator<CosmosObject<TwitchTeamMember>>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(_objectType)
                    }
                );


            var team = new List<TwitchTeamMember>();
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

        public async Task SaveAsync(TwitchTeamMember teamMember)
        {
            var objectToSave = new CosmosObject<TwitchTeamMember>
            {
                Id = $"{_objectType}::{teamMember.GuildId}::{teamMember.TwitchId}",
                Item = teamMember
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));
        }
    }
}
