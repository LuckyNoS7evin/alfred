using Bot.Data.Interfaces;
using BotApi.Extensions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Modules
{
	public class MiscModule : ModuleBase<SocketCommandContext>
    {
		private readonly ConcurrentDictionary<ulong, DateTime> _alfredCommand;
		private readonly IGuildSettingsRepository _guildSettingsRepoistory;
		private readonly Random _random;
		private readonly List<string> _alfredQuotes;

		public MiscModule(Random random, IGuildSettingsRepository guildSettingsRepoistory)
		{
			_random = random;
			_guildSettingsRepoistory = guildSettingsRepoistory;
			_alfredCommand = new ConcurrentDictionary<ulong, DateTime>();
			_alfredQuotes = new List<string> {
				"I don't get drunk, I just get less classy and more fun.",
				"Having shots of tequila is just another way of saying, \"I like where I wake up to always be a surprise.\"",
				"... one drink away from telling everyone what I really think!",
				"I hope to arrive to my death, late, in love, and a little drunk",
				"I did not trip. The floor looked sad, so I thought it needed a hug.",
				"Alcohol is not in my vodkabulary. However, I looked it up on Whiskeypedia and learned if you drink too much of it, it's likely tequilya!",
				"You know what rhymes with Friday? BEER!",
				"In alcohol's defense I've done some pretty dumb stuff while completely sober too.",
				"One should always be drunk. That's all that matters...But with what? With wine, with poetry, or with virtue, as you chose. But get drunk.",
				"I like to have a martini, two at the very most. After three I’m under the table, after four I’m under my host.",
				"I cook with wine, sometimes I even add it to the food.",
				"Oh, you hate your job? Why didn’t you say so? There’s a support group for that. It’s called EVERYBODY, and they meet at the bar.",
				"I distrust camels, and anyone else who can go a week without a drink.",
				"I drink too much. The last time I gave a urine sample it had an olive in it.",
				"Alcohol may be man’s worst enemy, but the bible says love your enemy.",
				"I don’t have a drinking problem ‘Cept when I can’t get a drink.",
				"The problem with the world is that everyone is a few drinks behind.",
				"The best research for playing a drunk is being a British actor for 20 years.",
				"There is no bad whiskey. There are only some whiskeys that aren’t as good as others.",
				"Work is the curse of the drinking classes.",
				"A bottle of wine contains more philosophy that all the books in the world",
				"A woman drove me to drink, and I hadn’t even the courtesy to thank her.",
				"He was a wise man who invented beer.",
				"Why do I drink Champagne for breakfast? Doesn’t everyone?",
				"I only drink Champagne on two occasions, when I am in love and when I am not",
				"Either give me more wine or leave me alone.",
				"Age is just a number. It’s totally irrelevant unless, of course, you happen to be a bottle of wine.",
				"Reality is an illusion that occurs due to lack of alcohol."
			};
		}

		[Command("alfred")]
		public async Task AlfredAsync()
		{
			var contains = _alfredCommand.ContainsKey(Context.Channel.Id);
			if ((contains && _alfredCommand[Context.Channel.Id].AddSeconds(60) < DateTime.Now) || !contains)
			{
				await ReplyAsync(_alfredQuotes[_random.Next(_alfredQuotes.Count)]);
				_alfredCommand.AddOrUpdate(Context.Channel.Id, DateTime.Now, (key, oldValue) => DateTime.Now);
			}
		}

		[Command("purge")]
		public async Task InformationAsyc(int amount)
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

			var messagesToPurge = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();

			foreach (var message in messagesToPurge)
			{
				await Context.Channel.DeleteMessageAsync(message);
			}

		}
    }


}
