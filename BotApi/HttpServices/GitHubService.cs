using BotApi.Models.Config;
using BotApi.Models.GitHub;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotApi.HttpServices
{
    public class GitHubService
    {
        private readonly ILogger<GitHubService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly AppConfig _config;

        public GitHubService(
            HttpClient httpClient, 
            IMemoryCache memoryCache, 
            IOptions<AppConfig> config,
            ILogger<GitHubService> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Organization>> GetGitHubOrgsAsync(string token)
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("alfred-discord"))
                {
                    Credentials = new Credentials(token)
                };
                var all = await client.Organization.GetAllForCurrent();

                return all;
            }
            catch (Exception ex)
            {
                var a = ex.ToString();
            }
            return null;
        }
        public async Task<InstallationsResponse> GetGitHubInstallationsAsync(string token)
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("alfred-discord"))
                {
                    Credentials = new Credentials(token)
                };
                var all = await client.GitHubApps.GetAllInstallationsForCurrentUser();

                return all;
            }
            catch (Exception ex)
            {
                var a = ex.ToString();
            }
            return null;
        }

        public async Task InviteMe(string token, long installationId)
        {

            var jwt = GetTempJwt();
            var client = new GitHubClient(new ProductHeaderValue("alfred-discord"))
            {
                Credentials = new Credentials(jwt, AuthenticationType.Bearer)
            };


            var installations = await client.GitHubApps.GetAllInstallationsForCurrent();

            var currentInstall = installations.Where(x => x.Id == installationId).FirstOrDefault();

            if(currentInstall == null)
            {
                return;
            }

            // Create an Installation token for the associated Insallation Id
            var response = await client.GitHubApps.CreateInstallationToken(currentInstall.Id);

            var user = await GetMeAsync(token);

            client.Credentials = new Credentials(response.Token, AuthenticationType.Bearer);

            await client.Organization.Member
                .AddOrUpdateOrganizationMembership(currentInstall.Account.Login, user.Login, new OrganizationMembershipUpdate
                {
                    Role = MembershipRole.Member
                });

        }

        public async Task<User> GetMeAsync(string token)
        {
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("alfred-discord"))
                {
                    Credentials = new Credentials(token)
                };
                var all = await client.User.Current();

                return all;
            }
            catch (Exception ex)
            {
                var a = ex.ToString();
            }
            return null;
        }

        public Uri GetAuthUrl(string state)
        {
            try
            {
                var jwt = GetTempJwt();
                var client = new GitHubClient(new ProductHeaderValue("alfred-discord"))
                {
                    Credentials = new Credentials(jwt)
                };
                var all =  client.Oauth.GetGitHubLoginUrl(new OauthLoginRequest(_config.GitHub.ClientId) { 
                    State = state
                });

                return all;
            }
            catch (Exception ex)
            {
                var a = ex.ToString();
            }
            return null;
        }

        public async Task<IdentityTokenModel> GetAccessTokenAsync(string code, string state)
        {
            var url = $"https://github.com/login/oauth/access_token?client_id={ _config.GitHub.ClientId}&client_secret={ _config.GitHub.Secret}&code={code}&state={state}&redirect_uri={_config.GitHub.RedirectUrl}";
            //var result = await _httpClient.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "application/json");
            var result = await _httpClient.SendAsync(request);
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

        private string GetTempJwt()
        {
            // Use GitHubJwt library to create the GitHubApp Jwt Token using our private certificate PEM file
            var generator = new GitHubJwt.GitHubJwtFactory(
                new GitHubJwt.FilePrivateKeySource("alfred-discord.2020-04-01.private-key.pem"),
                new GitHubJwt.GitHubJwtFactoryOptions
                {
                    AppIntegrationId = 59476, // The GitHub App Id
                    ExpirationSeconds = 600 // 10 minutes is the maximum time allowed
                }
            );

            return generator.CreateEncodedJwtToken();
        }

    }
}
