using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiscordBot.JsonClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace DiscordBot.Functions
{

    public static class Functions
    {
        private static readonly ConfigJson config = ReadConfig();
        private static readonly string dblocation = $@"{Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\Milfbase.db";

        public static ConfigJson ReadConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead($@"{Environment.CurrentDirectory}\config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }

        public static FishJson ReadFishConfig()
        {
            var json = string.Empty;
            using (var fs = File.OpenRead($@"{Environment.CurrentDirectory}\fishconfig.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<FishJson>(json);
        }

        public static string GetHexFromColor(string name)
        {
            var result = String.Empty;
            switch (name)
            {
                case "Black":
                case "Schwarz":
                    result = "#010101";
                break;

                case "White":
                case "Weiß":
                    result = "#FFFFFF";
                    break;

                case "Grey":
                case "Grau":
                    result = "#808080";
                    break;

                case "Blurple":
                case "Discord":
                    result = "#010101";
                    break;

                case "Red":
                case "Rot":
                    result = "#FF0000";
                    break;

                case "Green":
                case "Grün":
                    result = "#00FF00";
                    break;

                case "Blue":
                case "Blau":
                    result = "#0000FF";
                    break;

                case "Yellow":
                case "Gelb":
                    result = "#FFFF00";
                    break;

                case "Cyan":
                case "Türkis":
                    result = "#00FFFF";
                    break;

                case "Magenta":
                    result = "#FF00FF";
                    break;

                case "Gold":
                    result = "#FFD700";
                    break;

                default:
                    result = "#9c59b6";
                    break;
            }
            return result;
        }

        public static Tuple<bool, string> checkTwitterText(string searchtext)
        {
            switch (searchtext.Trim().ToLower())
            {
                case string a when a.Contains("giveaway"): return new Tuple<bool, string>(true, "giveaway");
                case string b when b.Contains("twitchdrop"): return new Tuple<bool, string>(true, "drop");
                case string c when c.Contains("releasenote"): return new Tuple<bool, string>(true, "release");
                case string d when d.Contains("maintenance"): return new Tuple<bool, string>(true, "maintenance");
            }

            return new Tuple<bool, string>(false, string.Empty);
        }

        public async static void SendErrorMessage(DiscordChannel channel, string error)
        {
            var msgEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.DarkRed
            };
            msgEmbed.AddField("Database Error", error);
            await channel.SendMessageAsync(embed: msgEmbed);
        }

        public static string GetCategoryText(CommandContext ctx, string categoryName)
        {
            try
            {
                string result = string.Empty;
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT text FROM BotText WHERE categoryName = $categoryName";
                    command.Parameters.AddWithValue("$categoryName", categoryName);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result = r["text"].ToString();
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx.Channel, e.Message);
                return string.Empty;
            }
        }

        public static void EditCategoryText(CommandContext ctx, string text, string categoryName)
        {
            try
            {
                string result = string.Empty;
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"UPDATE BotText SET text = $text WHERE categoryName = $categoryName";
                    command.Parameters.AddWithValue("$text", text);
                    command.Parameters.AddWithValue("$categoryName", categoryName);
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx.Channel, e.Message);
            }
        }

        public static string AddCategoryText(CommandContext ctx, string text, string categoryName)
        {
            try
            {
                string result = string.Empty;
                using (var connection = new SqliteConnection($"Data Source={dblocation}"))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"INSERT INTO BotText (text, categoryName) VALUES ($text, $categoryName)";
                    command.Parameters.AddWithValue("$text", text);
                    command.Parameters.AddWithValue("$categoryName", categoryName);

                    using var r = command.ExecuteReader();
                    while (r.HasRows && r.Read())
                    {
                        result = r["text"].ToString();
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception e)
            {
                SendErrorMessage(ctx.Channel, e.Message);
                return string.Empty;
            }
        }

        public static string DiscordFormat(CommandContext ctx, string text)
        {
            text = text.Replace("<b>", "**");
            text = text.Replace("<i>", "_");
            text = text.Replace("<u>", "__");
            text = text.Replace("<s>", "~");
            return text;
        }

        public static void PostTwitterGiveaway()
        {
            var giveawayChannel = config.TwitterValues.GiveawayChannelId;


            return;
        }

        public static async void GrantRolesByReaction(DiscordClient client, MessageReactionAddEventArgs eventArgs)
        {
            var member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id).ConfigureAwait(false);
            var reaction = eventArgs.Emoji;

            #region comments
            //if (eventArgs.Message.Id == 943945113963470879)
            //{
            //    var reaction = eventArgs.Emoji;

            //    switch (reaction)
            //    {
            //        case "👸":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(943944801605275749)).ConfigureAwait(false);
            //            break;

            //        case "👑":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(943944985210912828)).ConfigureAwait(false);
            //            break;

            //        case "🤴":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(943944860631711834)).ConfigureAwait(false); 
            //            break;

            //    }
            //}

            //if (eventArgs.Message.Id == 943945120473026590)
            //{
            //    var reaction = eventArgs.Emoji;

            //    switch (reaction)
            //    {
            //        case "👶":
            //            //await member.GrantRoleAsync(eventArgs.Guild.GetRole(943944801605275749)).ConfigureAwait(false);
            //            break;

            //        case "🍺":
            //            //await member.GrantRoleAsync(eventArgs.Guild.GetRole(943944985210912828)).ConfigureAwait(false);
            //            break;

            //        case "🥃":
            //            //await member.GrantRoleAsync(eventArgs.Guild.GetRole(943944860631711834)).ConfigureAwait(false);
            //            break;

            //        case "👵":
            //            //await member.GrantRoleAsync(eventArgs.Guild.GetRole(943944860631711834)).ConfigureAwait(false);
            //            break;
            //    }
            //}
            #endregion

            //#region Annies Server
            //if (eventArgs.Message.Id == 944161424761045063)
            //{
            //    switch (reaction)
            //    {
            //        case "🔫":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(939190874032726029)).ConfigureAwait(false);
            //            break;

            //        case "🏹":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(939187741755842663)).ConfigureAwait(false);
            //            break;

            //        case "🔮":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(939190765446393856)).ConfigureAwait(false);
            //            break;

            //        case "👻":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(939190944807407696)).ConfigureAwait(false);
            //            break;
            //    }
            //}
            //#endregion

            #region Plebhunter
            //if (eventArgs.Message.Channel.Id == 944225414342139914 && !member.IsBot && !member.IsOwner)
            //{
            //    switch (reaction)
            //    {
            //        case "🦟":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(888855861768880209)).ConfigureAwait(false);
            //            break;

            //        case "🍃":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(888855689982779393)).ConfigureAwait(false);
            //            break;

            //        case "🤡":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(888854826807623721)).ConfigureAwait(false);
            //            break;

            //        case "💰":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(888855558130659328)).ConfigureAwait(false);
            //            break;

            //        case "🐺":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(897944315395993600)).ConfigureAwait(false);
            //            break;

            //        case "🕵🏼":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(904801506593759273)).ConfigureAwait(false);
            //            break;

            //        case "🔫":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(904377583452442665)).ConfigureAwait(false);
            //            break;

            //        case "🔮":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(937026758161616978)).ConfigureAwait(false);
            //            break;

            //        case "🔞":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(921923090533982209)).ConfigureAwait(false);
            //            break;

            //        case "🍆":
            //            await member.GrantRoleAsync(eventArgs.Guild.GetRole(945044640418115626)).ConfigureAwait(false);
            //            break;
            //    }
            //}
            #endregion

        }

        public static async void RemoveRolesByReaction(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
        {
            var member = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id).ConfigureAwait(false);
            var reaction = eventArgs.Emoji;

            //#region Annies Server
            //if (eventArgs.Message.Id == 944161424761045063)
            //{
            //    switch (reaction)
            //    {
            //        case "🔫":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(939190874032726029)).ConfigureAwait(false);
            //            break;

            //        case "🏹":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(939187741755842663)).ConfigureAwait(false);
            //            break;

            //        case "🔮":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(939190765446393856)).ConfigureAwait(false);
            //            break;

            //        case "👻":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(939190944807407696)).ConfigureAwait(false);
            //            break;
            //    }
            //}
            //#endregion

            //#region Plebhunter
            //if (eventArgs.Message.Channel.Id == 944225414342139914)
            //{
            //    switch (reaction)
            //    {
            //        case "🦟":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(888855861768880209)).ConfigureAwait(false);
            //            break;

            //        case "🍃":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(888855689982779393)).ConfigureAwait(false);
            //            break;

            //        case "🤡":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(888854826807623721)).ConfigureAwait(false);
            //            break;

            //        case "💰":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(888855558130659328)).ConfigureAwait(false);
            //            break;

            //        case "🐺":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(897944315395993600)).ConfigureAwait(false);
            //            break;

            //        case "🕵🏼":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(904801506593759273)).ConfigureAwait(false);
            //            break;

            //        case "🔫":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(904377583452442665)).ConfigureAwait(false);
            //            break;

            //        case "🔮":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(937026758161616978)).ConfigureAwait(false);
            //            break;

            //        case "🔞":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(921923090533982209)).ConfigureAwait(false);
            //            break;

            //        case "🍆":
            //            await member.RevokeRoleAsync(eventArgs.Guild.GetRole(945044640418115626)).ConfigureAwait(false);
            //            break;
            //    }
            //}
            //#endregion

        }

        public static bool OnlyPlebhunter(CommandContext ctx)
        {
            return ctx.Guild.Id == 327105561298599946;
        }

        public static async void Buttonpressed(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            




        }

    }

    public class Lottery<T>
    {
        public class Ticket
        {
            public T Key { get; private set; }
            public double Weight { get; private set; }
            public Ticket(T key, double weight)
            {
                this.Key = key;
                this.Weight = weight;
            }
        }
        List<Ticket> tickets = new List<Ticket>();
        static Random rand = new Random();
        public void Add(T key, double weight)
        {
            tickets.Add(new Ticket(key, weight));
        }
        public Ticket Draw(bool removeWinner)
        {
            double r = rand.NextDouble() * tickets.Sum(a => a.Weight);
            double min = 0;
            double max = 0;
            Ticket winner = null;
            foreach (var ticket in tickets)
            {
                max += ticket.Weight;
                //-----------
                if (min <= r && r < max)
                {
                    winner = ticket;
                    break;
                }
                //-----------
                min = max;
            }
            if (winner == null) throw new Exception();
            if (removeWinner) tickets.Remove(winner);
            return winner;
        }
    }
}
