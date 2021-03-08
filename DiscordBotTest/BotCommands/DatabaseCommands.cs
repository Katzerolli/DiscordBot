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
using System.Diagnostics;
using System.Net.Mime;
using DSharpPlus;

namespace DiscordBotTest.Commands
{
    public class DatabaseCommands : BaseCommandModule
    {
        private readonly DiscordEmoji yes = DiscordEmoji.FromUnicode("👍");
        private readonly DiscordEmoji no = DiscordEmoji.FromUnicode("👎");
        private readonly DiscordEmoji okay = DiscordEmoji.FromUnicode("👌");
        private readonly ConfigJson config = Functions.Functions.ReadConfig();

        #region TestCommands

        [Command("SetStatus")]
        [Hidden]
        public async Task SetStatus(CommandContext ctx, string type = "Watching", [RemainingText]string status = "over the seven Seas")
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                //Define ActivityType
                var activityType = type.Equals("Watching") ? ActivityType.Watching :
                    type.Equals("Competing") ? ActivityType.Competing :
                    type.Equals("ListeningTo") ? ActivityType.ListeningTo :
                    type.Equals("Streaming") ? ActivityType.Streaming : ActivityType.Playing;
                var activity = new DiscordActivity($"{status}", activityType);
                await ctx.Client.UpdateStatusAsync(activity, UserStatus.Online);
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }

        [Command("ResetDiscord")]
        [Hidden]
        public async Task ResetDiscord(CommandContext ctx)
        {
            if (ctx.Member.Id == 326776845171294209)
            {
                var channels = new List<ulong>
                {
                    803314718424956928,
                    327105563236237312
                };

                var roles = new List<ulong>
                {
                    815999841237729340,
                    815990668282429440,
                    676725628803874839,
                    801757058302083092,
                    801763113161719828,
                    327105561298599946
                };

                foreach (var c in ctx.Guild.Channels)
                {
                    if (!channels.Contains(c.Value.Id))
                    {
                        //await c.Value.DeleteAsync();
                    }
                }

                foreach (var r in ctx.Guild.Roles)
                {
                    if (!roles.Contains(r.Value.Id))
                    {
                        //await r.Value.DeleteAsync();
                    }
                }
            }
        }

        [Command("Test")]
        [Hidden]
        public async Task Test(CommandContext ctx)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                var text = string.Empty;
                var text2 = string.Empty;
                var c = 1;
                var userlist = new List<ulong>{326776845171294209, 210172179403374593, 147049465373655040, 313983357036003328, 223847553517486081, 432122694696173568};

                var msgEmbed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(config.StoredValues.DefaultColor)
                };

                foreach (var u in userlist)
                {
                    if (c < 3)
                    {
                        text = text + $"{ctx.Guild.GetMemberAsync(u).Result.DisplayName} ({ctx.Guild.GetMemberAsync(u).Result.Mention})\n";
                        c++;
                    }
                    else
                    {
                        text2 = text2 + $"{ctx.Guild.GetMemberAsync(u).Result.DisplayName} ({ctx.Guild.GetMemberAsync(u).Result.Mention})\n";
                        c++;
                    }
                }

                //text =
                //    $"{ctx.Guild.GetMemberAsync(326776845171294209).Result.DisplayName} ({ctx.Guild.GetMemberAsync(326776845171294209).Result.Mention})\n{ctx.Guild.GetMemberAsync(210172179403374593).Result.DisplayName} ({ctx.Guild.GetMemberAsync(210172179403374593).Result.Mention})\n";
                msgEmbed.AddField("Test", text);
                msgEmbed.AddField("Test", text2);
                var msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

                //foreach (var u in userlist)
                //{
                //    text = text + $"{ctx.Guild.GetMemberAsync((ulong)u).Result.DisplayName} ({ ctx.Guild.GetMemberAsync((ulong)u).Result.Mention})\n";
                //}
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }

        #endregion

        #region ClanCommands

        [Command("AddClan")]
        [Description("Fügt ein Clan dem Discord und der Datenbank hinzu. Schlägt fehl wenn der Clanname schon vergeben ist.")]
        public async Task AddClan(CommandContext ctx,
                                [Description("Clanname, !CaSe SeNsItIvE!")] string clanName,
                                [Description("[Optional] Clanfarbe in Hex zB. #FFFFFF (Default = #1569a2)")] string clanColor = "#1569a2",
                                [Description("[Optional] Bool ob Clanbereich erstellt werden soll (Default = false)")] bool createClanArea = false)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                //Initialize lokal variable
                DiscordMessage msg;

                //Get Clan DB Dataset
                var clan = await Task.Run(() => Functions.Functions.SelectClanByName(ctx, clanName));

                //Check if Clan Dataset already exists
                if (clan.LID < 1)
                {
                    //Create new Discordrole
                    var newClan = await ctx.Guild.CreateRoleAsync(clanName, null, new DiscordColor(clanColor)).ConfigureAwait(false);

                    //Get Admin DB User Dataset
                    var sqlUser = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)ctx.Member.Id));

                    //Create Clan in DB
                    await Task.Run(() => Functions.Functions.AddClan(ctx, sqlUser.LID, (long)newClan.Id, clanName, clanColor));

                    if (createClanArea)
                    {
                        await Task.Run(() => Functions.Functions.CreateClanArea(ctx, clanName, newClan));
                    }

                    //Send conformation message to User
                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor(config.StoredValues.DefaultColor)
                    };
                    msgEmbed.AddField("Add Clan", $"Clan {newClan.Name} wurde angelegt (ID: {newClan.Id})");
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                }
                else
                {
                    //Send error message to User
                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.DarkRed
                    };
                    msgEmbed.AddField("Add Clan", $"Clan {clanName} konnte nicht angelegt werden. Grund: Clan schon vorhanden (Name: {clan.CLANNAME}; ID: {clan.CLANID}).");
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
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }

        [Command("SelectClanMember")]
        [Description("Gibt allgemeine Claninfromationen aus")]
        public async Task SelectClanMember(CommandContext ctx,
            [Description("Clanname, !CaSe SeNsItIvE!")] string clanName)
        {

            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                //Initialize local variable
                //var roles = new List<long>();
                var text = string.Empty;
                var text2 = string.Empty;
                var c = 1;
                var clan = await Task.Run(() => Functions.Functions.SelectClanByName(ctx, clanName));
                var clanMember = await Task.Run(() => Functions.Functions.SelectClanUser(ctx, clan.LID)); //TODO List evtl anpassen da theotisch Probleme


                var msgEmbed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(config.StoredValues.DefaultColor)
                };
                var msgEmbed2 = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(config.StoredValues.DefaultColor)
                };

                foreach (var m in clanMember)
                {
                    try
                    {
                        if (c <= 25)
                        {
                            text += $"{ctx.Guild.GetMemberAsync((ulong)m.USERID).Result.DisplayName} ({ctx.Guild.GetMemberAsync((ulong)m.USERID).Result.Mention})\n";
                            c++;
                        }
                        else
                        {
                            text2 += $"{ctx.Guild.GetMemberAsync((ulong)m.USERID).Result.DisplayName} ({ctx.Guild.GetMemberAsync((ulong)m.USERID).Result.Mention})\n";
                            c++;
                        }
                    }
                    catch
                    {

                    }

                }
                msgEmbed.AddField($"Clan {clanName} Seite 1:", text);
                var clanMemberMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(true);
                await clanMemberMsg.CreateReactionAsync(okay).ConfigureAwait(false);

                if (c > 24)
                {
                    msgEmbed2.AddField($"Clan {clanName} Seite 2:", text2);
                    var clanMemberMsg2 = await ctx.Channel.SendMessageAsync(embed: msgEmbed2).ConfigureAwait(false);
                    await clanMemberMsg2.CreateReactionAsync(okay).ConfigureAwait(false);
                }
                
                var ia = ctx.Client.GetInteractivity();

                var result = await ia.WaitForReactionAsync(
                    x => x.Message == clanMemberMsg &&
                         x.User == ctx.User &&
                         x.Emoji == okay).ConfigureAwait(false);

                if (result.Result.Emoji == okay)
                {
                    await clanMemberMsg.DeleteAsync().ConfigureAwait(false);
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                }
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }

        [Command("SelectClanlist")]
        [Description("Gibt alle Clans aus")]
        public async Task SelectClanlist(CommandContext ctx)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                DiscordMessage msg;

                var msgEmbed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(config.StoredValues.DefaultColor)
                };

                var sqlClans = await Task.Run(() => Functions.Functions.SelectAllClan(ctx));

                foreach (var c in sqlClans)
                {
                    var memberList = await Task.Run(() => Functions.Functions.CountClanMember(ctx, c.LID, new List<long> {2, 1 }));
                    var (anzahlAll, anzahlLeader, anzahlMember) = new Tuple<int, string, string>(Convert.ToInt32(memberList[0].Item2) + Convert.ToInt32(memberList[1].Item2), memberList[0].Item2, memberList[1].Item2);
                    msgEmbed.AddField(c.CLANNAME, $"__Gesamt:__ {anzahlAll}\n__Leader:__ {anzahlLeader}\n__Member:__ {anzahlMember}");
                }

                msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }


        [Command("UpdateClan")]
        [Description("Updated die Clanrole in Discord & in der Datenbank")]
        public async Task UpdateClan(CommandContext ctx,
                            [Description("Discord Role ID des Clans")] ulong clanId,
                            [Description("Neuer Clanname")] string clanName,
                            [Description("Neue Clanfarbe in Hex zB. #FFFFFF")] string clanColor)
        {

            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                //Verify action by the user
                var msgEmbed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(config.StoredValues.DefaultColor)
                };
                msgEmbed.AddField("Update Clan", $"Möchtest du den Clan {ctx.Guild.GetRole(clanId).Name} wirklich löschen?");

                //Send message
                var msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

                //React to own message
                await msg.CreateReactionAsync(yes).ConfigureAwait(false);
                await msg.CreateReactionAsync(no).ConfigureAwait(false);

                //Get User reaction
                var ia = ctx.Client.GetInteractivity();
                var reactionMsg = await ia.WaitForReactionAsync(
                    x => x.Message == msg &&
                    x.User == ctx.User &&
                    (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

                if (!clanName.Equals(string.Empty) && !clanColor.Equals(string.Empty) && reactionMsg.Result.Emoji == yes)
                {
                    //Change Discord role
                    await ctx.Guild.GetRole(clanId).ModifyAsync(name: clanName, color: new DiscordColor(clanColor));

                    //Get Admin User Dataset
                    var user = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)ctx.Member.Id));

                    //Get Clan DB Dataset
                    var clan = await Task.Run(() => Functions.Functions.SelectClanById(ctx, (long)clanId));

                    //Get all Clan Members which reference the provided Clan
                    var clanMember = await Task.Run(() => Functions.Functions.SelectClanUser(ctx, clan.LID));

                    //Update Clan in DB
                    await Task.Run(() => Functions.Functions.UpdateClan(ctx, user.LID, clan.LID, clanName, clanColor));

                    //Send conformation message to User
                    var msgEmbedConfirm = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor(config.StoredValues.DefaultColor)
                    };
                    msgEmbedConfirm.AddField("Update Clan", $"Der Clan \"{clan.CLANNAME}\" (ID {clan.CLANID}) wurde geändert.\nNeuer Name: {clanName} & Color: {clanColor}.");
                    var confirmMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbedConfirm).ConfigureAwait(false);

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
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }

        [Command("DeleteClan")]
        [Description("Entfernt ein Clan aus der Datenbank & Discord. 'Löscht' auch alle Clanuser in der Datenbank.")]
        public async Task DeleteClan(CommandContext ctx,
                                    [Description("Discord Role ID des Clans")] ulong clanId)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                //Verify action by the user
                var msgEmbed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(config.StoredValues.DefaultColor)
                };
                msgEmbed.AddField("Delete Clan", $"Möchtest du den Clan {ctx.Guild.GetRole(clanId).Name} wirklich löschen?");

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
                    var user = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)ctx.Member.Id));

                    //Get Clan DB Dataset
                    var clan = await Task.Run(() => Functions.Functions.SelectClanById(ctx, (long)clanId));

                    //Get all Clan Members which reference the provided Clan
                    var clanMember = await Task.Run(() => Functions.Functions.SelectClanUser(ctx, clan.LID));

                    //Loop through each Clan Member
                    foreach (var m in clanMember)
                    {
                        //Get Clan Member User Dataset
                        var sqlmember = await Task.Run(() => Functions.Functions.SelectUser(ctx, m.USERID));

                        //Update User and remove Clan reference
                        await Task.Run(() => Functions.Functions.UpdateUser(ctx, user.LID, m.USERID, m.ADMIN, 0, 0)); //Delete oder Update, das ist hier die Frage... 🤔

                        //Get Discord Member from DB User Dataset
                        var member = await ctx.Guild.GetMemberAsync((ulong)m.USERID);

                        //Remove Clansortierungsrolle from Discorduser
                        await member.RevokeRoleAsync(ctx.Guild.GetRole(config.StoredValues.ClanSortRoleId)).ConfigureAwait(false);
                    }

                    //Delete CLan Role from Discord Server
                    await ctx.Guild.GetRole(clanId).DeleteAsync().ConfigureAwait(false);

                    //Send conformation message to User
                    var msgEmbedConfirm = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor(config.StoredValues.DefaultColor)
                    };
                    msgEmbedConfirm.AddField("Delete Clan", $"Der Clan {clan.CLANNAME} (ID {clan.CLANID}) wurde auf dem Server gelöscht.\nWeiterhin wurden bei {clanMember.Count} Usern die Clan & Clansortierungsrolle entfernt.");
                    var confirmMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbedConfirm).ConfigureAwait(false);

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
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }

        }

        [Command("GetClanId")]
        [Description("Gibt die DiscordID eines Clans/einer Rolle aus.\n(Durchsucht den Discord nach einer Rolle mit dem Namen)")]
        public async Task GetClanId(CommandContext ctx,
                                    [Description("Clanname, !CaSe SeNsItIvE!")] string clanName)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
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
                            Color = new DiscordColor(config.StoredValues.DefaultColor)
                        };
                        msgEmbed.AddField("Get Clan ID", $"Clan {role.Value.Name} hat folgende ID: {role.Value.Id}).");
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
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
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

            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                //Initialize local variables
                var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);
                DiscordMessage joinMsg = null;
                DiscordMessage msg = null;

                //Lookup clan in DD
                var sqlClan = await Task.Run(() => Functions.Functions.SelectClanByName(ctx, clanName));

                if (sqlClan.LID == 0)
                {
                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.DarkRed
                    };
                    msgEmbed.AddField("Add Clan User",
                        "Der Clan konnte nicht in der Datenbank gefunden werden, Clanname muss genau angegeben werden (Case sensitive)!");
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                }
                else
                {
                    //Verify action by the user
                    var msgEmbedConfirm = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor(sqlClan.CLANCOLOR)
                    };
                    msgEmbedConfirm.AddField("Add Clan User", $"Möchtest du den User {member.Username} ({member.Mention}) dem Clan {sqlClan.CLANNAME} mit der Rolle {role} hinzufügen ?");

                    //Send message to User
                    joinMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbedConfirm).ConfigureAwait(false);

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
                        //Get DB User Dataset
                        var sqlUser = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)userId));

                        //Get DB Role Dataset
                        var sqlRole = await Task.Run(() => Functions.Functions.SelectRoleByName(ctx, role));

                        //Get Admin DB User Dataset
                        var user = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)ctx.Member.Id));

                        if (sqlRole.LID < 1 || user.LID < 1)
                        {
                            //Build errormessage
                            var msgEmbed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.DarkRed
                            };
                            msgEmbed.AddField("Add Clan User", "Entweder du oder die Rolle wurden noch nicht in der Datenbank angelegt worden, sorry... (╯︵╰,)");

                            //Send errormessage
                            msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                        }
                        else
                        {
                            //Compare User ID to DB
                            if (sqlUser.LID < 1)
                            {
                                //If User is new to DB
                                await Task.Run(() => Functions.Functions.AddUser(ctx, user.LID, (long)userId, admin, sqlClan.LID, sqlRole.LID));
                            }
                            else
                            {
                                //If User is already in DB
                                await Task.Run(() => Functions.Functions.UpdateUser(ctx, user.LID, sqlUser.USERID, sqlUser.ADMIN, sqlClan.LID, sqlRole.LID));
                            }

                            //Get the Discord Role ID
                            var discordRole = ctx.Guild.GetRole((ulong)sqlRole.ROLEID);

                            //If Leader give Leader Role
                            if (sqlRole.ROLENAME.Equals("Leader"))
                            {
                                //Testserver
                                //await member.GrantRoleAsync(ctx.Guild.GetRole(801757058302083092)).ConfigureAwait(false);

                                await member.GrantRoleAsync(ctx.Guild.GetRole((ulong)sqlRole.ROLEID)).ConfigureAwait(false);
                            }

                            //Get general Clan Roles
                            var clanRole = ctx.Guild.GetRole((ulong)sqlClan.CLANID);
                            var clanSortRole = ctx.Guild.GetRole(config.StoredValues.ClanSortRoleId);

                            //Grant Clan Roles
                            await member.GrantRoleAsync(clanRole).ConfigureAwait(false);
                            await member.GrantRoleAsync(clanSortRole).ConfigureAwait(false);

                            //Send conformationmessage
                            var msgEmbed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor(sqlClan.CLANCOLOR)
                            };
                            msgEmbed.AddField("Add Clan User", $"Der User {member.DisplayName} ({member.Mention}) wurde dem Clan {sqlClan.CLANNAME} mit der Rolle {role} hinzugefügt.");

                            msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                        }
                    }
                }

                //React to  own message
                await msg.CreateReactionAsync(okay).ConfigureAwait(false);

                //Get user reaction
                var ia2 = ctx.Client.GetInteractivity();
                var result = await ia2.WaitForReactionAsync(
                    x => x.Message == msg &&
                    x.User == ctx.User &&
                    x.Emoji == okay).ConfigureAwait(false);

                //Check User reaction
                if (result.Result.Emoji == okay)
                {
                    //Delete own message and the one who triggered the execution
                    await joinMsg.DeleteAsync().ConfigureAwait(false);
                    await msg.DeleteAsync().ConfigureAwait(false);
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                }
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }

        [Command("AddLeader")]
        [Description("Gibt einem User die Clanleaderrolle & aktualiert den Benutzer in der Datenbank.")]
        public async Task AddLeader(CommandContext ctx,
                                    [Description("Discord User ID")] ulong userId)
        {

            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                var sqlRole = await Task.Run(() => Functions.Functions.SelectRoleByName(ctx, "Leader"));
                var sqlUser = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)ctx.Member.Id));
                var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);

                var msgEmbedConfirm = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(config.StoredValues.DefaultColor)
                };
                msgEmbedConfirm.AddField("Add Clan Leader", $"Möchtest du dem User {member.Username} ({member.Nickname}) die Clanleaderrolle geben?");
                var joinMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbedConfirm).ConfigureAwait(false);

                await joinMsg.CreateReactionAsync(yes).ConfigureAwait(false);
                await joinMsg.CreateReactionAsync(no).ConfigureAwait(false);

                var ia = ctx.Client.GetInteractivity();

                var reaction = await ia.WaitForReactionAsync(
                    x => x.Message == joinMsg &&
                         x.User == ctx.User &&
                         (x.Emoji == yes || x.Emoji == no)).ConfigureAwait(false);

                if (reaction.Result.Emoji == yes)
                {
                    await Task.Run(() => Functions.Functions.UpdateUser(ctx, sqlUser.LID, (long)userId, sqlUser.ADMIN,
                        sqlUser.REF_CLANID ?? 0, sqlRole.LID));

                    //Testserver
                    //await member.GrantRoleAsync(ctx.Guild.GetRole(801757058302083092)).ConfigureAwait(false);

                    var roleLeader = ctx.Guild.GetRole((ulong)sqlRole.ROLEID);
                    await member.GrantRoleAsync(roleLeader).ConfigureAwait(false);

                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor(config.StoredValues.DefaultColor)
                    };
                    msgEmbed.AddField("Add Clan Leader", $"Der User {member.Username} ({member.Nickname}) hat die Clanleaderrolle bekommen");

                    var msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);

                    //React to  own message
                    await msg.CreateReactionAsync(okay).ConfigureAwait(false);

                    //Get user reaction
                    var ia2 = ctx.Client.GetInteractivity();
                    var result = await ia2.WaitForReactionAsync(
                        x => x.Message == msg &&
                             x.User == ctx.User &&
                             x.Emoji == okay).ConfigureAwait(false);
                    //Check User reaction
                    if (result.Result.Emoji == okay)
                    {
                        //Delete own message and the one who triggered the execution
                        await joinMsg.DeleteAsync().ConfigureAwait(false);
                        await msg.DeleteAsync().ConfigureAwait(false);
                        await ctx.Message.DeleteAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    await joinMsg.DeleteAsync().ConfigureAwait(false);
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                }
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }

        [Command("RemoveClanUser")]
        public async Task RemoveClanUser(CommandContext ctx,
                                        [Description("Discord User ID")] ulong userId,
                                        [Description("[Optional] bool delete User (Default = false)")] bool delete = false)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageChannels))
            {
                DiscordMessage msg;
                var member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);
                var sqlUser = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)userId));
                var sqlClan = await Task.Run(() => Functions.Functions.SelectClanByDbId(ctx, (long)sqlUser.REF_CLANID)); //Kontrolle ob Refernz überhaupt belegt

                if(sqlUser.LID < 1 || sqlClan.LID < 1)
                {
                    //Build errormessage
                    var msgEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.DarkRed
                    };
                    msgEmbed.AddField("Remove Clan User", "Der von dir angegebene Clan oder User exestieren nicht in der DB, sorry... (╯︵╰,)");

                    //Send errormessage
                    msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                }
                else
                {
                    //Build errormessage
                    var msgEmbedConfirm = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor(config.StoredValues.DefaultColor)
                    };
                    msgEmbedConfirm.AddField("Remove Clan User", $"Möchtest du den User {member.DisplayName} ({member.Mention}) aus dem Clan {sqlClan.CLANNAME} entfernen?");

                    var joinMsg = await ctx.Channel.SendMessageAsync(embed: msgEmbedConfirm).ConfigureAwait(false);

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
                        var sqlAdminUser = await Task.Run(() => Functions.Functions.SelectUser(ctx, (long)ctx.Member.Id));

                        if (sqlAdminUser.LID < 1)
                        {
                            //Build errormessage
                            var msgEmbed = new DiscordEmbedBuilder
                            {
                                Color = DiscordColor.DarkRed
                            };
                            msgEmbed.AddField("Remove Clan User", "Du wurdest noch nicht in der Datenbank angelegt, sorry... (╯︵╰,)");

                            //Send errormessage
                            msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                        }
                        else
                        {
                            //Update Clan reference
                            if (delete)
                            {
                                await Task.Run(() => Functions.Functions.DeleteUser(ctx, sqlUser.LID));
                            }
                            else
                            {
                                await Task.Run(() => Functions.Functions.UpdateUser(ctx, sqlAdminUser.USERID, sqlUser.LID, sqlAdminUser.ADMIN, 0, 0));
                            }

                            //Remove roles
                            await member.RevokeRoleAsync(ctx.Guild.GetRole((ulong)sqlClan.CLANID)).ConfigureAwait(false);
                            await member.RevokeRoleAsync(ctx.Guild.GetRole(config.StoredValues.ClanSortRoleId)).ConfigureAwait(false);

                            //Send conformationmessage
                            var msgEmbed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor(sqlClan.CLANCOLOR)
                            };
                            msgEmbed.AddField("Remove Clan User", $"Der User {member.DisplayName} ({member.Mention}) wurde aus dem Clan {sqlClan.CLANNAME} entfernt.");

                            msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed).ConfigureAwait(false);
                        }

                        //React to  own message
                        await msg.CreateReactionAsync(okay).ConfigureAwait(false);

                        //Get user reaction
                        var ia2 = ctx.Client.GetInteractivity();
                        var result = await ia2.WaitForReactionAsync(
                            x => x.Message == msg &&
                                 x.User == ctx.User &&
                                 x.Emoji == okay).ConfigureAwait(false);
                        //Check User reaction
                        if (result.Result.Emoji == okay)
                        {
                            //Delete own message and the one who triggered the execution
                            await joinMsg.DeleteAsync().ConfigureAwait(false);
                            await msg.DeleteAsync().ConfigureAwait(false);
                            await ctx.Message.DeleteAsync().ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await joinMsg.DeleteAsync().ConfigureAwait(false);
                        await ctx.Message.DeleteAsync().ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await Task.Run(() => Functions.Functions.SendUnauthorizedMessage(ctx, okay));
            }
        }
#endregion
    }
}
