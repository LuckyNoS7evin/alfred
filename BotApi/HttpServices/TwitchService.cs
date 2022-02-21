using BotApi.Models.Config;
using BotApi.Models.Twitch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotApi.HttpServices
{
    public class TwitchService
    {
        private readonly ILogger<TwitchService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly AppConfig _config;

        public TwitchService(
            HttpClient httpClient, 
            IMemoryCache memoryCache, 
            IOptions<AppConfig> config,
            ILogger<TwitchService> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<UserModel> GetMeAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/users");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("Client-ID", _config.TwitchClientId);

            try
            {
                var result = await _httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var clipResult = JsonSerializer.Deserialize<UsersModel>(await result.Content.ReadAsStringAsync());
                    return clipResult.Data[0];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(TwitchService)}.{nameof(GetMeAsync)}", ex);
            }
            return null;
        }

        public async Task<UserModel> GetUserAsync(string userId)
        {
            if (!_memoryCache.TryGetValue("AppToken", out string appToken))
            {
                appToken = await GetTokenAsync();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/users?id={userId}");
            request.Headers.Add("Authorization", $"Bearer {appToken}");
            request.Headers.Add("Client-ID", _config.TwitchClientId);
            try
            {
                var result = await _httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var clipResult = JsonSerializer.Deserialize<UsersModel>(await result.Content.ReadAsStringAsync());
                    return clipResult.Data[0];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(TwitchService)}.{nameof(GetUserAsync)}", ex);
            }

            return null;
        }

        public async Task<UserModel> GetUserByLoginAsync(string username)
        {
            if (!_memoryCache.TryGetValue("AppToken", out string appToken))
            {
                appToken = await GetTokenAsync();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/users?login={username}");
            request.Headers.Add("Authorization", $"Bearer {appToken}");
            request.Headers.Add("Client-ID", _config.TwitchClientId);
            try
            {
                var result = await _httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var clipResult = JsonSerializer.Deserialize<UsersModel>(await result.Content.ReadAsStringAsync());
                    return clipResult.Data[0];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(TwitchService)}.{nameof(GetUserByLoginAsync)}", ex);
            }

            return null;
        }

        public async Task<List<string>> GetTeamAsync(string name)
        {
            if (!_memoryCache.TryGetValue("AppToken", out string appToken))
            {
                appToken = await GetTokenAsync();
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/helix/teams?name={name}");
            request.Headers.Add("Authorization", $"Bearer {appToken}");
            request.Headers.Add("Client-ID", _config.TwitchClientId);
            try
            {
                var result = await _httpClient.SendAsync(request);
                if (result.IsSuccessStatusCode)
                {
                    var resultString = await result.Content.ReadAsStringAsync();
                    var clipResult = JsonSerializer.Deserialize<TeamsModel>(resultString);
                    return clipResult.Data[0].Users.Select(x => x.Id).ToList();
                }
                else
                {
                    var a = await result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(TwitchService)}.{nameof(GetTeamAsync)}", ex);
            }

            return null;
        }

        public async Task<List<StreamModel>> GetStreamsAsync(List<string> channelIds)
        {
            if (!_memoryCache.TryGetValue("AppToken", out string appToken))
            {
                appToken = await GetTokenAsync();
            }

            var streams = new List<StreamModel>();
            var pages = (int)Math.Ceiling((double)channelIds.Count / 100);

            for (var i = 0; i < pages; i++)
            {
                var currentChannels = channelIds.Skip(i * 100).Take(100);
                var url = "";
                if (currentChannels.Count() == 1)
                {
                    url = $"https://api.twitch.tv/helix/streams?user_id={currentChannels.First()}";
                }
                else
                {
                    var channelsQuery = string.Join("&user_id=", currentChannels);
                    url = $"https://api.twitch.tv/helix/streams?user_id={channelsQuery}";
                }


                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {appToken}");
                request.Headers.Add("Client-ID", _config.TwitchClientId);
                try
                {
                    var result = await _httpClient.SendAsync(request);
                    if (result.IsSuccessStatusCode)
                    {
                        var clipResult = JsonSerializer.Deserialize<StreamsModel>(await result.Content.ReadAsStringAsync());
                        streams.AddRange(clipResult.Data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{nameof(TwitchService)}.{nameof(GetStreamsAsync)}", ex);
                }
            }
            return streams;
        }

        private async Task<string> GetTokenAsync()
        {
            var url = $"https://id.twitch.tv/oauth2/token?client_id={ _config.TwitchClientId}&client_secret={ _config.TwitchSecret}&grant_type=client_credentials";
            var result = await _httpClient.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));

            var token = JsonSerializer.Deserialize<IdentityTokenModel>(await result.Content.ReadAsStringAsync());

            _memoryCache.Set("AppToken", token.AccessToken, DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn));

            return token.AccessToken;
        }

        public async Task<IdentityTokenModel> GetAccessTokenAsync(string code)
        {
            var url = $"https://id.twitch.tv/oauth2/token?client_id={ _config.TwitchClientId}&client_secret={ _config.TwitchSecret}&code={code}&grant_type=authorization_code&redirect_uri={_config.TwitchRedirectUrl}";
            var result = await _httpClient.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));
            
            try
            {
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                var a = ex.ToString();
                var bodyError = await result.Content.ReadAsStringAsync();
            }

            var body = JsonSerializer.Deserialize<IdentityTokenModel>(await result.Content.ReadAsStringAsync());

            return body;
        }

        public async Task<IdentityRefreshModel> GetRefreshTokenAsync(string refreshToken)
        {

            var url = $"https://id.twitch.tv/oauth2/token?client_id={ _config.TwitchClientId}&client_secret={ _config.TwitchSecret}&refresh_token={refreshToken}&grant_type=refresh_token";

            var result = await _httpClient.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));

            //response.EnsureSuccessStatusCode();

            var body = JsonSerializer.Deserialize<IdentityRefreshModel>(await result.Content.ReadAsStringAsync());

            return body;
        }

    }
}
