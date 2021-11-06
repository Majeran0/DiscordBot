using System;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Services;

namespace DiscordBot.Modules
{
    [Name("DJTS")]
    public class DJTS : ModuleBase<SocketCommandContext>
    {
        public DJTSService DjtsService { get; set; }

        [Command("2137")]
        [Summary("Pokazuje ile zostało do śmierci papieża polaka.")]
        public async Task DJTSAsync()
            => await ReplyAsync(DjtsService.DJTSAsync());
    }
}