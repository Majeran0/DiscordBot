using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBot.Services
{
    public class DJTSService
    {
        public string DJTSAsync()
        {
            var current = DateTime.Now;
            var dj = new DateTime(current.Year, current.Month, current.Day, 21, 37, 0);
            if (current.Hour == 21 && current.Minute == 37)
            {
                return "Papaj umar";
            }
            else
            {
                var interval = current < dj ? dj - current : dj.AddDays(1) - current;
                return "Do 21:37 zostało " + interval.Hours + ":" + interval.Minutes + ":" + interval.Seconds;
            }
        }
    }
}