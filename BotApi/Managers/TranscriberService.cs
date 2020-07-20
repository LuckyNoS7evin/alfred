using Bot.Data.Interfaces;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Services
{
    public class TranscriberService
    {
        private readonly IMemoryCache _cache;
        private readonly ITranscriberRepository _transcriberRepository;
        public TranscriberService(
            IMemoryCache cache, 
            ITranscriberRepository transcriberRepository)
        {
            _cache = cache;
            _transcriberRepository = transcriberRepository;
        }

        public Task MessageReceived(SocketMessage arg)
        {
            var messageChannelType = arg.Channel.GetType().Name;
            if (messageChannelType == "SocketTextChannel")
            {
                var guild = ((SocketGuildChannel)arg.Channel).Guild;
                if (_cache.TryGetValue<List<Bot.Core.Transcriber>>("TRANSCODING_CHANNELS", out var transcribers))
                {
                    if (transcribers.Any(x => x.GuildId == guild.Id.ToString() && x.ChannelId == arg.Channel.Id.ToString()))
                    {
                        return Task.CompletedTask;
                    }
                }
            }
            return Task.CompletedTask;

        }
        
        public async Task StartAsync()
        {
            var allTranscribers = await _transcriberRepository.GetAllAsync();

            _cache.Set("TRANSCODING_CHANNELS", allTranscribers.Where(x => x.Ended == DateTime.MinValue).ToList());

        }

    }
}
