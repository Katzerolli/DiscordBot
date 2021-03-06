﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DiscordBotTest.Functions
{
    public static class Functions
    {
        private static readonly string dblocation = $@"{Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\Database\ClanDatabase.db";

        public class clanResult
        {
            public long LID { get; set; }
            public DateTime DTINSERT { get; set; }
            public long LUSERIDINSERT { get; set; }
            public DateTime? DTEDIT { get; set; }
            public long? LUSERID { get; set; }
            public long CLANID { get; set; }
            public string CLANNAME { get; set; }
            public string? CLANCOLOR { get; set; }
        }

        public class userResult
        {
            public long LID { get; set; }
            public DateTime DTINSERT { get; set; }
            public long LUSERIDINSERT { get; set; }
            public DateTime? DTEDIT { get; set; }
            public long? LUSERID { get; set; }
            public long USERID { get; set; }
            public bool ADMIN { get; set; }
            public long? REF_CLANID { get; set; }
            public long? REF_CLANROLE { get; set; }
        }

        public class roleResult
        {
            public long LID { get; set; }
            public DateTime DTINSERT { get; set; }
            public long LUSERIDINSERT { get; set; }
            public DateTime? DTEDIT { get; set; }
            public long? LUSERID { get; set; }
            public long ROLEID { get; set; }
            public string ROLENAME { get; set; }
        }

        public static ConfigJson ReadConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead($@"{Environment.CurrentDirectory}\config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }

        public async static void SendUnauthorizedMessage(CommandContext ctx, DiscordEmoji emoji)
        {
            var msgEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.DarkRed
            };
            msgEmbed.AddField("Unauthorized", "Arrrr. Seefahrer lass die Finger davon, sonst werfe ich dich den Haien vor! Arrrrr!");

            var msg = await ctx.Channel.SendMessageAsync(embed: msgEmbed);
            await msg.CreateReactionAsync(emoji).ConfigureAwait(false);

            var ia = ctx.Client.GetInteractivity();

            var result = await ia.WaitForReactionAsync(
                x => x.Message == msg &&
                     x.User == ctx.User &&
                     x.Emoji == emoji).ConfigureAwait(false);

            if (result.Result.Emoji == emoji)
            {
                await msg.DeleteAsync().ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }

        public async static void SendErrorMessage(CommandContext ctx, string error)
        {
            var msgEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.DarkRed
            };
            msgEmbed.AddField("Database Error", error);
            await ctx.Channel.SendMessageAsync(embed: msgEmbed);
        }

        public static async void CreateClanArea(CommandContext ctx, string name, DiscordRole role)
        {
            //Get Moderator role
            var config = ReadConfig();
            var modRole = ctx.Guild.GetRole(config.StoredValues.ModRoleId);

            //Create category & channels
            var category = await ctx.Guild.CreateChannelCategoryAsync(name);
            var info = await ctx.Guild.CreateChannelAsync($"{name}-info", ChannelType.Text, category);
            var text1 = await ctx.Guild.CreateChannelAsync($"{name}-chat", ChannelType.Text, category);
            var text2 = await ctx.Guild.CreateChannelAsync($"{name}-chat-wichtig", ChannelType.Text, category);
            var voice1 = await ctx.Guild.CreateChannelAsync($"{name} 1", ChannelType.Voice, category);
            var voice2 = await ctx.Guild.CreateChannelAsync($"{name} 2", ChannelType.Voice, category);
            var voice3 = await ctx.Guild.CreateChannelAsync($"{name} 3", ChannelType.Voice, category);

            //Restrict text channels
            await text1.AddOverwriteAsync(role, Permissions.AccessChannels);
            await text1.AddOverwriteAsync(modRole, Permissions.AccessChannels);
            await text1.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.AccessChannels);
            await text2.AddOverwriteAsync(role, Permissions.AccessChannels);
            await text2.AddOverwriteAsync(modRole, Permissions.AccessChannels);
            await text2.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.AccessChannels);

            //Restrict voice channels
            await voice1.AddOverwriteAsync(role, Permissions.AccessChannels);
            await voice1.AddOverwriteAsync(modRole, Permissions.AccessChannels);
            await voice1.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.AccessChannels);
            await voice2.AddOverwriteAsync(role, Permissions.AccessChannels);
            await voice2.AddOverwriteAsync(modRole, Permissions.AccessChannels);
            await voice2.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.AccessChannels);
            await voice3.AddOverwriteAsync(role, Permissions.AccessChannels);
            await voice3.AddOverwriteAsync(modRole, Permissions.AccessChannels);
            await voice3.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.AccessChannels);
        }

        #region SQL User Commands

        public static void AddUser(CommandContext ctx, long userIdInsert, long userId, bool admin, long clanLid = 0, long roleLid = 0)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO DSUSER (DTINSERT, LUSERIDINSERT, USERID, ADMIN, REF_CLANID, REF_ROLE) VALUES ($DTINSERT, $LUSERIDINSERT, $USERID, $ADMIN, $REF_CLANID, $REF_ROLE);";
                    command.Parameters.AddWithValue("$DTINSERT", DateTime.Now);
                    command.Parameters.AddWithValue("$LUSERIDINSERT", userIdInsert);
                    command.Parameters.AddWithValue("$USERID", userId);
                    command.Parameters.AddWithValue("$ADMIN", admin);
                    if (clanLid == 0)
                    {
                        command.Parameters.AddWithValue("$REF_CLANID", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("$REF_CLANID", clanLid);
                    }
                    if (roleLid == 0)
                    {
                        command.Parameters.AddWithValue("$REF_ROLE", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("$REF_ROLE", roleLid);
                    }
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }

        public static void UpdateUser(CommandContext ctx, long lUserId, long userId, bool admin, long clanLid, long roleLid)
        {
            try 
            { 
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"UPDATE DSUSER SET DTEDIT = $DTEDIT, LUSERID = $LUSERID, ADMIN = $ADMIN, REF_CLANID = $REF_CLANID, REF_ROLE = $REF_ROLE WHERE USERID = $USERID;";
                    command.Parameters.AddWithValue("$DTEDIT", DateTime.Now);
                    command.Parameters.AddWithValue("$LUSERID", lUserId);
                    command.Parameters.AddWithValue("$ADMIN", admin);
                    if (clanLid == 0)
                    {
                        command.Parameters.AddWithValue("$REF_CLANID", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("$REF_CLANID", clanLid);
                    }
                    if (roleLid == 0)
                    {
                        command.Parameters.AddWithValue("$REF_ROLE", DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("$REF_ROLE", roleLid);
                    }
                    command.Parameters.AddWithValue("$USERID", userId);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }

        }

        public static userResult SelectUser(CommandContext ctx, long userId)
        {
            try
            {
                userResult result = new userResult();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSUSER WHERE USERID = $USERID";
                    command.Parameters.AddWithValue("$USERID", userId);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result.LID = (long) r["LID"];
                        result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        result.LUSERIDINSERT = (long) r["LUSERIDINSERT"];
                        result.DTEDIT = r["DTEDIT"] == DBNull.Value
                            ? (DateTime?) null
                            : Convert.ToDateTime(r["DTEDIT"]);
                        result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?) null : (long) r["LUSERID"];
                        result.USERID = (long) r["USERID"];
                        result.ADMIN = Convert.ToBoolean(r["ADMIN"]);
                        result.REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?) null : (long) r["REF_CLANID"];
                        result.REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?) null : (long) r["REF_ROLE"];
                    }

                    connection.Close();
                }

                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new userResult();
            }
        }

        public static List<userResult> SelectClanMember(CommandContext ctx, long clanId, List<long> roleId)
        {
            try
            {
                var result = new List<userResult>();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var roles = string.Join(", ", roleId);
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSUSER WHERE REF_CLAN = $CLANID AND REF_ROLE = $ROLEID";
                    command.Parameters.AddWithValue("$CLANID", clanId);
                    command.Parameters.AddWithValue("$ROLEID", roles);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        var tmp = new userResult()
                        {
                            LID = (long) r["LID"],
                            DTINSERT = Convert.ToDateTime(r["DTINSERT"]),
                            LUSERIDINSERT = (long) r["LUSERIDINSERT"],
                            DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?) null : Convert.ToDateTime(r["DTEDIT"]),
                            LUSERID = r["LUSERID"] == DBNull.Value ? (long?) null : (long) r["LUSERID"],
                            USERID = (long) r["USERID"],
                            ADMIN = Convert.ToBoolean(r["ADMIN"]),
                            REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?) null : (long) r["REF_CLANID"],
                            REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?) null : (long) r["REF_ROLE"]
                        };
                        result.Add(tmp);
                    }

                    connection.Close();
                }

                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new List<userResult>();
            }

        }

        public static userResult SelectAllUser(CommandContext ctx)
        {
            try
            {
                userResult result = new userResult();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSUSER";

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result.LID = (long)r["LID"];
                        result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        result.LUSERIDINSERT = (long)r["LUSERIDINSERT"];
                        result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                        result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : (long)r["LUSERID"];
                        result.USERID = (long)r["USERID"];
                        result.ADMIN = Convert.ToBoolean(r["ADMIN"]);
                        result.REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?)null : (long)r["REF_CLANID"];
                        result.REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?)null : (long)r["REF_ROLE"];
                    }
                    connection.Close();
                }
                return result;

            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new userResult();
            }
        }

        public static List<userResult> SelectClanUser(CommandContext ctx, [Description("Die Datenbank LID des Clans")]long clanId)
        {
            try
            {
                List<userResult> result = new List<userResult>();

                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSUSER WHERE REF_CLANID = $CLANID";
                    command.Parameters.AddWithValue("$CLANID", clanId);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        var tmp = new userResult()
                        {
                            LID = Convert.ToInt64(r["LID"]),
                            DTINSERT = Convert.ToDateTime(r["DTINSERT"]),
                            LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]),
                            DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]),
                            LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]),
                            USERID = Convert.ToInt64(r["USERID"]),
                            ADMIN = Convert.ToBoolean(r["ADMIN"]),
                            REF_CLANID = r["REF_CLANID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["REF_CLANID"]),
                            REF_CLANROLE = r["REF_ROLE"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["REF_ROLE"])
                        };
                        result.Add(tmp);
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new List<userResult>();
            }
        }

        public static List<Tuple<long, string>> CountClanMember(CommandContext ctx, long clanId, List<long> roleId)
        {
            try
            {
                var result = new List<Tuple<long, string>>();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    foreach (var role in roleId)
                    {
                        var tmp = new Tuple<long, string>(0, string.Empty);

                        var command = connection.CreateCommand();
                        command.CommandText = @"SELECT COUNT(*) AS Anzahl FROM DSUSER WHERE REF_CLANID = $CLANID AND REF_ROLE = $ROLEID";
                        command.Parameters.AddWithValue("$CLANID", clanId);
                        command.Parameters.AddWithValue("$ROLEID", role);

                        using var r = command.ExecuteReader();
                        while (r.HasRows && r.Read())
                        {
                            tmp = new Tuple<long, string>(role, (r["Anzahl"]).ToString());
                        }
                        result.Add(tmp);
                    }

                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new List<Tuple<long, string>>();
            }

        }

        public static void DeleteUser(CommandContext ctx, long userId)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"DELETE FROM DSUSER WHERE USERID = $USERID";
                    command.Parameters.AddWithValue("$USERID", userId);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }

        #endregion

        public static void AddClan(CommandContext ctx, long userIdInsert, long clanId, string clanName, string clanColor)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"INSERT INTO DSCLAN (DTINSERT, LUSERIDINSERT, CLANID, CLANNAME, CLANCOLOR) VALUES ($DTINSERT, $LUSERIDINSERT, $CLANID, $CLANNAME, $CLANCOLOR);";
                    command.Parameters.AddWithValue("$DTINSERT", DateTime.Now);
                    command.Parameters.AddWithValue("$LUSERIDINSERT", userIdInsert);
                    command.Parameters.AddWithValue("$CLANID", clanId);
                    command.Parameters.AddWithValue("$CLANNAME", clanName);
                    command.Parameters.AddWithValue("$CLANCOLOR", clanColor);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }

        public static void UpdateClan(CommandContext ctx, long userIdInsert, long clanId, string clanName, string clanColor)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"UPDATE DSCLAN SET DTEDIT = $DTEDIT, LUSERID = $LUSERID, CLANNAME = $CLANNAME, CLANCOLOR = $CLANCOLOR WHERE CLANID = $CLANID);";
                    command.Parameters.AddWithValue("$DTEDIT", DateTime.Now);
                    command.Parameters.AddWithValue("$LUSERID", userIdInsert);
                    command.Parameters.AddWithValue("$CLANID", clanId);
                    command.Parameters.AddWithValue("$CLANNAME", clanName);
                    command.Parameters.AddWithValue("$CLANCOLOR", clanColor);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }

        public static clanResult SelectClanByDbId(CommandContext ctx, long clanLid)
        {
            try
            {
                clanResult result = new clanResult();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSCLAN WHERE LID = $CLANLID";
                    command.Parameters.AddWithValue("$CLANLID", clanLid);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result.LID = Convert.ToInt64(r["LID"]);
                        result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                        result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                        result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                        result.CLANID = Convert.ToInt64(r["CLANID"]);
                        result.CLANNAME = r["CLANNAME"].ToString();
                        result.CLANCOLOR = r["CLANCOLOR"] == DBNull.Value ? string.Empty : r["CLANCOLOR"].ToString();
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new clanResult();
            }
        }

        public static clanResult SelectClanById(CommandContext ctx, long clanId)
        {
            try
            {
                clanResult result = new clanResult();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSCLAN WHERE CLANID = $CLANID";
                    command.Parameters.AddWithValue("$CLANID", clanId);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result.LID = Convert.ToInt64(r["LID"]);
                        result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                        result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                        result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                        result.CLANID = Convert.ToInt64(r["CLANID"]);
                        result.CLANNAME = r["CLANNAME"].ToString();
                        result.CLANCOLOR = r["CLANCOLOR"] == DBNull.Value ? string.Empty : r["CLANCOLOR"].ToString();
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new clanResult();
            }
        }

        public static clanResult SelectClanByName(CommandContext ctx, string clanName)
        {
            try
            {
                clanResult result = new clanResult();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSCLAN WHERE CLANNAME = $CLANNAME";
                    command.Parameters.AddWithValue("$CLANNAME", clanName);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result.LID = Convert.ToInt64(r["LID"]);
                        result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                        result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                        result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                        result.CLANID = Convert.ToInt64(r["CLANID"]);
                        result.CLANNAME = r["CLANNAME"].ToString();
                        result.CLANCOLOR = r["CLANCOLOR"] == DBNull.Value ? string.Empty : r["CLANCOLOR"].ToString();
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new clanResult();
            }
        }

        public static List<clanResult> SelectAllClan(CommandContext ctx)
        {
            try
            {
                var result = new List<clanResult>();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSCLAN";

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        var tmp = new clanResult();
                        tmp.LID = Convert.ToInt64(r["LID"]);
                        tmp.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        tmp.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                        tmp.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                        tmp.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                        tmp.CLANID = Convert.ToInt64(r["CLANID"]);
                        tmp.CLANNAME = r["CLANNAME"].ToString();
                        tmp.CLANCOLOR = r["CLANCOLOR"] == DBNull.Value ? string.Empty : r["CLANCOLOR"].ToString();
                        result.Add(tmp);
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new List<clanResult>();
            }
        }

        [Description("Entfernt Eintrag aus DSCLAN. Damit ein Clan gelöscht werden kann müssen erst Benutzer entfernt werden!")]
        public static void DeleteClan(CommandContext ctx, long clanId)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"DELETE FROM DSCLAN WHERE CLANID = $CLANID);";
                    command.Parameters.AddWithValue("$CLANID", clanId);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }

        public static void AddRole(CommandContext ctx, long userIdInsert, long roleId, string roleName)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO DSROLE (DTINSERT, LUSERIDINSERT, ROLEID, ROLENAME) VALUES ($DTINSERT, $LUSERIDINSERT, $ROLEID, $ROLENAME);";
                    command.Parameters.AddWithValue("$DTINSERT", DateTime.Now);
                    command.Parameters.AddWithValue("$LUSERIDINSERT", userIdInsert);
                    command.Parameters.AddWithValue("$ROLEID", roleId);
                    command.Parameters.AddWithValue("$ROLENAME", roleName);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }

        public static void UpdateRole(CommandContext ctx, long userIdEdit, long roleId, string roleName)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"UPDATE DSROLE SET DTEDIT = $DTEDIT, LUSERID = $LUSERID, ROLENAME = $ROLENAME WHERE ROLEID = $ROLEID);";
                    command.Parameters.AddWithValue("$DTEDIT", DateTime.Now);
                    command.Parameters.AddWithValue("$LUSERID", userIdEdit);
                    command.Parameters.AddWithValue("$ROLENAME", roleName);
                    command.Parameters.AddWithValue("$ROLEID", roleId);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }

        public static roleResult SelectRoleByName(CommandContext ctx, string roleName)
        {
            try
            {
                roleResult result = new roleResult();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSROLE WHERE ROLENAME = $ROLENAME";
                    command.Parameters.AddWithValue("$ROLENAME", roleName);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result.LID = Convert.ToInt64(r["LID"]);
                        result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                        result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                        result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                        result.ROLEID = Convert.ToInt64(r["ROLEID"]);
                        result.ROLENAME = r["ROLENAME"].ToString();
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new roleResult();
            }
        }

        public static roleResult SelectAllRole(CommandContext ctx)
        {
            try
            {
                roleResult result = new roleResult();
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * FROM DSROLE";

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result.LID = Convert.ToInt64(r["LID"]);
                        result.DTINSERT = Convert.ToDateTime(r["DTINSERT"]);
                        result.LUSERIDINSERT = Convert.ToInt64(r["LUSERIDINSERT"]);
                        result.DTEDIT = r["DTEDIT"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DTEDIT"]);
                        result.LUSERID = r["LUSERID"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["LUSERID"]);
                        result.ROLEID = Convert.ToInt64(r["ROLEID"]);
                        result.ROLENAME = r["ROLENAME"].ToString();
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
                return new roleResult();
            }
        }

        public static void DeleteRole(CommandContext ctx, long roleId)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"DELETE FROM DSROLE WHERE ROLEID = $ROLEID);";
                    command.Parameters.AddWithValue("$ROLEID", roleId);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx, e.Message);
            }
        }


    }
}
