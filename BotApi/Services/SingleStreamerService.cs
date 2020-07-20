

using Bot.Core;
using Bot.Data.Interfaces;
using BotApi.DiscordNet;
using BotApi.HttpServices;
using BotApi.Models.Twitch;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApi.Services
{
    public class SingleStreamerService : IHostedService, IAsyncDisposable
    {
        private readonly ILogger<SingleStreamerService> _logger;
        private readonly ISingleStreamerSettingsRepository _singleStreamerSettingsRepository;
        private readonly IEnumerable<BotDiscordSocketClient> _botDiscordSocketClients;
        private readonly TwitchService _twitchService;
        private readonly System.Timers.Timer _timer;
        private readonly ConcurrentDictionary<ulong, Message> _channelMainMessages;
        private readonly ConcurrentDictionary<ulong, Message> _liveMessages;
        private readonly Random _random;


        private class Message
        {
            public ulong Id { get; set; }
            public string TwitchId { get; set; }
        }

        public SingleStreamerService(
            ILogger<SingleStreamerService> logger,
            ISingleStreamerSettingsRepository singleStreamerSettingsRepository,
            IEnumerable<BotDiscordSocketClient> botDiscordSocketClients,
            TwitchService twitchService,
            Random random
            )
        {
            _logger = logger;
            _singleStreamerSettingsRepository = singleStreamerSettingsRepository;
            _botDiscordSocketClients = botDiscordSocketClients;
            _twitchService = twitchService;
            _random = random;

            _timer = new System.Timers.Timer(2 * 60 * 1000);
            _timer.Elapsed += timer_Elapsed;
            _timer.AutoReset = true;

            _channelMainMessages = new ConcurrentDictionary<ulong, Message>();
            _liveMessages = new ConcurrentDictionary<ulong, Message>();
        }

        private async Task RunLiveChannels()
        {
            try
            {
                //Go get all teams
                var allStreamersSettings = await _singleStreamerSettingsRepository.GetAllAsync();

                var channels = allStreamersSettings
                    .Where(x => !string.IsNullOrWhiteSpace(x.UserId))
                    .Select(x => x.UserId)
                    .ToList();

                //get if online
                var online = await _twitchService.GetStreamsAsync(channels);

                foreach (var streamer in allStreamersSettings)
                {

                    ulong mainMessageId = 0;
                    if (!_channelMainMessages.ContainsKey(ulong.Parse(streamer.GuildId)))
                    {
                        var messageId = await CreateLiveMessageAsync(ulong.Parse(streamer.GuildId), ulong.Parse(streamer.ChannelId), streamer);
                        _channelMainMessages.TryAdd(ulong.Parse(streamer.GuildId), new Message
                        {
                            Id = messageId,
                            TwitchId = streamer.UserId
                        });
                    }
                    mainMessageId = _channelMainMessages[ulong.Parse(streamer.GuildId)].Id;
                    //select streamer
                    if (online.Any(x => x.UserId == streamer.UserId))
                    {
                        await UpdateLiveMessageAsync(ulong.Parse(streamer.GuildId), ulong.Parse(streamer.ChannelId), mainMessageId, streamer, true);
                        var onlineChannel = online.First(x => x.UserId == streamer.UserId);
                        //if guild has existing message for twitchid
                        //update otherwise create, keep track of updated/created items
                        if (_liveMessages.ContainsKey(ulong.Parse(streamer.GuildId)))
                        {
                            //Update
                            var messageId = _liveMessages[ulong.Parse(streamer.GuildId)].Id;
                            await UpdateMessageAsync(ulong.Parse(streamer.GuildId), ulong.Parse(streamer.ChannelId), messageId, onlineChannel);
                        }
                        else
                        {
                            //Create
                            var messageId = await CreateMessageAsync(ulong.Parse(streamer.GuildId), ulong.Parse(streamer.ChannelId), onlineChannel);
                            if (messageId != 0)
                            {
                                _liveMessages.TryAdd(ulong.Parse(streamer.GuildId), new Message
                                {
                                    Id = messageId,
                                    TwitchId = onlineChannel.UserId
                                });
                            }
                        }

                    }
                    else
                    {
                        await UpdateLiveMessageAsync(ulong.Parse(streamer.GuildId), ulong.Parse(streamer.ChannelId), mainMessageId, streamer, false);
                        if (_liveMessages.ContainsKey(ulong.Parse(streamer.GuildId)))
                        {
                            var messageId = _liveMessages[ulong.Parse(streamer.GuildId)].Id;
                            await DeleteMessageAsync(ulong.Parse(streamer.GuildId), ulong.Parse(streamer.ChannelId), messageId);
                            _liveMessages.TryRemove(ulong.Parse(streamer.GuildId), out _);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.RunLiveChannels");
            }
        }

        private async Task CreateLiveMessages()
        {
            try
            {
                //Go get all teams
                var allStreamersSettings = await _singleStreamerSettingsRepository.GetAllAsync();
                foreach (var user in allStreamersSettings)
                {
                    var messageId = await CreateLiveMessageAsync(ulong.Parse(user.GuildId), ulong.Parse(user.ChannelId), user);
                    _channelMainMessages.TryAdd(ulong.Parse(user.GuildId), new Message
                    {
                        Id = messageId,
                        TwitchId = user.UserId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.CreateLiveMessages");
            }
        }

        private async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await RunLiveChannels();
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var allStreamersSettings = await _singleStreamerSettingsRepository.GetAllAsync();
                foreach (var streamer in allStreamersSettings)
                {
                    await ClearChannelAsync(ulong.Parse(streamer.GuildId), ulong.Parse(streamer.ChannelId));
                }
                await CreateLiveMessages();
                await RunLiveChannels();
                _timer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.StartAsync");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Stop();
            return Task.CompletedTask;
        }
        private async Task UpdateMessageAsync(ulong guildId, ulong channelId, ulong messageId, StreamModel streamModel)
        {
            try
            {
                foreach (var instance in _botDiscordSocketClients)
                {
                    if (instance.Guilds.Any(x => x.Id == guildId))
                    {
                        SocketTextChannel currentChannel = instance.GetChannel(channelId) as SocketTextChannel;

                        var streamAge = (DateTime.Now - streamModel.StartedAt);
                        var currentMessage = (RestUserMessage)await currentChannel.GetMessageAsync(messageId);
                        var embed = new EmbedBuilder()
                             .WithFooter($"{streamAge:hh\\:mm\\:ss}")
                             .WithColor(_random.Next(255), _random.Next(255), _random.Next(255))
                             .WithTitle($"{streamModel.Title}")
                             .WithImageUrl($"{streamModel.ThumbnailUrl.Replace("{width}", "640").Replace("{height}", "360")}?_={Guid.NewGuid()}")
                             .WithUrl($"https://twitch.tv/{streamModel.UserName}")
                             .WithAuthor(streamModel.UserName, url: $"https://twitch.tv/{streamModel.UserName}")
                            .Build();

                        await currentMessage.ModifyAsync((action) =>
                        {
                            action.Embed = embed;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.UpdateMessageAsync");
            }
        }

        private async Task<ulong> CreateMessageAsync(ulong guildId, ulong channelId, StreamModel streamModel)
        {
            try
            {
                foreach (var instance in _botDiscordSocketClients)
                {
                    if (instance.Guilds.Any(x => x.Id == guildId))
                    {
                        SocketTextChannel currentChannel = instance.GetChannel(channelId) as SocketTextChannel;
                        var streamAge = (DateTime.Now - streamModel.StartedAt);
                        var embed = new EmbedBuilder()
                             .WithFooter($"{streamAge:hh\\:mm\\:ss}")
                             .WithColor(_random.Next(255), _random.Next(255), _random.Next(255))
                             .WithTitle($"{streamModel.Title}")
                             .WithImageUrl($"{streamModel.ThumbnailUrl.Replace("{width}", "640").Replace("{height}", "360")}?_={Guid.NewGuid()}")
                             .WithUrl($"https://twitch.tv/{streamModel.UserName}")
                             .WithAuthor(streamModel.UserName, url: $"https://twitch.tv/{streamModel.UserName}")
                             .Build();

                        var message = await currentChannel.SendMessageAsync(embed: embed);
                        return message.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.CreateMessageAsync");
            }
            return 0;
        }

        private async Task UpdateLiveMessageAsync(ulong guildId, ulong channelId, ulong messageId, SingleStreamerSettings settings, bool isLive)
        {
            try
            {
                foreach (var instance in _botDiscordSocketClients)
                {
                    if (instance.Guilds.Any(x => x.Id == guildId))
                    {
                        SocketTextChannel currentChannel = instance.GetChannel(channelId) as SocketTextChannel;

                        var currentMessage = (RestUserMessage)await currentChannel.GetMessageAsync(messageId);
                        if (currentMessage != null)
                        {
                            if (currentMessage.Content.StartsWith("@everyone") && !isLive)
                            {
                                await currentMessage.DeleteAsync();
                                var newMessage = await currentChannel.SendMessageAsync($"{settings.DisplayName} is now offline, you missed it!");
                                _channelMainMessages[guildId].Id = newMessage.Id;
                            }

                            if (!currentMessage.Content.StartsWith("@everyone") && isLive)
                            {
                                await currentMessage.DeleteAsync();
                                var newMessage = await currentChannel.SendMessageAsync($"@everyone {settings.DisplayName} is now Live.... ");
                                _channelMainMessages[guildId].Id = newMessage.Id;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.UpdateLiveMessageAsync");
            }
        }

        private async Task<ulong> CreateLiveMessageAsync(ulong guildId, ulong channelId, SingleStreamerSettings singleStreamerSettings)
        {
            try
            {
                foreach (var instance in _botDiscordSocketClients)
                {
                    if (instance.Guilds.Any(x => x.Id == guildId))
                    {
                        SocketTextChannel currentChannel = instance.GetChannel(channelId) as SocketTextChannel;
                        var message = await currentChannel.SendMessageAsync($"{singleStreamerSettings.DisplayName} is now offline, you missed it!");
                        return message.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.CreateLiveMessageAsync");
            }
            return 0;
        }

        private async Task DeleteMessageAsync(ulong guildId, ulong channelId, ulong messageId)
        {
            try
            {
                foreach (var instance in _botDiscordSocketClients)
                {
                    if (instance.Guilds.Any(x => x.Id == guildId))
                    {
                        SocketTextChannel currentChannel = instance.GetChannel(channelId) as SocketTextChannel;
                        await currentChannel.DeleteMessageAsync(messageId);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.DeleteMessageAsync");
            }
        }

        private async Task ClearChannelAsync(ulong guildId, ulong channelId)
        {
            try
            {
                foreach (var instance in _botDiscordSocketClients)
                {
                    if (instance.Guilds.Any(x => x.Id == guildId))
                    {
                        SocketTextChannel currentChannel = instance.GetChannel(channelId) as SocketTextChannel;

                        var messagesToPurge = await currentChannel.GetMessagesAsync().FlattenAsync();
                        while (messagesToPurge.Count() > 0)
                        {
                            foreach (var message in messagesToPurge)
                            {
                                await currentChannel.DeleteMessageAsync(message);
                            }
                            messagesToPurge = await currentChannel.GetMessagesAsync().FlattenAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SingleStreamerService.ClearChannelAsync");
            }
        }
    }
}
