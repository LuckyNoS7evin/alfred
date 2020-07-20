
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Data.Interfaces;
using BotApi.DiscordNet;
using BotApi.HttpServices;
using BotApi.Models.Credentials;
using BotApi.Services;
using Discord;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BotApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly ILogger<CallbackController> _logger;
        private readonly GitHubService _service;
        private readonly StringCipher _cypher;
        private readonly IEnumerable<BotDiscordSocketClient> _botDiscordSocketClients;
        private readonly IGitHubCredentialRepository _ownerGitHubCredentialRepository;

        public GitHubController(
            ILogger<CallbackController> logger,
            GitHubService service, 
            StringCipher cypher,
            IEnumerable<BotDiscordSocketClient> botDiscordSocketClients,
            IGitHubCredentialRepository ownerGitHubCredentialRepository)
        {
            _logger = logger;
            _service = service;
            _cypher = cypher;
            _botDiscordSocketClients = botDiscordSocketClients;
            _ownerGitHubCredentialRepository = ownerGitHubCredentialRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string code, string state)
        {

            try
            {
                var serialisedState = _cypher.Decrypt(state);
                var stateModel = System.Text.Json.JsonSerializer.Deserialize<StateModel>(serialisedState);

                var accessModel = await _service.GetAccessTokenAsync(code, state);
                await _ownerGitHubCredentialRepository.SaveAsync(new Bot.Core.GitHubCredentials
                {
                    UserId = stateModel.UserId.ToString(),
                    TokenType = accessModel.TokenType,
                    AccessToken = accessModel.AccessToken
                });

                foreach (var client in _botDiscordSocketClients)
                {
                    if (client.Guilds.Any(x => x.Id == stateModel.GuildId))
                    {
                        var channel = (ITextChannel)client.GetChannel(stateModel.ChannelId);
                        await channel.SendMessageAsync("Thanks, you are now authed");
                    }
                }
                return Redirect($"https://discordapp.com/channels/{stateModel.GuildId}/{stateModel.ChannelId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Poopageddon");
                return BadRequest();
            }

        }
    }
}
