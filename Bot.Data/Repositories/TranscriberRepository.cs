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
    internal class TranscriberRepository : ITranscriberRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof(Transcriber).ToString();
        public TranscriberRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient
                .GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }


        public async Task<List<Transcriber>> GetAllAsync()
        {
            QueryDefinition query = new QueryDefinition("select * from s");
            FeedIterator<CosmosObject<Transcriber>> resultSet =
                _cosmosContainer.GetItemQueryIterator<CosmosObject<Transcriber>>(
                    query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(_objectType)
                    }
                );


            var team = new List<Transcriber>();
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

        public async Task<Transcriber> GetAsync(ulong guildId, ulong channelId)
        {
            var id = $"{_objectType}::{guildId}::{channelId}";
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<CosmosObject<Transcriber>>(id, new PartitionKey(_objectType));
                return response.Resource.Item;
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveAsync(Transcriber transcriber)
        {
            var objectToSave = new CosmosObject<Transcriber>
            {
                Id = $"{_objectType}::{transcriber.GuildId}::{transcriber.ChannelId}",
                Item = transcriber
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));
        }
    }
}
