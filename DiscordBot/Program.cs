﻿using DiscordBot.Services;
using System.Threading.Tasks;

namespace DiscordBot {
    class Program {
        public static Task Main(string[] args)
            => new StartupService().InitializeAsync();
    }
}