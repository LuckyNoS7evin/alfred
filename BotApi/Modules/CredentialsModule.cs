using BotApi.Extensions;
using BotApi.HttpServices;
using BotApi.Models.Config;
using BotApi.Models.Credentials;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace BotApi.Modules
{

	[Group("credentials")]
	public class CredentialsModule : ModuleBase<SocketCommandContext>
    {
		private readonly AppConfig _config;
		private readonly StringCipher _cypher;
		private readonly GitHubService _gitHubService;
		public CredentialsModule( 
			IOptions<AppConfig> config,
			StringCipher cypher,
			GitHubService gitHubService)
		{
			_config = config.Value;
			_cypher = cypher;
			_gitHubService = gitHubService;
		}

		[Command("twitch")]
		public async Task AddCredentialsAsync()
		{

			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			if (!await CheckPermission.CheckOwnerPermission(
				guild.OwnerId,
				Context.User.Id))
			{
				await ReplyAsync("You are not the owner of this discord so cannot add credentials");
				return;
			}

			var state = new StateModel
			{
				UserId = Context.User.Id,
				ChannelId = Context.Channel.Id,
				GuildId = guild.Id
			};

			var serialisedState = System.Text.Json.JsonSerializer.Serialize(state);

			var encryptedSerialisedState = _cypher.Encrypt(serialisedState);

			var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
			await dmChannel.SendMessageAsync("To add your user credentials so we can access bits & subscriber " +
				"information use the following link to get access " +
				$"https://id.twitch.tv/oauth2/authorize?client_id={_config.TwitchClientId}&response_type=code&redirect_uri={_config.TwitchRedirectUrl}&scope=channel:read:subscriptions+bits:read+user:read:email&state={encryptedSerialisedState}");

			await ReplyAsync("I sent you a DM, better go check!");


		}
		

		[Command("github")]
		public async Task AddGitHubCredentialsAsync()
		{
			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			var state = new StateModel
			{
				UserId = Context.User.Id,
				ChannelId = Context.Channel.Id,
				GuildId = guild.Id
			};

			var serialisedState = System.Text.Json.JsonSerializer.Serialize(state);

			var encryptedSerialisedState = _cypher.Encrypt(serialisedState);

			var authLink = _gitHubService.GetAuthUrl(encryptedSerialisedState);
			var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
			await dmChannel.SendMessageAsync("To add your user credentials so we can access bits & subscriber " +
				"information use the following link to get access " +
				$"{authLink}");

			await ReplyAsync("I sent you a DM, better go check!");
		}

	}
}
