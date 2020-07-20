using Bot.Data.Interfaces;
using BotApi.Extensions;
using BotApi.HttpServices;
using BotApi.Models.Credentials;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Octokit;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Modules
{

	[Group("github")]
	public class GitHubModule : ModuleBase<SocketCommandContext>
	{
		private readonly IGitHubCredentialRepository _ownerGitHubCredentialRepository;
		private readonly IGuildSettingsRepository _guildSettingsRepoistory;
		private readonly GitHubService _gitHubService;
		private readonly StringCipher _cypher;

		public GitHubModule(
				IGitHubCredentialRepository ownerGitHubCredentialRepository,
				IGuildSettingsRepository guildSettingsRepoistory,
				GitHubService gitHubService,
				StringCipher cypher
			)
		{
			_ownerGitHubCredentialRepository = ownerGitHubCredentialRepository;
			_guildSettingsRepoistory = guildSettingsRepoistory;
			_gitHubService = gitHubService;
			_cypher = cypher;
		}

		[Command("install")]
		public async Task InstallGitHubCredentialsAsync()
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

			//var authLink = await _gitHubService.GetAuthUrlAsync(encryptedSerialisedState);
			var dmChannel = await Context.User.GetOrCreateDMChannelAsync();
			await dmChannel.SendMessageAsync("To add your user credentials so we can access bits & subscriber " +
				"information use the following link to get access " +
				$"https://github.com/apps/alfred-discord/installations/new?state={encryptedSerialisedState}");

			await ReplyAsync("I sent you a DM, better go check!");
		}

		[Command("set-org")]
		public async Task ListOrgsAsync([Remainder] string orgName = "")
		{
			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			var settings = await _guildSettingsRepoistory.GetAsync(guild.Id);
			if (!await CheckPermission.CheckOwnerPermission(
				guild.OwnerId,
				Context.User.Id
			))
			{
				await ReplyAsync("You don't have permission to do this!");
				return;
			}

			var current = await _ownerGitHubCredentialRepository.GetAsync(Context.User.Id);

			if (current == null)
			{
				await ReplyAsync("no github credentials set for you");
				return;
			}

			if (string.IsNullOrEmpty(orgName ))
			{

				var allOrgs = await _gitHubService.GetGitHubOrgsAsync(current.AccessToken);
				if (allOrgs.Count() == 0)
				{
					await ReplyAsync("Alfred is not installed in any of your Organisations");
				}
				var embed = new EmbedBuilder()
				.WithTitle("GitHub Organisations")
				.WithDescription("re-run `!github set-org` with the name of the organisation e.g. `!github set-org MyOrgName`")
				.AddField("Organisation Names", string.Join(" \r\n", allOrgs.Select(x => x.Login)), true)
				.Build();

				await ReplyAsync(embed: embed);
			} 
			else
			{
				var installs = await _gitHubService.GetGitHubInstallationsAsync(current.AccessToken);

				var install = installs.Installations
					.Where(x => x.TargetType == AccountType.Organization)
					.Where(x => x.Account.Login.ToLower() == orgName.ToLower())
					.FirstOrDefault();
				if(install == null)
				{
					await ReplyAsync("Alfred is not installed in any of your Organisations");
				}
				settings.GitHubInstallationID = install.Id.ToString();
				await _guildSettingsRepoistory.SaveAsync(settings);
				await ReplyAsync("Organisation set!");
			}


		}

		[Command("invite-me")]
		public async Task InviteMeAsync()
		{
			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			var settings = await _guildSettingsRepoistory.GetAsync(guild.Id);

			var current = await _ownerGitHubCredentialRepository.GetAsync(Context.User.Id);

			if (current == null)
			{
				await ReplyAsync("no github credentials set for you");
				return;
			}

			if (string.IsNullOrEmpty(settings.GitHubInstallationID))
			{
				await ReplyAsync("no github organisation linked to this discord server, discord server owner needs to run `!github set-org`");
				return;
			}

			await _gitHubService.InviteMe(current.AccessToken, long.Parse(settings.GitHubInstallationID));
			await ReplyAsync("You have been invited, check your emails");
		}
		
	}
}
