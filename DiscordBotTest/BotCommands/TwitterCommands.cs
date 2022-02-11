using DiscordBot.JsonClasses;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DiscordBot.JsonClasses.TwitterJson;

namespace DiscordBot.BotCommands
{
    public  class TwitterCommands : BaseCommandModule
    {

        private static System.Timers.Timer aTimer;
        private readonly ConfigJson config = Functions.Functions.ReadConfig();

        public static void TwitterTimer()
        {
            // Create a timer and set a two hour interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 1000 * 60 * 120;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {

        }

        [Command("GetSOT")]
        [Description("Postet die letzten SOT Tweets")]
        public async Task GetSOT(CommandContext ctx, [Description("Anzahl der Tweets (Min 5, Max 10, Default 5)")] int anzahl = 5)
        {
            var msg = string.Empty;
            var msg2 = string.Empty;
            TweetList twitterResponse = new TweetList();

            if (5 > anzahl || anzahl > 100)
            {
                await ctx.Channel.SendMessageAsync("Du kek hast eine ungültige Zahl angegeben").ConfigureAwait(false);
                return;
            }

            var client = new RestClient($"https://api.twitter.com/2/users/3375660701/tweets?max_results={anzahl}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {config.TwitterValues.BearerToken}");
            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                int c = 0;
                twitterResponse = JsonConvert.DeserializeObject<TweetList>(response.Content);
                
                foreach (var t in twitterResponse.data)
                {
                    if (c < 5)
                    {
                        msg += $"https://twitter.com/SeaOfThieves/status/{t.id}\n";
                        c++;
                    }
                    else
                    {
                        msg2 += $"https://twitter.com/SeaOfThieves/status/{t.id}\n";
                        c++;
                    }
                }

                await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);

                if (c > 5)
                {
                    await ctx.Channel.SendMessageAsync(msg2).ConfigureAwait(false);
                }
            }
        }

        [Command("GetGiveaway")]
        [Description("Durchsucht SOT Twitter nach Giveaways")]
        public async Task GetGiveaway(CommandContext ctx, [Description("Anzahl der Tweets (Min 5, Max 10, Default 5)")] int anzahl = 5)
        {
            var msg = string.Empty;
            var msg2 = string.Empty;
            TweetList twitterResponse = new TweetList();

            if (5 > anzahl || anzahl > 100)
            {
                await ctx.Channel.SendMessageAsync("Du kek hast eine ungültige Zahl angegeben").ConfigureAwait(false);
                return;
            }

            var client = new RestClient($"https://api.twitter.com/2/users/3375660701/tweets?max_results={anzahl}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", $"Bearer {config.TwitterValues.BearerToken}");
            IRestResponse response = client.Execute(request);

            if (response.IsSuccessful)
            {
                int c = 0;
                twitterResponse = JsonConvert.DeserializeObject<TweetList>(response.Content);

                foreach (var t in twitterResponse.data)
                {
                    if (c < 5)
                    {
                        msg += $"https://twitter.com/SeaOfThieves/status/{t.id}\n";
                        c++;
                    }
                    else
                    {
                        msg2 += $"https://twitter.com/SeaOfThieves/status/{t.id}\n";
                        c++;
                    }
                }

                await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);

                if (c > 5)
                {
                    await ctx.Channel.SendMessageAsync(msg2).ConfigureAwait(false);
                }
            }
        }

    }
}
