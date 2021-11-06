using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using System.Threading.Tasks;

namespace DiscordBot.Modules {
    public class AudioModule : ModuleBase<SocketCommandContext> {
        private AudioService AudioService { get; }
        
        public AudioModule(AudioService audioService)
        {
            AudioService = audioService;
        }
        
        [Command("Join")]
        public async Task JoinAndPlay()
            => await ReplyAsync("", false, await AudioService.JoinOrPlayAsync((SocketGuildUser)Context.User, Context.Channel, Context.Guild));

        [Command("Leave")]
        public async Task Leave()
            => await ReplyAsync("", false, await AudioService.LeaveAsync(Context.Guild));

        [Command("Play")]
        public async Task Play([Remainder]string search)
            => await ReplyAsync("", false, await AudioService.JoinOrPlayAsync((SocketGuildUser)Context.User, Context.Channel, Context.Guild, search));

        [Command("Stop")]
        public async Task Stop()
            => await ReplyAsync("", false, await AudioService.StopAsync(Context.Guild));

        [Command("List")]
        public async Task List()
            => await ReplyAsync("", false, await AudioService.ListAsync(Context.Guild));

        [Command("Skip")]
        public async Task Delist(string id = null)
            => await ReplyAsync("", false, await AudioService.SkipTrackAsync(Context.Guild));

        [Command("Volume")]
        public async Task Volume(ushort volume)
            => await ReplyAsync(await AudioService.VolumeAsync(Context.Guild, volume));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await AudioService.Pause(Context.Guild));

        [Command("Resume")]
        public async Task Resume()
=> await ReplyAsync(await AudioService.Pause(Context.Guild));
    }
}
