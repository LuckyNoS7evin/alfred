using Bot.Data.Extensions;
using Bot.Data.Models.Config;
using BotApi.DiscordNet;
using BotApi.HttpServices;
using BotApi.Models.Config;
using BotApi.Modules;
using BotApi.Services;
using Discord.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;

namespace BotApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddControllers();
            services
              .AddLogging()
              .AddSingleton<Random>()
              .AddTransient<StringCipher>()
              .AddSingleton<CommandHandler>()
              .AddSingleton<CommandService>();

            InitialiseCosmosClient(services);

            var config = Configuration.Get<AppConfig>();
            foreach (var bot in config.Bots)
            {
                services.AddSingleton(new BotDiscordSocketClient()
                {
                    InstanceName = bot.Name
                });
            }
            services.AddLogging(builder =>
            {
                builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Warning);
                builder.AddApplicationInsights(config.ApplicationInsights);
            });
            services
                .AddHostedService<TheBot>()
                .AddHostedService<FeaturedChatService>()
                .AddHostedService<TwitchTeamService>()
                .AddHostedService<StreamTeamService>()
                .AddHostedService<SingleStreamerService>()
                .Configure<AppConfig>(Configuration)
                .AddMemoryCache();
            services.AddHttpClient<TwitchService>();
            services.AddHttpClient<GitHubService>(); 
        }

        private void InitialiseCosmosClient(IServiceCollection services)
        {
            var config = Configuration.GetSection("CosmosDb").Get<CosmosDb>();
            services.Configure<CosmosDb>(Configuration.GetSection("CosmosDb"));
            services.AddDataDependencies(config);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
