using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiscordBot.JsonClasses;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
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
