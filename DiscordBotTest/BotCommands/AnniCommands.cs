using DiscordBot.JsonClasses;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
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
    public  class AnniCommands : BaseCommandModule
    {

        private readonly ConfigJson config = Functions.Functions.ReadConfig();

        [Description("Postet die Rolennachricht in den aktuellen Channel")]
        [Command("PostRoleText")]
        [Hidden]
        public async Task PostRoleText(CommandContext ctx)
        {
            DiscordEmoji male = DiscordEmoji.FromName(ctx.Client, ":prince:");
            DiscordEmoji female = DiscordEmoji.FromName(ctx.Client, ":princess:");
            DiscordEmoji divers = DiscordEmoji.FromName(ctx.Client, ":crown:");
            DiscordEmoji baby = DiscordEmoji.FromName(ctx.Client, ":baby:");
            DiscordEmoji teen = DiscordEmoji.FromName(ctx.Client, ":beer:");
            DiscordEmoji adult = DiscordEmoji.FromName(ctx.Client, ":tumbler_glass:");
            DiscordEmoji old = DiscordEmoji.FromName(ctx.Client, ":older_woman:");
            DiscordEmoji csgo = DiscordEmoji.FromName(ctx.Client, ":gun:");
            DiscordEmoji valo = DiscordEmoji.FromName(ctx.Client, ":bow_and_arrow:");
            DiscordEmoji lol = DiscordEmoji.FromName(ctx.Client, ":crystal_ball:");
            DiscordEmoji phasmo = DiscordEmoji.FromName(ctx.Client, ":ghost:");

            //var msgGender = await ctx.Channel.SendMessageAsync($"**Geschlecht**\n{female} weiblich\n{divers} divers\n{male} mänlich").ConfigureAwait(false);
            //await msgGender.CreateReactionAsync(female).ConfigureAwait(false);
            //await msgGender.CreateReactionAsync(divers).ConfigureAwait(false);
            //await msgGender.CreateReactionAsync(male).ConfigureAwait(false);

            //var msgAge = await ctx.Channel.SendMessageAsync($"**Alter**\n{baby} 12+\n{teen} 16+\n{adult} 18+\n{old} 21+").ConfigureAwait(false);
            //await msgAge.CreateReactionAsync(baby).ConfigureAwait(false);
            //await msgAge.CreateReactionAsync(teen).ConfigureAwait(false);
            //await msgAge.CreateReactionAsync(adult).ConfigureAwait(false);
            //await msgAge.CreateReactionAsync(old).ConfigureAwait(false);

            var msgGames = await ctx.Channel.SendMessageAsync($"**Reagiere hier um deine Rollen zu bekommen:**\n{csgo} CS:GO \n{valo} Valorant\n{lol} LoL\n{phasmo} Phasmophobia").ConfigureAwait(false);
            await msgGames.CreateReactionAsync(csgo).ConfigureAwait(false);
            await msgGames.CreateReactionAsync(valo).ConfigureAwait(false);
            await msgGames.CreateReactionAsync(lol).ConfigureAwait(false);
            await msgGames.CreateReactionAsync(phasmo).ConfigureAwait(false);

            await ctx.Message.DeleteAsync();

        }

       

    }
}
