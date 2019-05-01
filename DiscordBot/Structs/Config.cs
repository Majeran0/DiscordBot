using System.Collections.Generic;

namespace DiscordBot.Structs {
    public class Config {
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string GameStatus { get; set; }
        public List<ulong> Blacklist { get; set; }
    }
}
