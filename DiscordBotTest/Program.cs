using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DiscordBot.BotCommands;

namespace DiscordBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = Functions.Functions.ReadConfig();

            //TwitterTimer();

            var bot = new Bot();
            bot.RunAsync(config).GetAwaiter().GetResult();
        }
    }
}
