using System;
using System.IO;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace DiscordBotTest.Commands
{
    public class DatabaseCommands : BaseCommandModule
    {
        private readonly DiscordEmoji yes = DiscordEmoji.FromUnicode("👍");
        private readonly DiscordEmoji no = DiscordEmoji.FromUnicode("👎");
        private readonly DiscordEmoji okay = DiscordEmoji.FromUnicode("👌");

        #region ClanCommands

        [Command("AddClan")]
        public async Task AddClan(CommandContext ctx,
                                [Description("Clanname")] string clanName,
                                [Description("Clanfarbe (Hex => #FFFFFF)")] string clanColor = "#1569a2")
        {
            DiscordMessage msg;

            var clan = await Task.Run(() => Functions.Functions.GetClanByName(clanName));
            var newClan = await ctx.Guild.CreateRoleAsync(clanName, null, new DiscordColor(clanColor)).ConfigureAwait(false);
            var sqlUser = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));

            if (clan.LID.ToString().Length > 0)
            {
                await Task.Run(() => Functions.Functions.AddClan(sqlUser.LID, (long)newClan.Id, clanName, clanColor));

                var msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan {newClan.Name} wurde angelegt (ID: {newClan.Id})",
                    Color = newClan.Color

                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                var msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan {clanName} konnte nicht angelegt werden. Grund: Clan schon vorhanden (Name: {clan.CLANNAME}; ID: {clan.CLANID}).",
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
                var clan = await Task.Run(() => Functions.Functions.GetClanById((long)clanId));
                var clanMember = await Task.Run(() => Functions.Functions.GetClanUser(clan.LID));
                var user = await Task.Run(() => Functions.Functions.GetClanById((long)ctx.Member.Id));
                foreach (var m in clanMember)
                {
                    var member = await Task.Run(() => Functions.Functions.GetUser(m.USERID));
                    await Task.Run(() => Functions.Functions.UpdateUser(user.LID, member.USERID, member.ADMIN, 0, member.REF_CLANROLE ?? 0));
                }
                await ctx.Guild.GetRole(clanId).DeleteAsync().ConfigureAwait(false);
            }

            await deleteMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

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

        #endregion

        #region UserCommands

        [Command("AddClanUserById")]
        public async Task AddClanUserById(CommandContext ctx,
                                        [Description("ClanID")] ulong clanId,
                                        [Description("UserID")] ulong userId,
                                        [Description("Admin")] bool admin = false,
                                        [Description("Role (Member/Leader)")]string role = "Member")
        {
            var clan = ctx.Guild.GetRole(clanId);
            var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du den User {member.Username} ({member.Nickname}) dem Clan {clan.Name} mit der Rolle {role} hinzufügen?",
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
                var sqlUser = await Task.Run(() => Functions.Functions.GetUser((long)userId));
                var sqlClan = await Task.Run(() => Functions.Functions.GetClanById((long)clanId));
                var sqlRole = await Task.Run(() => Functions.Functions.GetRoleIdByName(role));
                var user = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));
 
                if (sqlUser.LID < 1)
                {
                    await Task.Run(() => Functions.Functions.AddUser(user.LID, (long)userId, admin, sqlClan.LID, sqlRole.LID));
                    Console.WriteLine("new User");
                }
                else
                {
                    await Task.Run(() => Functions.Functions.UpdateUser(user.LID, sqlUser.USERID, sqlUser.ADMIN, sqlClan.LID, sqlRole.LID));
                    Console.WriteLine("update User");
                }
                var r = ctx.Guild.GetRole((ulong)sqlRole.ROLEID);
                
                if (sqlRole.ROLENAME.Equals("Leader"))
                {
                    var r2 = await Task.Run(() => Functions.Functions.GetRoleIdByName("Member"));
                    var rm = ctx.Guild.GetRole((ulong)sqlRole.ROLEID);
                    await member.GrantRoleAsync(r).ConfigureAwait(false);
                    await member.GrantRoleAsync(rm).ConfigureAwait(false);
                }
                else
                {
                    await member.GrantRoleAsync(r).ConfigureAwait(false);
                }
            }
            await joinMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("AddLeader")]
        public async Task AddLeader(CommandContext ctx, ulong userId, ulong clanId)
        {
            //var config = Functions.Functions.ReadConfig();
            var sqlRole = await Task.Run(() => Functions.Functions.GetRoleIdByName("Member"));
            var sqlUser = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));
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
                await Task.Run(() => Functions.Functions.UpdateUser(sqlUser.LID, (long)userId, sqlUser.ADMIN, sqlUser.REF_CLANID ?? 0, sqlRole.LID));
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

        #endregion

        #region SQLCommands

        [Command("SqlAddUser")]
        public async Task SqlAddUser(CommandContext ctx, long userId, bool admin) //TODO: Add Clan & Role
        {
            DiscordMessage msg = null;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));

            if (sqlUser != null && sqlUser.ADMIN)
            {
                await Task.Run(() => Functions.Functions.AddUser(sqlUser.LID, userId, admin, 0, 0));
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
        public async Task SqlSelectUser(CommandContext ctx, long userId)
        {
            DiscordMessage msg = null;
            var sqlResult = await Task.Run(() => Functions.Functions.GetUser(userId));
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
        public async Task SqlUpdatetUser(CommandContext ctx, long userId, bool admin, long clanId, string roleName)
        {
            DiscordMessage msg = null;
            var sqlUser = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));
            var sqlClan = await Task.Run(() => Functions.Functions.GetClanById(clanId));
            var sqlRole = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));
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

        [Command("SqlAddClan")]
        public async Task SqlAddClan(CommandContext ctx, long clanId, string clanName, string clanColor = "#FFFFFF")
        {
            DiscordMessage msg = null;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));
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
        public async Task SqlSelectClanById(CommandContext ctx, long clanId)
        {
            DiscordMessage msg = null;
            var sqlResult = await Task.Run(() => Functions.Functions.GetClanById(clanId));
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
        public async Task SqlSelectClanByName(CommandContext ctx, string clanName)
        {
            DiscordMessage msg = null;
            var sqlResult = await Task.Run(() => Functions.Functions.GetClanByName(clanName));
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

        [Command("SqlAddRole")]
        public async Task SqlAddRole(CommandContext ctx, long roleId, string roleName)
        {
            DiscordMessage msg = null;
            var msgEmbed = new DiscordEmbedBuilder();
            var sqlUser = await Task.Run(() => Functions.Functions.GetUser((long)ctx.Member.Id));
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

        #endregion
    }
}
