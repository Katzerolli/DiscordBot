using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DiscordBotTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Config in Debug -> Testbot Katzeroll & Plebhunter Discord
            //Config in Release -> SOT Clanbot & SOT Germany
            var config = Functions.Functions.ReadConfig();

            var bot = new Bot();
            bot.RunAsync(config).GetAwaiter().GetResult();


        }
    }
}
