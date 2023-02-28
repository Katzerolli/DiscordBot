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

namespace DiscordBot.SlashCommands
{
    public class StandardCommands : ApplicationCommandModule
    {
        private readonly RestClientOptions rOptions = new RestClientOptions() { MaxTimeout = -1 };

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
        #endregion
    
        
    
    }
}
