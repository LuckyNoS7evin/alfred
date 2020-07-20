

using Bot.Core;
using Bot.Data.Interfaces;
using BotApi.DiscordNet;
using BotApi.Models.FeaturedChat;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApi.Services
{
    public class FeaturedChatService : IHostedService, IAsyncDisposable
    {
        private readonly ILogger<FeaturedChatService> _logger;
        private readonly IFeaturedChatSettingsRepository _featuredChatSettingsRepository;
        private readonly IEnumerable<BotDiscordSocketClient> _botDiscordSocketClients;
        private  HubConnection _signalrConnection;
        private readonly System.Timers.Timer _timer;

        private readonly List<FeaturedChatConnection> _featuredChatConnections;
        public FeaturedChatService(
            ILogger<FeaturedChatService> logger,
            IFeaturedChatSettingsRepository featuredChatSettingsRepository,
            IEnumerable<BotDiscordSocketClient> botDiscordSocketClients
            )
        {
            _logger = logger;
            _featuredChatSettingsRepository = featuredChatSettingsRepository;
            _botDiscordSocketClients = botDiscordSocketClients;
            _featuredChatConnections = new List<FeaturedChatConnection>();

            _timer = new System.Timers.Timer(30000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
        }

        private async void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await LoadConnections();
        }

        private class FeaturedChatConnection
        {
            public string Channel { get; set; }
            public string Slug { get; set; }
            public FeaturedChatSettings Settings { get; set; }
        }


        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {

                _signalrConnection = new HubConnectionBuilder()
                    .WithUrl("https://signalr.featured.chat/browserHub")
                    .WithAutomaticReconnect()
                    .Build();
                _signalrConnection.On<ShowChatLogModel>("show", async data => {
                    await WriteMessageToDiscord(data);
                });
                await _signalrConnection.StartAsync();

                await LoadConnections();

                _timer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TwitchTeamService.StartAsync");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TwitchTeamService.StopAsync");
            }
            return Task.CompletedTask;
        }


        private async Task LoadConnections()
        {
            var all = await _featuredChatSettingsRepository.GetAllAsync();
            foreach (var item in all)
            {
                if (!_featuredChatConnections.Any(x => x.Settings.GuildId == item.GuildId))
                {
                    var browserSourceUri = new Uri(item.BrowserSourceUrl);

                    if (browserSourceUri.Segments.Length < 4)
                        continue;

                    var channel = browserSourceUri.Segments[2].Trim('/');
                    var slug = browserSourceUri.Segments[3].Trim('/');

                    await _signalrConnection.InvokeAsync("ValidateAsync", channel, slug);

                    _featuredChatConnections.Add(new FeaturedChatConnection
                    {
                        Channel = channel,
                        Slug = slug,
                        Settings = item
                    });
                }
            }
        }

        private async Task WriteMessageToDiscord(ShowChatLogModel data)
        {
            var settings = _featuredChatConnections.Where(x => x.Channel == data.ChannelId);
            foreach( var setting in settings)
            {
                await CreateMessageAsync(ulong.Parse(setting.Settings.GuildId), ulong.Parse(setting.Settings.ChannelId), data);
            }
        }

        private async Task<ulong> CreateMessageAsync(ulong guildId, ulong channelId, ShowChatLogModel data)
        {
            foreach (var instance in _botDiscordSocketClients)
            {
                if (instance.Guilds.Any(x => x.Id == guildId))
                {
                    try
                    {
                        SocketTextChannel currentChannel = instance.GetChannel(channelId) as SocketTextChannel;
                        

                        var embed = new EmbedBuilder()
                             .WithTitle($"{data.User.DisplayName}")
                             .WithDescription($"{data.Message.Body}")
                             .WithThumbnailUrl($"{data.User.ProfileImageUrl}?_={Guid.NewGuid()}")
                             .Build();

                        var message = await currentChannel.SendMessageAsync(embed: embed);
                        return message.Id;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"FeaturedChatService.CreateMessageAsync");
                    }
                }
            }
            return 0;
        }
    }
}
