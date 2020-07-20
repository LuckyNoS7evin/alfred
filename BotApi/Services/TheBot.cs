using BotApi.DiscordNet;
using BotApi.Models.Config;
using BotApi.Modules;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApi.Services
{
    public class TheBot : IHostedService, IAsyncDisposable
    {
        private readonly AppConfig _config;
        private readonly IEnumerable<BotDiscordSocketClient> _botDiscordSocketClients;
        private readonly CommandHandler _commandHandler;
        private readonly ConcurrentDictionary<ulong, DateTime> _tableflip;
        private readonly ILogger<TheBot> _logger;
        public TheBot(IOptions<AppConfig> config, 
            IEnumerable<BotDiscordSocketClient> botDiscordSocketClients,
            CommandHandler commandHandler,
            ILogger<TheBot> logger)
        {
            _config = config.Value;
            _botDiscordSocketClients = botDiscordSocketClients;
            _commandHandler = commandHandler;
            _tableflip = new ConcurrentDictionary<ulong, DateTime>();
            _logger = logger;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _commandHandler.InstallCommandsAsync();
            foreach (var bot in _botDiscordSocketClients)
            {
                var botConfig = _config.Bots.FirstOrDefault(x => x.Name == bot.InstanceName);
                await bot.LoginAsync(Discord.TokenType.Bot, botConfig.DiscordToken);
                await bot.StartAsync();
                bot.MessageReceived += Bot_MessageReceived;
            }
            _logger.LogInformation("started");
        }

        private async Task Bot_MessageReceived(Discord.WebSocket.SocketMessage arg)
        {
            if(arg.Content == "(╯°□°）╯︵ ┻━┻")
            {
                var contains = _tableflip.ContainsKey(arg.Channel.Id);
                if ((contains &&_tableflip[arg.Channel.Id].AddSeconds(60) < DateTime.Now) || !contains)
                {
                    await arg.Channel.SendMessageAsync("┬─┬ ノ( ゜-゜ノ)");
                    _tableflip.AddOrUpdate(arg.Channel.Id, DateTime.Now, (key, oldValue) => DateTime.Now);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
