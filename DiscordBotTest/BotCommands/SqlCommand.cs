using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTest.Commands
{
    public class SqlCommand : BaseCommandModule
    {
        private readonly DiscordEmoji yes = DiscordEmoji.FromUnicode("👍");
        private readonly DiscordEmoji no = DiscordEmoji.FromUnicode("👎");
        private readonly DiscordEmoji okay = DiscordEmoji.FromUnicode("👌");

        [Command("SqlAddUser")]
        [Hidden]
        public async Task SqlAddUser(CommandContext ctx, long userId, bool admin, long clanRef = 0, long roleRef = 0)
        {
            DiscordMessage msg;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));

            if (sqlUser != null && sqlUser.ADMIN)
            {
                await Task.Run(() => Functions.Functions.AddUser(sqlUser.LID, userId, admin, clanRef, roleRef));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"User (ID: {userId}) wurde in der Datenbank aufgenommen."
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = "Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlSelectUser")]
        [Hidden]
        public async Task SqlSelectUser(CommandContext ctx, long userId)
        {
            DiscordMessage msg;
            var sqlResult = await Task.Run(() => Functions.Functions.SelectUser(userId));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlResult.LID.ToString().Length != 0)
            {
                msgEmbed.Title = $"ID:\t{sqlResult.LID}\nUSERID:\t{sqlResult.USERID}\nADMIN:\t{sqlResult.ADMIN}";
                msgEmbed.Color = DiscordColor.Green;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed.Title = $"User mit der ID {userId} konnte nicht in der Datenbank gefunden werden.";
                msgEmbed.Color = DiscordColor.DarkRed;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlUpdateUser")]
        [Hidden]
        public async Task SqlUpdatetUser(CommandContext ctx, long userId, bool admin, long clanId, string roleName)
        {
            DiscordMessage msg;
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            var sqlClan = await Task.Run(() => Functions.Functions.SelectClanById(clanId));
            var sqlRole = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlUser != null && sqlUser.ADMIN)
            {
                await Task.Run(() => Functions.Functions.UpdateUser(sqlUser.LID, userId, admin, sqlClan.LID, sqlRole.LID));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"User (ID: {userId}) wurde in der Datenbank aktualisiert."
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlDeleteUser")]
        [Hidden]
        public async Task SqlDeleteUser(CommandContext ctx, long userId)
        {
            DiscordMessage msg;
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser(userId));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlUser.LID.ToString().Length != 0)
            {
                await Task.Run(() => Functions.Functions.DeleteUser(userId));
                msgEmbed.Title = $"User mit der ID {sqlUser.USERID} wurde aus der Datenbank gelöscht.";
                msgEmbed.Color = DiscordColor.Green;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed.Title = $"User mit der ID {userId} konnte nicht in der Datenbank gefunden werden.";
                msgEmbed.Color = DiscordColor.DarkRed;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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


        [Command("SqlAddClan")]
        [Hidden]
        public async Task SqlAddClan(CommandContext ctx, long clanId, string clanName, string clanColor = "#1569a2")
        {
            DiscordMessage msg;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            if (sqlUser != null && sqlUser.ADMIN)
            {
                await Task.Run(() => Functions.Functions.AddClan(sqlUser.LID, clanId, clanName, clanColor));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan {clanName} (ID: {clanId}) wurde in der Datenbank angelegt.",
                    Color = new DiscordColor(clanColor)
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlSelectClanById")]
        [Hidden]
        public async Task SqlSelectClanById(CommandContext ctx, long clanId)
        {
            DiscordMessage msg;
            var sqlResult = await Task.Run(() => Functions.Functions.SelectClanById(clanId));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlResult.LID.ToString().Length != 0)
            {
                msgEmbed.Title = $"ID:\t{sqlResult.LID}\nCLANID:\t{sqlResult.CLANID}\nCLANNAME:\t{sqlResult.CLANNAME}\nCLANCOLOR:\t{sqlResult.CLANCOLOR}";
                msgEmbed.Color = DiscordColor.Green;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed.Title = $"Clan mit der ID {clanId} konnte nicht in der Datenbank gefunden werden.";
                msgEmbed.Color = DiscordColor.DarkRed;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlSelectClanByName")]
        [Hidden]
        public async Task SqlSelectClanByName(CommandContext ctx, string clanName)
        {
            DiscordMessage msg;
            var sqlResult = await Task.Run(() => Functions.Functions.SelectClanByName(clanName));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlResult.LID.ToString().Length != 0)
            {
                msgEmbed.Title = $"ID:\t{sqlResult.LID}\nCLANID:\t{sqlResult.CLANID}\nCLANNAME:\t{sqlResult.CLANNAME}\nCLANCOLOR:\t{sqlResult.CLANCOLOR}";
                msgEmbed.Color = DiscordColor.Green;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed.Title = $"Clan mit dem Name {clanName} konnte nicht in der Datenbank gefunden werden.";
                msgEmbed.Color = DiscordColor.DarkRed;
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlUpdateClan")]
        [Hidden]
        public async Task SqlUpdateClan(CommandContext ctx, long clanId, string clanName, string clanColor)
        {
            DiscordMessage msg;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            if (sqlUser != null && sqlUser.ADMIN)
            {
                await Task.Run(() => Functions.Functions.UpdateClan(sqlUser.LID, clanId, clanName, clanColor));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan {clanName} (ID: {clanId}) wurde in der Datenbank aktualisiert.",
                    Color = new DiscordColor(clanColor)
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlDeleteClan")]
        [Hidden]
        public async Task SqlDeleteClan(CommandContext ctx, long clanId)
        {
            DiscordMessage msg;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            if (sqlUser != null && sqlUser.ADMIN)
            {
                await Task.Run(() => Functions.Functions.DeleteClan(clanId));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan mit der ID: {clanId} wurde aus der Datenbank gelöscht.",
                    Color = DiscordColor.Green
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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


        [Command("SqlAddRole")]
        [Hidden]
        public async Task SqlAddRole(CommandContext ctx, long roleId, string roleName)
        {
            DiscordMessage msg;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            if (sqlUser != null && sqlUser.ADMIN)
            {
                await Task.Run(() => Functions.Functions.AddRole(sqlUser.LID, roleId, roleName));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Rolle {roleName} (ID: {roleId}) wurde in der Datenbank angelegt.",
                    Color = DiscordColor.Green
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlSelectRoleByName")]
        [Hidden]
        public async Task SqlSelectRoleByName(CommandContext ctx, string roleName)
        {
            DiscordMessage msg;
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            var sqlRole = await Task.Run(() => Functions.Functions.SelectRoleByName(roleName));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlUser != null && sqlUser.ADMIN)
            {
                if (sqlRole.LID.ToString().Length != 0)
                {
                    msgEmbed.Title = $"ID:\t{sqlRole.LID}\nCLANID:\t{sqlRole.ROLEID}\nCLANNAME:\t{sqlRole.ROLENAME}";
                    msgEmbed.Color = DiscordColor.Green;
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                }
                else
                {
                    msgEmbed.Title = $"Role mit dem Namen {roleName} konnte nicht in der Datenbank gefunden werden.";
                    msgEmbed.Color = DiscordColor.DarkRed;
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                }
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlUpdateRole")]
        [Hidden]
        public async Task SqlUpdateRole(CommandContext ctx, long roleId, string roleName)
        {
            DiscordMessage msg;
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlUser != null && sqlUser.ADMIN)
            {
                //Errorhandling bezüglich Referenzen implementieren
                await Task.Run(() => Functions.Functions.UpdateRole(sqlUser.LID, roleId, roleName));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Role {roleName} (ID: {roleId}) wurde in der Datenbank aktualisiert.",
                    Color = DiscordColor.Green
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

        [Command("SqlDeleteRole")]
        [Hidden]
        public async Task SqlDeleteRole(CommandContext ctx, long roleId, string roleName)
        {
            DiscordMessage msg;
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            var msgEmbed = new DiscordEmbedBuilder();

            if (sqlUser != null && sqlUser.ADMIN)
            {
                //Errorhandling bezüglich Referenzen implementieren
                await Task.Run(() => Functions.Functions.DeleteRole(roleId));
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Role mit der ID: {roleId} wurde in der Datenbank gelöscht.",
                    Color = DiscordColor.Green
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Du bist leider nicht berechtigt so eine Aktion durchzuführen, sorry... (╯︵╰,)",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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

    }
}
