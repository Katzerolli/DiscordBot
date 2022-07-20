using DiscordBot.JsonClasses;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

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
            //Declare emojis
            DiscordEmoji csgo = DiscordEmoji.FromName(ctx.Client, ":gun:");
            DiscordEmoji valo = DiscordEmoji.FromName(ctx.Client, ":bow_and_arrow:");
            DiscordEmoji lol = DiscordEmoji.FromName(ctx.Client, ":crystal_ball:");
            DiscordEmoji phasmo = DiscordEmoji.FromName(ctx.Client, ":ghost:");

            DiscordEmoji linke = DiscordEmoji.FromName(ctx.Client, ":mosquito:");
            DiscordEmoji grüne = DiscordEmoji.FromName(ctx.Client, ":leaves:");
            DiscordEmoji afd = DiscordEmoji.FromName(ctx.Client, ":clown:");
            DiscordEmoji fdp = DiscordEmoji.FromName(ctx.Client, ":moneybag:");
            DiscordEmoji partei = DiscordEmoji.FromName(ctx.Client, ":detective_tone2:");
            DiscordEmoji panther = DiscordEmoji.FromName(ctx.Client, ":wolf:");
            DiscordEmoji child = DiscordEmoji.FromName(ctx.Client, ":underage:");
            DiscordEmoji horny = DiscordEmoji.FromName(ctx.Client, ":eggplant:");

            if (ctx.Guild.Id == 939186520617783317)
            {
                #region Annies Server
                #region comments
                //DiscordEmoji male = DiscordEmoji.FromName(ctx.Client, ":prince:");
                //DiscordEmoji female = DiscordEmoji.FromName(ctx.Client, ":princess:");
                //DiscordEmoji divers = DiscordEmoji.FromName(ctx.Client, ":crown:");
                //DiscordEmoji baby = DiscordEmoji.FromName(ctx.Client, ":baby:");
                //DiscordEmoji teen = DiscordEmoji.FromName(ctx.Client, ":beer:");
                //DiscordEmoji adult = DiscordEmoji.FromName(ctx.Client, ":tumbler_glass:");
                //DiscordEmoji old = DiscordEmoji.FromName(ctx.Client, ":older_woman:");

                //var msgGender = await ctx.Channel.SendMessageAsync($"**Geschlecht**\n{female} weiblich\n{divers} divers\n{male} mänlich").ConfigureAwait(false);
                //await msgGender.CreateReactionAsync(female).ConfigureAwait(false);
                //await msgGender.CreateReactionAsync(divers).ConfigureAwait(false);
                //await msgGender.CreateReactionAsync(male).ConfigureAwait(false);

                //var msgAge = await ctx.Channel.SendMessageAsync($"**Alter**\n{baby} 12+\n{teen} 16+\n{adult} 18+\n{old} 21+").ConfigureAwait(false);
                //await msgAge.CreateReactionAsync(baby).ConfigureAwait(false);
                //await msgAge.CreateReactionAsync(teen).ConfigureAwait(false);
                //await msgAge.CreateReactionAsync(adult).ConfigureAwait(false);
                //await msgAge.CreateReactionAsync(old).ConfigureAwait(false);
                #endregion

                //Build message
                var msgGames = await ctx.Channel.SendMessageAsync($"**Reagiere hier um deine Rollen zu bekommen:**\n{csgo} CS:GO \n{valo} Valorant\n{lol} LoL\n{phasmo} Phasmophobia").ConfigureAwait(false);
                await msgGames.CreateReactionAsync(csgo).ConfigureAwait(false);
                await msgGames.CreateReactionAsync(valo).ConfigureAwait(false);
                await msgGames.CreateReactionAsync(lol).ConfigureAwait(false);
                await msgGames.CreateReactionAsync(phasmo).ConfigureAwait(false);
                #endregion
            }
            else if (ctx.Guild.Id == 327105561298599946)
            {
                #region Plebhunter
                //Politische Rollen
                var msgPolRoles = await ctx.Channel.SendMessageAsync($"**Wer sich politisch äußern möchte kann hier gerne sich Rollen abholen:**\n{linke} Linke\n{grüne} Grüne\n{afd} AFD\n{fdp} FDP\n{panther} Die grauen Panther\n{partei} Die Partei").ConfigureAwait(false);
                await msgPolRoles.CreateReactionAsync(linke).ConfigureAwait(false);
                await msgPolRoles.CreateReactionAsync(grüne).ConfigureAwait(false);
                await Task.Delay(1000);
                await msgPolRoles.CreateReactionAsync(afd).ConfigureAwait(false);
                await msgPolRoles.CreateReactionAsync(fdp).ConfigureAwait(false);
                await msgPolRoles.CreateReactionAsync(partei).ConfigureAwait(false);
                await msgPolRoles.CreateReactionAsync(panther).ConfigureAwait(false);
                await Task.Delay(1000);

                //Games
                var msgGameRoles = await ctx.Channel.SendMessageAsync($"**Lass mich in Ruhe, ich will zocken!**\n{csgo} CSGO\n{lol} LoL").ConfigureAwait(false);
                await msgGameRoles.CreateReactionAsync(csgo).ConfigureAwait(false);
                await msgGameRoles.CreateReactionAsync(lol).ConfigureAwait(false);
                await Task.Delay(1000);

                //Age Rollen
                var msgAgeRoles = await ctx.Channel.SendMessageAsync($"**Damit sich keiner beschweren kann:**\n{child} Minderjährig\n{horny} Horny on Main").ConfigureAwait(false);
                await msgAgeRoles.CreateReactionAsync(child).ConfigureAwait(false);
                await msgAgeRoles.CreateReactionAsync(horny).ConfigureAwait(false);
                await Task.Delay(1000);
                #endregion
            }
            //Delete executing message
            await ctx.Message.DeleteAsync();
        }
    }
}
