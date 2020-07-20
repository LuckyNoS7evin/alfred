using Bot.Data.Interfaces;
using Bot.Data.Models.Config;
using Bot.Data.Repositories;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Bot.Data.Extensions
{
    public static class AddDataDependenciesExtension
    {
        public static void AddDataDependencies(this IServiceCollection services, CosmosDb configuration)
        {
            CosmosClientBuilder clientBuilder = new CosmosClientBuilder($"AccountEndpoint={configuration.Account};AccountKey={configuration.Key};");
            var client = clientBuilder.WithConnectionModeDirect().Build();
            services.AddSingleton(client);
            services.AddTransient<IOwnerTwitchCredentialRepository, OwnerTwitchCredentialRepository>();
            services.AddTransient<IGitHubCredentialRepository, GitHubCredentialRepository>();
            services.AddTransient<ITwitchTeamMemberRepository, TwitchTeamMemberRepository>();
            services.AddTransient<ITwitchTeamSettingsRepository, TwitchTeamSettingsRepository>();
            services.AddTransient<ISingleStreamerSettingsRepository, SingleStreamerSettingsRepository>();
            services.AddTransient<IGuildSettingsRepository, GuildSettingsRepository>();
            services.AddTransient<ITranscriberRepository, TranscriberRepository>();
            services.AddTransient<IFeaturedChatSettingsRepository, FeaturedChatSettingsRepository>();
        }
    }
}
