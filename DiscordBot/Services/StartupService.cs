using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using DiscordBot.Handlers;
using System;
using System.Threading.Tasks;
using Victoria;
using System.Threading;

namespace DiscordBot.Services {
    public class StartupService {
        private ServiceProvider _provider;
        private DiscordSocketClient _discord;
        private Lavalink _lavalink;

        public async Task InitializeAsync() {
            _provider = ConfigureServices();
            _discord = _provider.GetRequiredService<DiscordSocketClient>();
            _lavalink = _provider.GetRequiredService<Lavalink>();
            var global = new Global().Initialize();
            HookEvents();

            await _discord.LoginAsync(TokenType.Bot, Global.Config.Token);
            await _discord.StartAsync();

            await _provider.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private void HookEvents() {
            _lavalink.Log += LogAsync;
            _discord.Log += LogAsync;
            _provider.GetRequiredService<CommandService>().Log += LogAsync;
            _discord.Ready += OnReadyAsync;
        }

        private async Task OnReadyAsync() {
            try {
                var node = await _lavalink.AddNodeAsync(_discord, new Configuration {
                    Severity = LogSeverity.Info
                });
                node.TrackFinished += _provider.GetService<AudioService>().OnFinished;
                await _discord.SetGameAsync(Global.Config.GameStatus);
            } catch (Exception ex) {
                await LoggingService.LogInformationAsync(ex.Source, ex.Message);
            }
        }

        private async Task LogAsync(LogMessage logMessage) {
            await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }


        private ServiceProvider ConfigureServices() {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<Lavalink>()
                .AddSingleton<AudioService>()
                .AddSingleton<BotService>()
                .BuildServiceProvider();
        }
    }
}