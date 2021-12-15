using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;


namespace DiscordBotTest
{
    class Program
    {

        static async Task Main(string[] args)
        {
            var config = Functions.Functions.ReadConfig();

            var bot = new Bot();
            bot.RunAsync(config).GetAwaiter().GetResult();
        }
    }
}
