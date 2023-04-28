using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DiscordBot.JsonClasses;
using Newtonsoft.Json;
using RestSharp;
using System.Linq.Expressions;
using static System.Net.WebRequestMethods;
using System.Reflection.Metadata;
using DSharpPlus.EventArgs;
using System.Security.Policy;
using System.Net.WebSockets;

namespace DiscordBot.SlashCommands
{
    public class StandardCommands : ApplicationCommandModule
    {
        private readonly RestClientOptions rOptions = new RestClientOptions() { MaxTimeout = -1 };
        private static readonly RestClientOptions staticrOptions = new RestClientOptions() { MaxTimeout = -1 };

        [SlashCommand("SetStatus", "Setzt den Status des Bot's")]
        public async Task SetStatus(InteractionContext ctx, 
                                    [Option("Typ", "Der status Typ")] string type = "Watching",
                                    [Option("Statustext", "Text der dargestellt wird")]
                                    [RemainingText] string status = "over the seven Seas")
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                var activityType = type.Equals("Watching") ? ActivityType.Watching :
                    type.Equals("Competing") ? ActivityType.Competing :
                    type.Equals("ListeningTo") ? ActivityType.ListeningTo :
                    type.Equals("Streaming") ? ActivityType.Streaming : ActivityType.Playing;
                var activity = new DiscordActivity($"{status}", activityType);
                await ctx.Client.UpdateStatusAsync(activity, UserStatus.Online);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Success!"));
            }
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No Permissions!"));
        }

        #region Animals, Comics
        [SlashCommand("xkcd", "Postet einen xkcd Comic")]
        public async Task PostComic(InteractionContext ctx,
                        [Option("Number", "Nummer des Comics")] long number = -1)
        {
            try
            {
                if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
                {
                    string uri = string.Empty;
                    string messageText;

                    if (number == -1)
                    {
                        uri = "https://xkcd.com/info.0.json";
                    }
                    else if (number == 0)
                    {
                        var rClient = new RestClient(rOptions);
                        var rRequest = new RestRequest("https://xkcd.com/info.0.json", Method.Get);
                        RestResponse rResponse = rClient.Execute(rRequest);

                        if (rResponse.IsSuccessful)
                        {
                            var rSerilizedXkcd = JsonConvert.DeserializeObject<xkcd>(rResponse.Content);
                            Random rnd = new Random();
                            int cNr = rnd.Next(1, rSerilizedXkcd.num);
                            uri = $"https://xkcd.com/{cNr}/info.0.json";
                        }
                        else
                        {
                            messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                        }
                    }
                    else
                    {
                        uri = $"https://xkcd.com/{number}/info.0.json";
                    }

                    var client = new RestClient(rOptions);
                    var request = new RestRequest(uri, Method.Get);
                    RestResponse response = client.Execute(request);

                    if (response.IsSuccessful)
                    {
                        var serilizedXkcd = JsonConvert.DeserializeObject<xkcd>(response.Content);
                        messageText = serilizedXkcd.img;
                    }
                    else
                    {
                        messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                    }
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(messageText));
                }
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No Permissions!"));
            }
        }

        [SlashCommand("Hund", "What the dog doin? Guess you have to find out...")]
        public async Task Hund(InteractionContext ctx)
        {
            try
            {

                if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
                {
                    string messageText;
                    string randomDog;

                    var client = new RestClient(rOptions);
                    var request = new RestRequest("https://random.dog/woof", Method.Get);
                    RestResponse randomDogRequest = client.Execute(request);

                    if (randomDogRequest.IsSuccessful)
                    {
                        randomDog = randomDogRequest.Content;
                        messageText = $"https://random.dog/{randomDog}";
                    }
                    else
                    {
                        messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                    }

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(messageText));
                }
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No Permissions!"));
            }
        }

        [SlashCommand("Katze", @"*Miau* /ᐠ｡▿｡ᐟ\*ᵖᵘʳʳ*")]
        public async Task Katze(InteractionContext ctx)
        {
            try
            {
                if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
                {
                    string messageText;

                    var client = new RestClient(rOptions);
                    var request = new RestRequest("https://api.thecatapi.com/v1/images/search", Method.Get);
                    RestResponse randomCatRequest = client.Execute(request);

                    if (randomCatRequest.IsSuccessful)
                    {
                        var strippedResponse = randomCatRequest.Content.Substring(1, randomCatRequest.Content.Length - 2);
                        var serilizedCat = JsonConvert.DeserializeObject<cat>(strippedResponse);
                        messageText = serilizedCat.url;
                    }
                    else
                    {
                        messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                    }

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(messageText));
                }
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No Permissions!"));
            }
        }

        [SlashCommand("Echse", "random.exe")]
        public async Task Echse(InteractionContext ctx)
        {
            try
            {
                if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
                {
                    string messageText;

                    var client = new RestClient(rOptions);
                    var request = new RestRequest("https://nekos.life/api/v2/img/lizard", Method.Get);
                    RestResponse randomLizardRequest = client.Execute(request);

                    if (randomLizardRequest.IsSuccessful)
                    {
                        var serilizedLizard = JsonConvert.DeserializeObject<echse>(randomLizardRequest.Content);
                        messageText = serilizedLizard.url;
                    }
                    else
                    {
                        messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                    }

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(messageText));
                }
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No Permissions!"));
            }
        }

        [SlashCommand("Quote", "Get a random anime quote")]
        public async Task Quote(InteractionContext ctx, 
                                                        [Option("Anime", "Which anime to quote. Not compatible with \"Character\"")] string anime = "",
                                                        [Option("Character", "Which character to quote. Not compatible with \"Anime\"")] string character = "")
        {

            string messageText;
            var interactionResponse = new DiscordInteractionResponseBuilder();

            try
            {

                if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
                {
                    bool err = false;
                    var embedResponse = new DiscordEmbedBuilder();
                    string url = "https://animechan.vercel.app/api/random";

                    if (anime == "" && character == "")
                    {
                        url = "https://animechan.vercel.app/api/random";
                    }
                    else if (anime != "" && character == "")
                    {
                        url = $"https://animechan.vercel.app/api/random/anime?title={anime}";
                    }
                    else if (character != "" && anime == "")
                    {
                        url = $"https://animechan.vercel.app/api/random/character?name={character}";
                    }
                    else
                    {
                        url = "https://animechan.vercel.app/api/random";
                        err = true;
                    }

                    var client = new RestClient(rOptions);
                    var request = new RestRequest(url, Method.Get);
                    RestResponse randomAnimeRequest = client.Execute(request);

                    if (randomAnimeRequest.IsSuccessful)
                    {
                        var randomQuote = JsonConvert.DeserializeObject<RandomQuote>(randomAnimeRequest.Content);
                        messageText = randomQuote.quote.ToString();
                        messageText += $"\n - {randomQuote.character}";
                        messageText += $"\n From: {randomQuote.anime}";
                        if (err)
                        {
                            messageText = "You cannot specify an anime and character";
                        }

                        embedResponse = new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Cyan,
                            Description = "Random Anime Quote"
                        };


                        embedResponse.AddField("Quote", messageText);

                        interactionResponse = new DiscordInteractionResponseBuilder()
                        {
                            Content = messageText,
                            Title = "Random Anime Quote"
                        };

                    }
                    else
                    {
                        messageText = "failed";
                    }

                    await ctx.CreateResponseAsync(/* messageText */ string.Empty, embedResponse, false);
                   
                }
            }
            
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("No Permissions!"));
            }
        }

        #endregion

        public async static void QuoteReactionAdded(DiscordClient client, MessageReactionAddEventArgs eventArgs)
        {
            var member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id).ConfigureAwait(false);
            var reaction = eventArgs.Emoji;
             
            // var msg = eventArgs.Message.Id;
            // var msgContent = eventArgs.Message.Content;
            var msgEmbedArray = eventArgs.Message.Embeds.ToArray();
            var msgEmbedContent = msgEmbedArray[0].Fields[0].Value;
            string split = "From: ";
            string anime = string.Empty;
            anime = msgEmbedContent.Substring(msgEmbedContent.IndexOf(split) + split.Length);

            if (eventArgs.Channel.Id == 941361942482796577)
            {
                switch (reaction)
                {
                    case "❤️":

                        await eventArgs.Channel.SendMessageAsync($"Danke <@{eventArgs.User.Id}> das du das geliked hast! <3");

                        string messageText = "";
                        // string anime = "";
                        string url;

                        if (anime == "")
                        {
                            url = "https://animechan.vercel.app/api/random";
                        }
                        else
                        {
                            url = $"https://animechan.vercel.app/api/random/anime?title={anime}";
                        }

                        var restClient = new RestClient(staticrOptions);
                        var request = new RestRequest(url, Method.Get);
                        RestResponse newQuoteRequest = restClient.Execute(request);

                        var randomQuote = JsonConvert.DeserializeObject<RandomQuote>(newQuoteRequest.Content);
                        messageText = randomQuote.quote.ToString();
                        messageText += $"\n - {randomQuote.character}";
                        messageText += $"\n From: {randomQuote.anime}";

                        var embedResponse = new DiscordEmbedBuilder();

                        embedResponse = new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Cyan,
                            Description = "Random Anime Quote"
                        };

                        embedResponse.AddField("Quote", messageText);

                        await eventArgs.Channel.SendMessageAsync(embedResponse).ConfigureAwait(false);

                        break;
                    default:
                        break;
                }
            }



        }
        public async static void QuoteReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
        {
            var member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id).ConfigureAwait(false);
            var reaction = eventArgs.Emoji;

            if (eventArgs.Channel.Id == 941361942482796577)
            {
                switch (reaction)
                {
                    case "❤️":
                        await eventArgs.Channel.SendMessageAsync($"Danke <@{eventArgs.User.Id}> das du das geliked hast! <3");
                        break;
                    default:
                        break;
                }
            }



        }
    }
}
