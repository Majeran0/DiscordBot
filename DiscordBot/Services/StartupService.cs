using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Services {
    public class StartupService {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config) {
            _provider = provider;
            _config = config;
            _commands = commands;
            _discord = discord;
        }

        public async Task StartAsync() {
            string discordToken = _config.GetSection("token").Value;
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("You must enter your discord token to _config.yml");

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
    }
}