using System;
using System.IO;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Collections.Generic;

namespace DiscordBotTest.Commands
{
    public class DatabaseCommands : BaseCommandModule
    {
        private readonly DiscordEmoji yes = DiscordEmoji.FromUnicode("👍");
        private readonly DiscordEmoji no = DiscordEmoji.FromUnicode("👎");
        private readonly DiscordEmoji okay = DiscordEmoji.FromUnicode("👌");

        #region TestCommands

        [Command("PingUser")]
        public async Task PingUser(CommandContext ctx, string userid)
        {
            
            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Clan",
                Color = DiscordColor.CornflowerBlue
            };

            msgEmbed.AddField("test",$"{ctx.Message.Author.Mention}");

            var msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
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






        #region ClanCommands

        [Command("AddClan")]
        [Description("Fügt ein Clan dem Discord und der Datenbank hinzu. Schlägt fehl wenn der Clanname schon vergeben ist.")]
        public async Task AddClan(CommandContext ctx,
                                [Description("Clanname, !CaSe SeNsItIvE!")] string clanName,
                                [Description("[Optional] Clanfarbe in Hex zB. #FFFFFF (Default = #1569a2)")] string clanColor = "#1569a2")
        {
            //Initialize lokal variable
            DiscordMessage msg;

            //Get Clan DB Dataset
            var clan = await Task.Run(() => Functions.Functions.SelectClanByName(clanName));
            
            //Create new Discordrole
            var newClan = await ctx.Guild.CreateRoleAsync(clanName, null, new DiscordColor(clanColor)).ConfigureAwait(false); //TODO Permissions abklären!

            //Get Admin DB User Dataset
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));

            //Check if Clan Dataset already exists
            if (clan.LID < 1)
            {
                //If Clan doesn't already exists

                //Create Clan in DB
                await Task.Run(() => Functions.Functions.AddClan(sqlUser.LID, (long)newClan.Id, clanName, clanColor));

                //Send conformation message to User
                var msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan {newClan.Name} wurde angelegt (ID: {newClan.Id})",
                    Color = newClan.Color

                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                //If CLan already exists

                //Send error message to User
                var msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Clan {clanName} konnte nicht angelegt werden. Grund: Clan schon vorhanden (Name: {clan.CLANNAME}; ID: {clan.CLANID}).",
                    Color = DiscordColor.DarkRed
                };
                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }

            //React to own message
            await msg.CreateReactionAsync(okay).ConfigureAwait(false);
            
            //Get User reaction
            var ia = ctx.Client.GetInteractivity();
            var result = await ia.WaitForReactionAsync(
                x => x.Message == msg &&
                x.User == ctx.User &&
                x.Emoji == okay).ConfigureAwait(false);
            
            //Check User reaction
            if (result.Result.Emoji == okay)
            {
                //Delete own message and the one who triggered the execution
                await msg.DeleteAsync().ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }

        [Command("SelectClan")]
        [Description("Gibt allgemeine Claninfromationen aus")]
        public async Task SelectClan(CommandContext ctx,
                                    [Description("Clanname, !CaSe SeNsItIvE!")] string clanName)
        {
            DiscordMessage msg = null;
            var clan = await Task.Run(() => Functions.Functions.SelectClanByName(clanName));
            var clanList = await Task.Run(() => Functions.Functions.CountClanMember(clan.CLANID, new List<long>{1, 2})); //TODO List evtl anpassen da theotisch Probleme


            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Clanname: {clan.CLANNAME}\nClanid: {clan.CLANID}\nClancolor: {clan.CLANCOLOR}\nAnzahl Member: {clanList[0].Item2 + clanList[1].Item2}\nLeader: {clanList[0].Item2}\nMember: {clanList[1].Item2}",
                Color = new DiscordColor(clan.CLANCOLOR)
            };

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

        [Command("SelectClanMember")]
        [Description("Gibt allgemeine Claninfromationen aus")]
        public async Task SelectClanMember(CommandContext ctx,
            [Description("Clanname, !CaSe SeNsItIvE!")] string clanName)
        {
            DiscordMessage msg = null;
            var roles = new List<long>();
            var clan = await Task.Run(() => Functions.Functions.SelectClanByName(clanName));
            var clanMember = await Task.Run(() => Functions.Functions.SelectClanUser(clan.CLANID)); //TODO List evtl anpassen da theotisch Probleme


            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Clan {clan.CLANNAME}",
                Color = new DiscordColor(clan.CLANCOLOR)
            };

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
        [Description("Entfernt ein Clan aus der Datenbank & Discord. 'Löscht' auch alle Clanuser in der Datenbank.")]
        public async Task DeleteClan(CommandContext ctx,
                                    [Description("Discord Role ID des Clans")] ulong clanId)
        {
            //Verify action by the user
            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du den Clan {ctx.Guild.GetRole(clanId).Name} wirklich löschen?",
                Color = ctx.Guild.GetRole(clanId).Color
            };

            //Send message
            var deleteMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

            //React to own message
            await deleteMsg.CreateReactionAsync(yes).ConfigureAwait(false);
            await deleteMsg.CreateReactionAsync(no).ConfigureAwait(false);

            //Get User reaction
            var ia = ctx.Client.GetInteractivity();
            var reactionDelete = await ia.WaitForReactionAsync(
                x => x.Message == deleteMsg &&
                x.User == ctx.User &&
                (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

            //Check User reaction
            if (reactionDelete.Result.Emoji == yes)
            {
                //Get Admin User Dataset
                var user = await Task.Run(() => Functions.Functions.SelectClanById((long)ctx.Member.Id));

                //Get Clan DB Dataset
                var clan = await Task.Run(() => Functions.Functions.SelectClanById((long)clanId));

                //Get all Clan Members which reference the provided Clan
                var clanMember = await Task.Run(() => Functions.Functions.SelectClanUser(clan.LID));

                //Loop through each Clan Member
                foreach (var m in clanMember)
                {
                    //Get Clan Member User Dataset
                    var sqlmember = await Task.Run(() => Functions.Functions.SelectUser(m.USERID));

                    //Update User and remove Clan reference
                    await Task.Run(() => Functions.Functions.UpdateUser(user.LID, m.USERID, m.ADMIN, 0, 0)); //Delete oder Update, das ist hier die Frage... 🤔

                    //Get Discord Member from DB User Dataset
                    var member = await ctx.Guild.GetMemberAsync((ulong)m.USERID);

                    //Remove Clansortierungsrolle from Discorduser
                    await member.RevokeRoleAsync(ctx.Guild.GetRole(593499616322781194)).ConfigureAwait(false); //TODO RoleSortId nicht hardcoden
                }

                //Delete CLan Role from Discord Server
                await ctx.Guild.GetRole(clanId).DeleteAsync().ConfigureAwait(false);

                //Send conformation message to User
                msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Der Clan {clan.CLANNAME} (ID {clan.CLANID}) wurde auf dem Server gelöscht.\nWeiterhin wurden bei {clanMember.Count} Usern die Clan & Clansortierungsrolle entfernt.",
                    Color = DiscordColor.Green
                };
                var confirmMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

                //react to own Message
                await confirmMsg.CreateReactionAsync(okay).ConfigureAwait(false);

                //Get User reaction
                var reactionConfirm = await ia.WaitForReactionAsync(
                    x => x.Message == confirmMsg &&
                    x.User == ctx.User &&
                    (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

                //Check User reaction
                if (reactionConfirm.Result.Emoji == okay)
                {
                    //Delete own message
                    await confirmMsg.DeleteAsync().ConfigureAwait(false);
                }
            }

            //Delete own message and the one who triggered the execution
            await deleteMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("GetClanId")]
        [Description("Gibt die DiscordID eines Clans/einer Rolle aus.\n(Durchsucht den Discord nach einer Rolle mit dem Namen)")]
        public async Task GetClanId(CommandContext ctx,
                                    [Description("Clanname, !CaSe SeNsItIvE!")] string clanName)
        {
            DiscordMessage msg = null;

            //Loop though all Roles in Server
            foreach (var role in ctx.Guild.Roles)
            {
                //Check if Rolename is same as provided
                if (role.Value.Name.Equals(clanName))
                {
                    //Output User message
                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Title = $"Clan {role.Value.Name} hat folgende ID: {role.Value.Id}).",
                        Color = ctx.Guild.GetRole(role.Value.Id).Color
                    };
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                }
            }

            //React to  own message
            await msg.CreateReactionAsync(okay).ConfigureAwait(false);

            //Get user reaction
            var ia = ctx.Client.GetInteractivity();
            var result = await ia.WaitForReactionAsync(
                x => x.Message == msg &&
                x.User == ctx.User &&
                x.Emoji == okay).ConfigureAwait(false);

            //Check User reaction
            if (result.Result.Emoji == okay)
            {
                //Delete own message and the one who triggered the execution
                await msg.DeleteAsync().ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }

        #endregion

        #region UserCommands

        [Command("AddClanUser")]
        [Description("Ordnet User einem Clan zu. Ist User noch nicht in Datenbank wird er angelegt, sonst nur aktualisiert. Schlägt fehl wenn Clanname nicht in der DB ist.")]
        public async Task AddClanUser(CommandContext ctx,
                                    [Description("Discorduserid")] ulong userId,
                                    [Description("Clanname, !CaSe SeNsItIvE!")] string clanName,
                                    [Description("[Optional] Bestimmt die Rolle im Clan. Werte: Member/Leader (Default = Member)")]string role = "Member",
                                    [Description("[Optional] Bestimmt ob User Datenbankeinträge direkt bearbeiten kann. (Default = false)")] bool admin = false)
        {
            //Lookup clan in DD
            var sqlClan = await Task.Run(() => Functions.Functions.SelectClanByName(clanName));
            
            //Initialize local variables
            var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);
            DiscordMessage joinMsg = null;

            if (sqlClan.LID == 0)
            {
                //If no Clan is found
                var msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Der Clan konnte nicht in der Datenbank gefunden werden, Clanname muss genau angegeben werden!",
                    Color = DiscordColor.DarkRed
                };
            }
            else
            {
                //If Clan is found

                //Verify action by the user
                var msgEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Möchtest du den User {member.Username} ({member.Nickname}) dem Clan {sqlClan.CLANNAME} mit der Rolle {role} hinzufügen?",
                    Color = new DiscordColor(sqlClan.CLANCOLOR)
                };

                //Send message to User
                joinMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

                //React to  own message
                await joinMsg.CreateReactionAsync(yes).ConfigureAwait(false);
                await joinMsg.CreateReactionAsync(no).ConfigureAwait(false);

                //Get user reaction
                var ia = ctx.Client.GetInteractivity();
                var reaction = await ia.WaitForReactionAsync(
                    x => x.Message == joinMsg &&
                         x.User == ctx.User &&
                         (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

                //Check User reaction
                if (reaction.Result.Emoji == yes)
                {
                    //User conffirms input
                    
                    //Get Admin DB User Dataset
                    var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)userId));
                    
                    //Get DB Role Dataset
                    var sqlRole = await Task.Run(() => Functions.Functions.SelectRoleByName(role));
                    
                    //Get DB User Dataset
                    var user = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));

                    //Compare User ID to DB
                    if (sqlUser.LID < 1)
                    {
                        //If User is new to DB
                        await Task.Run(() => Functions.Functions.AddUser(user.LID, (long)userId, admin, sqlClan.LID, sqlRole.LID));
                    }
                    else
                    {
                        //If User is already in DB
                        await Task.Run(() => Functions.Functions.UpdateUser(user.LID, sqlUser.USERID, sqlUser.ADMIN, sqlClan.LID, sqlRole.LID));
                    }

                    //Get the Discord Role ID
                    var discordRole = ctx.Guild.GetRole((ulong)sqlRole.ROLEID);

                    //If Leader give Leader Role
                    if (sqlRole.ROLENAME.Equals("Leader"))
                    {
                        var clanLeaderRole = ctx.Guild.GetRole((ulong)sqlRole.ROLEID);
                        await member.GrantRoleAsync(clanLeaderRole).ConfigureAwait(false);
                    }

                    //Get general Clan Roles
                    var clanRole = ctx.Guild.GetRole((ulong)sqlClan.CLANID);
                    var clanSortRole = ctx.Guild.GetRole(801763113161719828);  //593499616322781194

                    //Grant Clan Roles
                    await member.GrantRoleAsync(clanRole).ConfigureAwait(false);
                    await member.GrantRoleAsync(clanSortRole).ConfigureAwait(false);
                }
            }

            //Delete own message and the one who triggered the execution
            await joinMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("AddLeader")]
        [Description("Gibt einem User die Clanleaderrolle & aktualiert den Benutzer in der Datenbank.")]
        public async Task AddLeader(CommandContext ctx,
                                    [Description("Discorduserid")] ulong userId)
        {
            var sqlRole = await Task.Run(() => Functions.Functions.SelectRoleByName("Leader"));
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du dem User {member.Username} ({member.Nickname}) die Clanleaderrolle geben?"
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
                var roleLeader = ctx.Guild.GetRole((ulong)sqlRole.ROLEID);
                await member.GrantRoleAsync(roleLeader).ConfigureAwait(false);
            }

            await joinMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

        }

        [Command("RemoveClanUser")]
        public async Task RemoveClanUser(CommandContext ctx,
                                        [Description("Discord User ID")] ulong userId,
                                        [Description("Clan Role ID")] string clanName)
        {

            var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);
            var sqlUser = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));
            var sqlClan = await Task.Run(() => Functions.Functions.SelectClanByName(clanName));

            var msgEmbed = new DiscordEmbedBuilder
            {
                Title = $"Möchtest du den User {member.Username} ({member.Nickname}) aus dem Clan {clanName} entfernen?",
                Color = new DiscordColor(sqlClan.CLANCOLOR)
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
                //Get User Db Dataset
                var sqlMember = await Task.Run(() => Functions.Functions.SelectUser((long)ctx.Member.Id));

                //Update Clan reference
                await Task.Run(() => Functions.Functions.UpdateUser(sqlUser.LID, sqlMember.USERID, sqlMember.ADMIN, 0, 0));

                //Remove Clan Role
                await member.RevokeRoleAsync(ctx.Guild.GetRole((ulong)sqlClan.CLANID)).ConfigureAwait(false);

                //Remove Clansortierungsrolle
                await member.RevokeRoleAsync(ctx.Guild.GetRole(593499616322781194)).ConfigureAwait(false); //TODO Hartgecodede Rolle ersetzen
            }

            await joinMsg.DeleteAsync().ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);

        }

#endregion
    }
}
