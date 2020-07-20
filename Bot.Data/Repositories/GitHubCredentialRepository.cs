using Bot.Core;
using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Bot.Data.Repositories
{
    internal class GitHubCredentialRepository : IGitHubCredentialRepository
    {
        private readonly Container _cosmosContainer;
        private readonly string _objectType = typeof(GitHubCredentials).ToString();
        public GitHubCredentialRepository(CosmosClient cosmosClient, IOptions<CosmosDb> cosmosDb)
        {
            _cosmosContainer = cosmosClient.GetContainer(cosmosDb.Value.Database, cosmosDb.Value.Container);
        }

        public async Task<GitHubCredentials> GetAsync(ulong userId)
        {
            var id = $"{_objectType}::{userId}";
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<CosmosObject<GitHubCredentials>>(id, new PartitionKey(_objectType));
                return response.Resource.Item;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SaveAsync(GitHubCredentials gitHubCredentials)
        {
            var id = $"{_objectType}::{gitHubCredentials.UserId}";
            var objectToSave = new CosmosObject<GitHubCredentials>
            {
                Id = id,
                Item = gitHubCredentials
            };

            await _cosmosContainer.UpsertItemAsync(objectToSave, new PartitionKey(_objectType));

            return true;
        }
    }
}
