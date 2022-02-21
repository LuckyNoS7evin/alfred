using BotApi.Models.Config;
using BotApi.Models.Twitch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotApi.HttpServices
{
    public class EventSubService
    {
        private readonly ILogger<EventSubService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly AppConfig _config;
        public EventSubService(
            HttpClient httpClient,
            IMemoryCache memoryCache,
            IOptions<AppConfig> config,
            ILogger<EventSubService> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _config = config.Value;
            _logger = logger;
        }


        //public async Task CheckStreamSubscriptionsAsync(string accessToken)
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/users");
        //    request.Headers.Add("Authorization", $"Bearer {accessToken}");
        //    request.Headers.Add("Client-ID", _config.TwitchClientId);

        //    try
        //    {
        //        var result = await _httpClient.SendAsync(request);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            var clipResult = JsonSerializer.Deserialize<UsersModel>(await result.Content.ReadAsStringAsync());
        //            return clipResult.Data[0];
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"{nameof(TwitchService)}.{nameof(CheckStreamSubscriptionsAsync)}", ex);
        //    }
        //    return null;
        //}

    }
}
