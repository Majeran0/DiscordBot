using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBot.Modules {
    [Name("DJTS")]
    public class DJTS : ModuleBase<SocketCommandContext> {
        [Command("2137")]
        [Summary("No 2137.")]
        public async Task DJTSAsync() {
            var current = DateTime.Now;
            var dj = new DateTime(current.Year, current.Month, current.Day, 21, 37, 0);
            var djj = new DateTime(current.Year, current.Month, current.Day + 1, 21, 37, 0);
            if (current.Hour == 21 && current.Minute == 37) {
                await ReplyAsync("Papaj umar");
            } else {
                var interval = current < dj ? dj - current : djj - current;
                await ReplyAsync("Do 21:37 zostało " + interval.Hours + ":" + interval.Minutes + ":" + interval.Seconds);
            }
        }
    }
}