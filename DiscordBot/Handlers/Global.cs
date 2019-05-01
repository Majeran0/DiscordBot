using DiscordBot.Services;
using DiscordBot.Structs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Threading;

namespace DiscordBot.Handlers {
    public class Global {
        public static string ConfigPath { get; set; } = "_config.json";
        public static Config Config { get; set; }

        public async Task Initialize() {
            var json = string.Empty;

            if (!File.Exists(ConfigPath)) {
                json = JsonConvert.SerializeObject(GenerateNewConfig(), Formatting.Indented);
                File.WriteAllText("_config.json", json, new UTF8Encoding(false));
                await LoggingService.LogAsync("Bot", LogSeverity.Error, "No config file, generated a new one");
                await Task.Delay(Timeout.Infinite);
            }

            json = File.ReadAllText(ConfigPath, new UTF8Encoding(false));
            Config = JsonConvert.DeserializeObject<Config>(json);
        }

        private static Config GenerateNewConfig() => new Config {
            Token = "",
            Prefix = "|",
            GameStatus = "Test",
            Blacklist = new List<ulong>()
        };
    }
}
