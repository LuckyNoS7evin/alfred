using Bot.Data.Interfaces;
using BotApi.Extensions;
using BotApi.Models.Config;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BotApi.Modules
{

	[Group("transcribe")]
	public class TranscriberModule : ModuleBase<SocketCommandContext>
	{
		private readonly IGuildSettingsRepository _guildSettingsRepoistory;
		private readonly ITranscriberRepository _transcriberRepository;
		private readonly IMemoryCache _cache;
		public TranscriberModule(
			IGuildSettingsRepository guildSettingsRepoistory,
			ITranscriberRepository transcriberRepository,
			IMemoryCache cache
			)
		{
			_guildSettingsRepoistory = guildSettingsRepoistory;
			_transcriberRepository = transcriberRepository;
			_cache = cache;
		}

		[Command("start")]
		public async Task Start()
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
			var current = await _transcriberRepository.GetAsync(guild.Id, Context.Channel.Id);

			if(current == null)
			{
				current = new Bot.Core.Transcriber
				{
					GuildId = guild.Id.ToString(),
					ChannelId = Context.Channel.Id.ToString(),
					Started = DateTime.Now,
					Ended = DateTime.MinValue,
					MessageId = Context.Message.Id.ToString()
				};
			} 
			else if(current.Ended != DateTime.MinValue)
			{
				current.Started = DateTime.Now;
				current.Ended = DateTime.MinValue;
				current.MessageId = Context.Message.Id.ToString();
			} 
			else
			{
				await ReplyAsync("Transcriber already running on this channel");
				return;
			}

			await _transcriberRepository.SaveAsync(current);
			
			await ReplyAsync("Started transcriber on this channel");
		}

		[Command("end")]
		public async Task End()
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

			var current = await _transcriberRepository.GetAsync(guild.Id, Context.Channel.Id);

			if( current == null || current.Ended != DateTime.MinValue)
			{
				await ReplyAsync("Transcriber not currently running on this channel");
				return;
			}

			//Process Log
			current.Ended = DateTime.Now;

			var messageCount = 0;
			List<IMessage> messages = (await Context.Channel.GetMessagesAsync(ulong.Parse(current.MessageId), Direction.After).FlattenAsync()).ToList();
			messageCount = messages.Count();
			while (messageCount == 100)
			{
				var top = messages.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
				var nextSet = await Context.Channel.GetMessagesAsync(top.Id, Direction.After).FlattenAsync();
				messages.AddRange(nextSet);
				messageCount = nextSet.Count();
			}

			await _transcriberRepository.SaveAsync(current);

			byte[] buffer;

			using (var ms = new MemoryStream())
			{
				HtmlConverter.ConvertToPdf(@$"<html>
											<style>
												body {{
													font-family: Arial, Helvetica, sans-serif;
												}}

												.messages {{
													list-style-type: none;
													padding-inline-start: 0;
													font-size: 10pt;
												}}

												.chat-message {{
													color: #EFEFEF;
													width: 100%;
													background-color: #1e1e1e;
													border-radius: 10px;
													padding: 10px;
													margin-bottom: 5px;
												}}

												.chat-message img {{
													border-radius: 15px;
													height: 30px;
													width: 30px;
												}}

												.chat-message .img-col {{
												  float:left;
												  width:50px;
												}}

												.username {{
												  margin - bottom:5px;
												  font-weight:bold;
												}}

												.body-col {{
												  float:left;
												  width: 620px;
												}}

												.row:after {{
												  content: """";
												  display: table;
												  clear: both;
												}}
											</style>
											<body>
											<h1>{Context.Channel.Name}</h1>
											<h2>{current.Started.ToShortDateString()} {current.Started.ToShortTimeString()} 
											to {current.Ended.ToShortDateString()} {current.Ended.ToShortTimeString()}</h2>
											{GetMessagesHtml(messages)}
											</body></html>", ms);
				buffer = ms.ToArray();
			}

			using (var ms = new MemoryStream(buffer))
			{
				await Context.Channel.SendFileAsync(ms, $"transcribe.pdf");
			}

		}
		private string GetMessagesHtml(List<IMessage> messages)
		{
			return $"<ul class=\"messages\">{string.Join("",messages.OrderBy(x=> x.CreatedAt).Select(x => GetMessageHtml(x)))}</ul>";
		}

		private string GetMessageHtml(IMessage message)
		{
			return @$"<li class=""chat-message"">
						<div class=""row"">
							<div class=""img-col"">
								<img src=""{message.Author.GetAvatarUrl()}"" />
							</div>
							<div class=""body-col"">
								<div class=""username"">{message.Author.Username}</div>
								<div class=""body"">{message.Content}</div>
							</div>
						</div>
					</li>";
		}
	}
}
