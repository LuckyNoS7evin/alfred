
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
    [Route("[controller]")]
    public class CallbackController : ControllerBase
    {
        private readonly ILogger<CallbackController> _logger;
        private readonly TwitchService _service;
        private readonly StringCipher _cypher;
        private readonly IEnumerable<BotDiscordSocketClient> _botDiscordSocketClients;
        private readonly IOwnerTwitchCredentialRepository _ownerTwitchCredentialRespository;

        public CallbackController(
            ILogger<CallbackController> logger,
            TwitchService service, 
            StringCipher cypher,
            IEnumerable<BotDiscordSocketClient> botDiscordSocketClients,
            IOwnerTwitchCredentialRepository ownerTwitchCredentialRespository)
        {
            _logger = logger;
            _service = service;
            _cypher = cypher;
            _botDiscordSocketClients = botDiscordSocketClients;
            _ownerTwitchCredentialRespository = ownerTwitchCredentialRespository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string code, string scope, string state)
        {

            try
            {
                var serialisedState = _cypher.Decrypt(state);
                var stateModel = System.Text.Json.JsonSerializer.Deserialize<StateModel>(serialisedState);

                var accessModel = await _service.GetAccessTokenAsync(code);
                var user = await _service.GetMeAsync(accessModel.AccessToken);

                if (user == null) return BadRequest();
                await _ownerTwitchCredentialRespository.SaveAsync(new Bot.Core.OwnerTwitchCredentials
                {
                    Expiry = accessModel.ExpiresIn,
                    GuildId = stateModel.GuildId.ToString(),
                    RefreshToken = accessModel.RefreshToken,
                    TwitchId = user.Id
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
