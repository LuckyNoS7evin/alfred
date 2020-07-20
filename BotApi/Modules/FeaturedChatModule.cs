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

	[Group("featured-chat")]
	public class FeaturedChatModule : ModuleBase<SocketCommandContext>
	{
		private readonly IFeaturedChatSettingsRepository _featuredChatSettingsRepository;
		private readonly IGuildSettingsRepository _guildSettingsRepoistory;
		private readonly GitHubService _gitHubService;
		private readonly StringCipher _cypher;

		public FeaturedChatModule(
				IFeaturedChatSettingsRepository featuredChatSettingsRepository,
				IGuildSettingsRepository guildSettingsRepoistory,
				GitHubService gitHubService,
				StringCipher cypher
			)
		{
			_featuredChatSettingsRepository = featuredChatSettingsRepository;
			_guildSettingsRepoistory = guildSettingsRepoistory;
			_gitHubService = gitHubService;
			_cypher = cypher;
		}

		[Command("set-source")]
		public async Task ListOrgsAsync(ITextChannel channel, [Remainder] string browserSourceUrl = "")
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

			if (string.IsNullOrEmpty(browserSourceUrl))
			{
				await ReplyAsync("please enter your featured chat browser source url");
				return;
			}

			var current = await _featuredChatSettingsRepository.GetAsync(guild.Id);
			if(current == null)
			{
				current = new Bot.Core.FeaturedChatSettings
				{
					GuildId = guild.Id.ToString()
				};
			}

			current.ChannelId = channel.Id.ToString();
			current.BrowserSourceUrl = browserSourceUrl;

			await _featuredChatSettingsRepository.SaveAsync(current);

			
		}

		
	}
}
