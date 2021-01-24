using System;
using System.IO;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;

namespace DiscordBotTest.Commands
{
    public class DatabaseCommands : BaseCommandModule
    {
        private readonly DiscordEmoji yes = DiscordEmoji.FromUnicode("👍");
        private readonly DiscordEmoji no = DiscordEmoji.FromUnicode("👎");
        private readonly DiscordEmoji okay = DiscordEmoji.FromUnicode("👌");

        [Command("AddClan")]
        public async Task AddClan(CommandContext ctx,
                                [Description("Clanname")] string name,
                                [Description("Clanfarbe (Hex => #FFFFFF)")] string color)
        {
            var f = false;
            var c = new DiscordColor(color);
            DiscordMessage msg = null;

            foreach (var role in ctx.Guild.Roles)
            { 
                if(role.Value.Name == name){
                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Clan {name} konnte nicht angelegt werden. Grund: Clan schon vorhanden (Name: {role.Value.Name}; ID: {role.Value.Id}).",
                    };
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                    f = true;
                }
            }





            if (!f)
            {
                var reaction = await ctx.Guild.CreateRoleAsync(name, null, c).ConfigureAwait(false);

                var msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan {reaction.Name} wurde angelegt (ID: {reaction.Id})",
                    Color = reaction.Color
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }

            #region MessageReaction
            await msg.CreateReactionAsync(okay).ConfigureAwait(false);
            
            var ia = ctx.Client.GetInteractivity();

            var result = await ia.WaitForReactionAsync(
                x => x.Message == msg &&
                x.User == ctx.User &&
                x.Emoji == okay).ConfigureAwait(false);
            
            if (result.Result.Emoji == okay)
            {
                await msg.DeleteAsync().ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
            #endregion
        }

        [Command("DeleteClan")]
        public async Task DeleteClan(CommandContext ctx,
                                    [Description("Clanid")] ulong clanId)
        {
            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du den Clan {ctx.Guild.GetRole(clanId).Name} wirklich löschen?",
                Color = ctx.Guild.GetRole(clanId).Color
            };

            var deleteMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

            await deleteMsg.CreateReactionAsync(yes).ConfigureAwait(false);
            await deleteMsg.CreateReactionAsync(no).ConfigureAwait(false);

            var ia = ctx.Client.GetInteractivity();

            var reaction = await ia.WaitForReactionAsync(
                x => x.Message == deleteMsg &&
                x.User == ctx.User &&
                (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);


            if (reaction.Result.Emoji == yes)
            {
                await ctx.Guild.GetRole(clanId).DeleteAsync().ConfigureAwait(false);
            }

            await deleteMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

        //[Command("UpdateClan")]
        //public async Task UpdateClan(CommandContext ctx)
        //{

        //}

        [Command("GetClanId")]
        public async Task GetClanId(CommandContext ctx,
                                    [Description("Clanname")] string name)
        {
            DiscordMessage msg = null;

            foreach (var role in ctx.Guild.Roles)
            {
                if (role.Value.Name == name)
                {
                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Clan {role.Value.Name} hat folgende ID: {role.Value.Id}).",
                        Color = ctx.Guild.GetRole(role.Value.Id).Color
                    };
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                }
            }
        
            await msg.CreateReactionAsync(okay).ConfigureAwait(false);

            var ia = ctx.Client.GetInteractivity();

            var result = await ia.WaitForReactionAsync(
                x => x.Message == msg &&
                x.User == ctx.User &&
                x.Emoji == okay).ConfigureAwait(false);

            if (result.Result.Emoji == okay)
            {
                await msg.DeleteAsync().ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }

        [Command("AddMemberById")]
        public async Task AddClanMemberById(CommandContext ctx,
                                        [Description("ClanID")] ulong clanId,
                                        [Description("UserID")] ulong userId)
        {
            var clan = ctx.Guild.GetRole(clanId);
            DiscordMember member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du den User {member.Username} ({member.Nickname}) dem Clan {clan.Name} hinzufügen?",
                Color = ctx.Guild.GetRole(clanId).Color
            };

            var joinMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

            await joinMsg.CreateReactionAsync(yes).ConfigureAwait(false);
            await joinMsg.CreateReactionAsync(no).ConfigureAwait(false);

            var ia = ctx.Client.GetInteractivity();

            var reaction = await ia.WaitForReactionAsync(
                x => x.Message == joinMsg &&
                     x.User == ctx.User &&
                     (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

            if (reaction.Result.Emoji == yes)
            {
                await member.GrantRoleAsync(clan).ConfigureAwait(false);
            }

            await joinMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

        }

        [Command("AddLeader")]
        public async Task AddLeader(CommandContext ctx, ulong userId, ulong clanId)
        {
            var config = Functions.Functions.ReadConfig();
            var clanLeader = ctx.Guild.GetRole(config.Roles.ClanLeader);
            var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du dem User {member.Username} ({member.Nickname}) die Clanleaderrolle geben?",
                Color = ctx.Guild.GetRole(clanId).Color
            };

            var joinMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

            await joinMsg.CreateReactionAsync(yes).ConfigureAwait(false);
            await joinMsg.CreateReactionAsync(no).ConfigureAwait(false);

            var ia = ctx.Client.GetInteractivity();

            var reaction = await ia.WaitForReactionAsync(
                x => x.Message == joinMsg &&
                     x.User == ctx.User &&
                     (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

            if (reaction.Result.Emoji == yes)
            {
                await member.GrantRoleAsync(clanLeader).ConfigureAwait(false);
            }

            await joinMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

        }

        [Command("RemoveMember")]
        public async Task RemoveClanMember(CommandContext ctx,
                                        [Description("ClanID")] ulong clanId,
                                        [Description("UserID")] ulong userId)
        {
            var clan = ctx.Guild.GetRole(clanId);
            DiscordMember member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du den User {member.Username} ({member.Nickname}) aus dem Clan {clan.Name} entfernen?",
                Color = ctx.Guild.GetRole(clanId).Color
            };

            var joinMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

            await joinMsg.CreateReactionAsync(yes).ConfigureAwait(false);
            await joinMsg.CreateReactionAsync(no).ConfigureAwait(false);

            var ia = ctx.Client.GetInteractivity();

            var reaction = await ia.WaitForReactionAsync(
                x => x.Message == joinMsg &&
                     x.User == ctx.User &&
                     (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

            if (reaction.Result.Emoji == yes)
            {
                await member.RevokeRoleAsync(clan).ConfigureAwait(false);
            }

            await joinMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

        }

        [Command("SqlAddUser")]
        public async Task SqlAddUser(CommandContext ctx, ulong userIdInsert, ulong userId, bool admin)
        {
            await Task.Run(() => Functions.Functions.AddUser(userIdInsert, userId, admin));

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"User (ID: {userId}) wurde in der Datenbank aufgenommen."
            };

            await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
        }

        [Command("SqlSelectUser")]
        public async Task SqlSelectUser(CommandContext ctx, ulong userId)
        {
            var sqlResult = await Task.Run(() => Functions.Functions.GetUserById(userId));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlResult != null)
            {
                msgEmbed.Title = $"{sqlResult[0].ADMIN}";
                await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed.Title = $"User mit der ID {userId} konnte nicht in der Datenbank gefunden werden.";
                await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
        }

        [Command("SqlGetUserById")]
        public async Task SqlGetUserById(CommandContext ctx, ulong userId)
        {
            var dummy = Functions.Functions.GetClanById(userId);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"{dummy}"
            };

            await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
        }

        [Command("SqlAddClan")]
        public async Task SqlAddClan(CommandContext ctx, ulong clanId)
        {
            var dummy = Functions.Functions.GetClanById(clanId);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"{dummy}"
            };

            await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

        }

        [Command("SqlGetClanById")]
        public async Task SqlGetClanById(CommandContext ctx, ulong clanId)
        {
            var dummy = Functions.Functions.GetClanById(clanId);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"{dummy}"
            };

            await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

        }
    }
}
