using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            HookEvents();
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);
        }

        private void HookEvents()
        {
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogAsync;
            _discord.MessageReceived += HandleCommandAsync;
        }

        private Task HandleCommandAsync(SocketMessage socketMessage)
        {
            var argPos = 0;
            if (socketMessage is not SocketUserMessage message || message.Author.IsBot || message.Author.IsWebhook ||
                message.Channel is IPrivateChannel)
                return Task.CompletedTask;

            if (!message.HasStringPrefix(Global.Config.Prefix, ref argPos))
                return Task.CompletedTask;

            var context = new SocketCommandContext(_discord, message);

            var blacklistedCheck = from a in Global.Config.Blacklist
                where a == context.Channel.Id
                select a;
            var blacklistedChannel = blacklistedCheck.FirstOrDefault();

            if (blacklistedChannel == context.Channel.Id)
                return Task.CompletedTask;
            else
            {
                var result = _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);

                if (!result.Result.IsSuccess)
                    context.Channel.SendMessageAsync(result.Result.ErrorReason);

                return result;
            }
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }
    }
}