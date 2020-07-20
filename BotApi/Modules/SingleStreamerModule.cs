using Bot.Data.Interfaces;
using BotApi.Extensions;
using BotApi.HttpServices;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Modules
{

	[Group("single-streamer")]
	public class SingleStreamerModule : ModuleBase<SocketCommandContext>
	{
		private readonly ISingleStreamerSettingsRepository _singleStreamerSettingsRepository;
		private readonly IGuildSettingsRepository _guildSettingsRepoistory;
		private readonly TwitchService _twitchService;

		public SingleStreamerModule(
				ISingleStreamerSettingsRepository singleStreamerSettingsRepository,
				 IGuildSettingsRepository guildSettingsRepoistory,
				TwitchService twitchService
			)
		{
			_singleStreamerSettingsRepository = singleStreamerSettingsRepository;
			_guildSettingsRepoistory = guildSettingsRepoistory;
			_twitchService = twitchService;
		}

		[Command("set-channel")]
		public async Task SetChannelAsync(IChannel channel)
		{
			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			var settings = await _guildSettingsRepoistory.GetAsync(guild.Id);
			if (!await CheckPermission.CheckModPermission(
				guild.OwnerId,
				Context.User.Id,
				settings.ModRoles,
				((SocketGuildUser)Context.User).Roles.Select(x=>x.Id).ToList()
			))
			{
				await ReplyAsync("You don't have permission to do this!");
				return;
			}
			
			var current = await _singleStreamerSettingsRepository.GetAsync(guild.Id);

			if (current == null)
			{
				current = new Bot.Core.SingleStreamerSettings
				{
					GuildId = guild.Id.ToString()
				};
			}
			current.ChannelId = channel.Id.ToString();

			await _singleStreamerSettingsRepository.SaveAsync(current);
			await ReplyAsync("Channel Set");
		}

		[Command("set-streamer")]
        public async Task AddAsync(string twitchUsername)
		{
			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			var settings = await _guildSettingsRepoistory.GetAsync(guild.Id);
			if (!await CheckPermission.CheckModPermission(
				guild.OwnerId,
				Context.User.Id,
				settings.ModRoles,
				((SocketGuildUser)Context.User).Roles.Select(x => x.Id).ToList()
			))
			{
				await ReplyAsync("You don't have permission to do this!");
				return;
			}

			var current = await _singleStreamerSettingsRepository.GetAsync(guild.Id);

			if (current == null)
			{
				current = new Bot.Core.SingleStreamerSettings
				{
					GuildId = guild.Id.ToString()
				};
			}
			var user = await _twitchService.GetUserByLoginAsync(twitchUsername);

			if (user == null) return;

			current.UserId = user.Id;
			current.DisplayName = user.DisplayName;
			await _singleStreamerSettingsRepository.SaveAsync(current);
			
			await ReplyAsync("Single streamer set");
		}
    }
}
