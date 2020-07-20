using Bot.Data.Interfaces;
using BotApi.Extensions;
using BotApi.Models.Config;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Modules
{

	[Group("mod")]
	public class ModerationModule : ModuleBase<SocketCommandContext>
	{
		private readonly IGuildSettingsRepository _guildSettingsRepoistory;
		public ModerationModule(IGuildSettingsRepository guildSettingsRepoistory)
		{
			_guildSettingsRepoistory = guildSettingsRepoistory;
		}

		[Command("add-role")]
		public async Task AddModRole(IRole role)
		{
			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			if (!await CheckPermission.CheckOwnerPermission(
				guild.OwnerId,
				Context.User.Id))
			{ 
				await ReplyAsync("You are not the owner of this discord so cannot add credentials");
				return;
			}

			var currentSettings = await _guildSettingsRepoistory.GetAsync(guild.Id);
			if(currentSettings == null)
			{
				currentSettings = new Bot.Core.GuildSettings
				{
					GuildId = guild.Id.ToString(),
					ModRoles = new List<string>()
				};
			}

			currentSettings.ModRoles.Add(role.Id.ToString());
			await _guildSettingsRepoistory.SaveAsync(currentSettings);
			await ReplyAsync("Role added as moderator");
		}

		[Command("remove-role")]
		public async Task RemoveModRole(IRole role)
		{
			var guild = ((SocketGuildChannel)Context.Channel).Guild;
			if (!await CheckPermission.CheckOwnerPermission(
				guild.OwnerId,
				Context.User.Id))
			{
				await ReplyAsync("You are not the owner of this discord so cannot add credentials");
				return;
			}
			var currentSettings = await _guildSettingsRepoistory.GetAsync(guild.Id);
			if (currentSettings == null)
			{
				currentSettings = new Bot.Core.GuildSettings
				{
					GuildId = guild.Id.ToString(),
					ModRoles = new List<string>()
				};
			}

			currentSettings.ModRoles.Remove(role.Id.ToString());
			await _guildSettingsRepoistory.SaveAsync(currentSettings);
			await ReplyAsync("Role removed as moderator");
		}


	}
}
