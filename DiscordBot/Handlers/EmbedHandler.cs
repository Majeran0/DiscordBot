using Discord;
using System.Threading.Tasks;

namespace DiscordBot.Handlers
{
    public class EmbedHandler
    {
        public static async Task<Embed> CreateBasicEmbed(string title, string description, Color color)
        {
            var embed = await Task.Run(()
                => new EmbedBuilder()
                    .WithTitle(title)
                    .WithDescription(description)
                    .WithColor(color)
                    .WithCurrentTimestamp()
                    .Build());
            return embed;
        }

        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(()
                => new EmbedBuilder()
                    .WithTitle($"ERROR OCCURED FROM - {source}")
                    .WithDescription($"**ERROR DETAILS**: \n{error}")
                    .WithColor(Color.DarkRed)
                    .WithCurrentTimestamp()
                    .Build());
            return embed;
        }
    }
}