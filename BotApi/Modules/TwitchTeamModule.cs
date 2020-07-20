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

	[Group("twitch-team")]
	public class TwitchTeamModule : ModuleBase<SocketCommandContext>
	{
		private readonly ITwitchTeamMemberRepository _twitchTeamMemberRepository;
		private readonly ITwitchTeamSettingsRepository _twitchTeamSettingsRepository;
		private readonly IGuildSettingsRepository _guildSettingsRepoistory;
		private readonly TwitchService _twitchService;

		public TwitchTeamModule(
				ITwitchTeamMemberRepository twitchTeamMemberRepository,
				ITwitchTeamSettingsRepository twitchTeamSettingsRepository,
				IGuildSettingsRepository guildSettingsRepoistory,
				TwitchService twitchService
			)
		{
			_twitchTeamMemberRepository = twitchTeamMemberRepository;
			_twitchTeamSettingsRepository = twitchTeamSettingsRepository;
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
				((SocketGuildUser)Context.User).Roles.Select(x => x.Id).ToList()
			))
			{
				await ReplyAsync("You don't have permission to do this!");
				return;
			}
			var current = await _twitchTeamSettingsRepository.GetAsync(guild.Id);

			if (current == null)
			{
				current = new Bot.Core.TwitchTeamSettings
				{
					GuildId = guild.Id.ToString(),
					TeamName = ""
				};
			}
			current.ChannelId = channel.Id.ToString();

			await _twitchTeamSettingsRepository.SaveAsync(current);
			await ReplyAsync("Channel Set");
		}

		[Command("set-team")]
		public async Task SetTeamAsync(string teamName)
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
			var current = await _twitchTeamSettingsRepository.GetAsync(guild.Id);

			if(current == null)
			{
				current = new Bot.Core.TwitchTeamSettings
				{
					GuildId = guild.Id.ToString(),
					ChannelId = ""
				};
			}
			current.TeamName = teamName;

			await _twitchTeamSettingsRepository.SaveAsync(current);
			await ReplyAsync("Team Name Set");
		}

		[Command("add")]
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

			var team = await _twitchTeamMemberRepository.GetTeamAsync(guild.Id);

			var user = await _twitchService.GetUserByLoginAsync(twitchUsername);

			if (user == null) return;

			if (!team.Any(x => x.TwitchId == user.Id))
			{
				await _twitchTeamMemberRepository.SaveAsync(new Bot.Core.TwitchTeamMember
				{
					GuildId = guild.Id.ToString(),
					TwitchId = user.Id,
					TwitchDisplayName = user.DisplayName
				});
				await ReplyAsync("Team member added");
			}
			else
			{
				await ReplyAsync("Team member already exists");
			}
		}

		[Command("remove")]
		public async Task RemoveAsync(string twitchUsername)
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

			var team = await _twitchTeamMemberRepository.GetTeamAsync(guild.Id);

			var user = await _twitchService.GetUserByLoginAsync(twitchUsername);

			if (user == null) return;

			if (team.Any(x => x.TwitchId == user.Id))
			{
				await _twitchTeamMemberRepository.DeleteAsync(guild.Id, user.Id);
				await ReplyAsync("Team member removed");
			}
			else
			{
				await ReplyAsync("Team member doesn't exist");
			}
		}

		[Command("show")]
		public async Task ShowAsync()
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

			var team = await _twitchTeamMemberRepository.GetTeamAsync(guild.Id);

			if (team.Count == 0)
			{
				await ReplyAsync("No team members yet");
				return;
			}

			var embed = new EmbedBuilder()
				.WithTitle("Twitch Team Members")
				.AddField("Id",string.Join("\r\n", team.Select(x => x.TwitchId)), true)
				.AddField("Display Name", string.Join("\r\n", team.Select(x => x.TwitchDisplayName)), true)
				.Build();


			await ReplyAsync(embed: embed);
		}
	}
}
