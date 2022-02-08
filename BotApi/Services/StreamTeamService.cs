

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
    public class StreamTeamService : IHostedService, IAsyncDisposable
    {
        private readonly ILogger<StreamTeamService> _logger;
        private readonly ITwitchTeamMemberRepository _twitchTeamMemberRepository;
        private readonly ITwitchTeamSettingsRepository _twitchTeamSettingsRepository;
        private readonly IEnumerable<BotDiscordSocketClient> _botDiscordSocketClients;
        private readonly TwitchService _twitchService;
        private readonly System.Timers.Timer _timer;
        private readonly ConcurrentDictionary<ulong, List<Message>> _existingMessages;
        private readonly Random _random;


        private class Message
        {
            public ulong Id { get; set; }
            public string TwitchId { get; set; }
        }

        public StreamTeamService(
            ILogger<StreamTeamService> logger,
            ITwitchTeamMemberRepository twitchTeamMemberRepository,
            ITwitchTeamSettingsRepository twitchTeamSettingsRepository,
            IEnumerable<BotDiscordSocketClient> botDiscordSocketClients,
            TwitchService twitchService,
            Random random
            )
        {
            _logger = logger;
            _twitchTeamMemberRepository = twitchTeamMemberRepository;
            _twitchTeamSettingsRepository = twitchTeamSettingsRepository;
            _botDiscordSocketClients = botDiscordSocketClients;
            _twitchService = twitchService;
            _random = random;
            _timer = new System.Timers.Timer(2 * 60 * 1000);
            _timer.Elapsed += timer_Elapsed;
            _timer.AutoReset = true;

            _existingMessages = new ConcurrentDictionary<ulong, List<Message>>();
        }

        private async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //Go get all teams
                var allTeamMembers = await _twitchTeamMemberRepository.GetAllTeamsAsync();
                var allTeamSettings = await _twitchTeamSettingsRepository.GetAllAsync();
                //create dictiopnary to keep track of updates/creates in this round
                var updates = new Dictionary<ulong, List<string>>();
                foreach (var team in allTeamSettings.Where(x => string.IsNullOrWhiteSpace(x.TeamName)))
                {
                    updates.Add(ulong.Parse(team.GuildId), new List<string>());
                    if (!_existingMessages.ContainsKey(ulong.Parse(team.GuildId)))
                    {
                        _existingMessages.TryAdd(ulong.Parse(team.GuildId), new List<Message>());
                    }
                }
                //unique list of streamers
                var channels = allTeamMembers.GroupBy(x => x.TwitchId).Select(x => x.Key).ToList();



                //get if online
                var online = await _twitchService.GetStreamsAsync(channels);

                //foreach team go through online streamers, write post in specified channel
                foreach (var onlineChannel in online)
                {
                    var teams = allTeamMembers.Where(x => x.TwitchId == onlineChannel.UserId);
                    foreach (var team in teams)
                    {
                        var settings = allTeamSettings.Where(x => string.IsNullOrWhiteSpace(x.TeamName)).FirstOrDefault(x => x.GuildId == team.GuildId);
                        updates[ulong.Parse(team.GuildId)].Add(onlineChannel.UserId);
                        //if guild has existing message for twitchid
                        //update otherwise create, keep track of updated/created items
                        if (_existingMessages[ulong.Parse(team.GuildId)].Any(x => x.TwitchId == onlineChannel.UserId))
                        {
                            //Update
                            var messageId = _existingMessages[ulong.Parse(team.GuildId)].First(x => x.TwitchId == onlineChannel.UserId).Id;
                            await UpdateMessageAsync(ulong.Parse(team.GuildId), ulong.Parse(settings.ChannelId), messageId, onlineChannel);
                        }
                        else
                        {
                            //Create
                            var messageId = await CreateMessageAsync(ulong.Parse(team.GuildId), ulong.Parse(settings.ChannelId), onlineChannel);
                            if (messageId != 0)
                            {
                                _existingMessages[ulong.Parse(team.GuildId)].Add(new Message
                                {
                                    Id = messageId,
                                    TwitchId = onlineChannel.UserId
                                });
                            }
                        }
                    }
                }

                foreach (var team in allTeamSettings.Where(x => string.IsNullOrWhiteSpace(x.TeamName)))
                {
                    var messagesToDelete = _existingMessages[ulong.Parse(team.GuildId)]
                        .Where(x => !updates[ulong.Parse(team.GuildId)].Any(y => y == x.TwitchId));

                    foreach (var message in messagesToDelete)
                    {
                        await DeleteMessageAsync(ulong.Parse(team.GuildId), ulong.Parse(team.ChannelId), message.Id);
                    }
                    _existingMessages[ulong.Parse(team.GuildId)].RemoveAll(x => messagesToDelete.Any(y => y.Id == x.Id));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StreamTeamService.timer_Elapsed");
            }
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var allTeamSettings = await _twitchTeamSettingsRepository.GetAllAsync();
                foreach (var team in allTeamSettings.Where(x => string.IsNullOrWhiteSpace(x.TeamName)))
                {
                    await ClearChannelAsync(ulong.Parse(team.GuildId), ulong.Parse(team.ChannelId));
                }

                _timer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StreamTeamService.StartAsync");
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
                _logger.LogError(ex, "StreamTeamService.UpdateMessageAsync");
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
                _logger.LogError(ex, "StreamTeamService.CreateMessageAsync");
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
                _logger.LogError(ex, "StreamTeamService.DeleteMessageAsync");
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
                _logger.LogError(ex, "StreamTeamService.ClearChannelAsync");
            }
        }
    }
}
