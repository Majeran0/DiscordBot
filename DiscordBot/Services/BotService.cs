using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using DiscordBot.Handlers;
using System.Linq;
using Victoria;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    class BotService {
        public AudioService Audio { get; set; }

        public async Task<Embed> DisplayInfoAsync(SocketCommandContext context) {
            var fields = new List<EmbedFieldBuilder>();
            fields.Add(new EmbedFieldBuilder {
                Name = "Client Info",
                Value = $"Current Server: {context.Guild.Name} - Prefix {Global.Config.Prefix}",
                IsInline = false
            });
            fields.Add(new EmbedFieldBuilder {
                Name = "Guild Info",
                Value = $"Current People : {context.Guild.Users.Count(x => !x.IsBot)}",
                IsInline = false
            });

            var embed = await Task.Run(() => new EmbedBuilder {
                Title = "Info",
                ThumbnailUrl = context.Guild.IconUrl,
                Timestamp = DateTime.Now,
                Color = Color.DarkOrange,
                Footer = new EmbedFooterBuilder { Text = "Discord.net + Victoria test bot", IconUrl = context.Client.CurrentUser.GetAvatarUrl() },
                Fields = fields
            });

            return embed.Build();
        }
    }
}
