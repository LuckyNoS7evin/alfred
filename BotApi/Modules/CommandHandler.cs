using BotApi.DiscordNet;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BotApi.Modules
{
    public class CommandHandler
    {
        private readonly ILogger<CommandHandler> _logger;
        private readonly IEnumerable<BotDiscordSocketClient> _clients;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(
            ILogger<CommandHandler> logger,
            IServiceProvider services, 
            IEnumerable<BotDiscordSocketClient> clients, 
            CommandService commands)
        {
            _logger = logger;
            _commands = commands;
            _clients = clients;
            _services = services;

        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            foreach(var client in _clients)
            {
                client.MessageReceived += HandleCommandAsync;
            }

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
           
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            var messageChannelType = message.Channel.GetType().Name;
            if (messageChannelType == "SocketTextChannel")
            {
                var guildId = ((SocketGuildChannel)message.Channel).Guild.Id;

                // // Determine if the message is a command based on the prefix and make sure no bots trigger commands
                if (!message.HasCharPrefix('!', ref argPos) || message.Author.IsBot) return;

                foreach (var client in _clients)
                {
                    if (client.Guilds.Any(x => x.Id == guildId))
                    {
                        // Create a WebSocket-based command context based on the message
                        var context = new SocketCommandContext(client, message);

                        // Execute the command with the command context we just
                        // created, along with the service provider for precondition checks.

                        // Keep in mind that result does not indicate a return value
                        // rather an object stating if the command executed successfully.
                        var result = await _commands.ExecuteAsync(
                            context: context,
                            argPos: argPos,
                            services: _services);

                    }
                }
            }
        }
    }
}
