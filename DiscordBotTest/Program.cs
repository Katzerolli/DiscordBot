using System.Threading.Tasks;

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
