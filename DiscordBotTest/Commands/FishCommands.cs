using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DiscordBot.FishClasses;
using DiscordBot.Functions;
using System.Numerics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data.SqlTypes;
using System.Net.Http.Headers;

namespace DiscordBot.Commands
{

    public class FishCommands : BaseCommandModule
    {

        const string connStr = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\ndeboni\\Documents\\GitHub\\DiscordBot\\DiscordBotTest\\Database\\Database.mdf;Integrated Security=True";

        [Command("Info")]
        [Hidden]
        public async Task Info(CommandContext ctx)
        {

            Player player = checkPlayer((long)ctx.Member.Id);
            if (player.id > 0)
            {
                var embeded = new DiscordEmbedBuilder()
                {
                    Title = "Game info",
                    Description = $"This whole thing currently is still in development.\nCurrently available features are:\nHurensohn :)"
                };

                await ctx.Channel.SendMessageAsync(embeded).ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Please register first.").ConfigureAwait(false);
            }
        }

        [Command("Fish")]
        [Hidden]
        public async Task Fish(CommandContext ctx)
        {
            //Ablauf was alles passieren muss:
            //-zukünftig evtl Minispiel
            //-Player muss eine Standardzeit - Bonuszeit von Ausrüstung warten
            //✔️Fish wird generiert & Bild von dem Fish wird in den Channel gepostet
            //✔️Fish wird dem Player im Inventar hinzugefügt
            //✔️Wenn man mit Ködern Angelt werden diese auch abgezogen


            //Was muss alles gegeben sein damit man fischen kann:
            //-Es muss aktiv eine fishing rod ausgerüstet sein
            //✔️Man muss an einer Insel stehen
            //-Man darf nicht schon überladen sein
            //->Eventuell berechnung von Max. Gewicht was man finden kann + aktuelles Gewicht < Max gewicht???
            //
            Player player = checkPlayer((long)ctx.Member.Id);
            if (player.id == -1)
            {
                await ctx.Channel.SendMessageAsync("Please register first.").ConfigureAwait(false);
                return;
            }else if ((bool)player.locked)
            {
                await ctx.Channel.SendMessageAsync("You can't do that at this moment").ConfigureAwait(false);
                return;
            }

            //lockPlayer(player.id);

            Random rnd = new Random();
            var fishingRod = "Obsidian Fishing Pole";
            int? amount = null;

            //Ködern abziehen
            if (player.equippedBaitId > 0)
            {
                amount = insertPlayerInventory(player.id, (int)player.equippedBaitId, 2, -1);

                if (amount < 0 || !amount.HasValue)
                {
                    await ctx.Channel.SendMessageAsync("You don't have enough bait for this action.").ConfigureAwait(false);
                    return;
                }
            }

            var msg = await ctx.Channel.SendMessageAsync($"You cast your {fishingRod}.").ConfigureAwait(false);

            List<Fish> fishPool = getFishPool((int)player.currentIslandId, player.equippedBaitId);

            var lottery = new Lottery<string>();
            foreach (var fishType in fishPool)
            {
                lottery.Add(fishType.name, (double)fishType.percentage);
            }
            string caughtFishName = lottery.Draw(true).Key;
            int caughtFishId = fishPool.Where(x => x.name == caughtFishName).Select(x => x.id).First();
            Item caughtFish = selectItem(caughtFishId);
                    
            await Task.Delay(2000);

            var caughtFishAmount = insertPlayerInventory(player.id, caughtFish.id, 1, 1);
            //Tuple<int,int> level = addPlayerXp(player.id, (int)caughtFish.xp);

            var dummy = "level would be here (or not)";

            var catchMsgBuilder = new DiscordEmbedBuilder()
            {
                Title = "Success!",
                Description = $"You caught a {caughtFishName}!\nYou gained {caughtFish.xp} experience.\n{ /* (level.Item1 == 1 ? $"You leveled up! You are now level {level.Item2}\n" : string.Empty) */ dummy}" +
                            $"You alreday caught {caughtFishAmount} {caughtFish.name}{(player.equippedBaitId == 0 ? string.Empty : $"\nYou've got {amount} bait left")}",
                ImageUrl = caughtFish.imageURL
            };

            await ctx.Channel.SendMessageAsync(catchMsgBuilder).ConfigureAwait(false);

            unlockPlayer(player.id);
            return;
        }

        [Command("ListInventory")]
        [Hidden]
        public async Task ListInventory(CommandContext ctx)
        {
            string fishList = "__All your current fish:__\n";
            string baitList = "\n__All your current bait:__\n";
            string sonstige = string.Empty;

            Player player = checkPlayer((long)ctx.Member.Id);
            if (player.id == -1)
            {
                await ctx.Channel.SendMessageAsync("Please register first.").ConfigureAwait(false);
                return;
            }

            List<Item> playerInventory = selectItemsByPlayer(player.id); 

            foreach(var item in playerInventory)
            {
                switch (item.itemTypeId)
                {
                    case 1:
                        fishList += $"{DiscordEmoji.FromName(ctx.Client, item.emoji.Trim())} {item.amount}x{item.name}\n";
                    break;

                    case 2:
                        baitList += $"{DiscordEmoji.FromName(ctx.Client, item.emoji.Trim())} {item.amount}x{item.name}\n";
                    break;

                    default:

                        break;
                }
            }
            await ctx.Channel.SendMessageAsync($"The contents of your inventory:\n\n{fishList}{baitList}").ConfigureAwait(false);
            return;
        }

        [Command("ListInventoryEmbeded")]
        [Hidden]
        public async Task ListInventoryEmbeded(CommandContext ctx)
        {
            string amountList = string.Empty;
            string nameList = string.Empty;
            string worthList = string.Empty;

            Player player = checkPlayer((long)ctx.Member.Id);
            if (player.id > 0)
            {
                var text = new List<Tuple<string, string, string>>();
                var tmpA = string.Empty;
                var tmpN = string.Empty;
                var tmpW = string.Empty;
                var c = 0;
                var ch = 0;

                List<Item> playerInventory = selectItemsByPlayer(player.id);

                foreach (var item in playerInventory)
                {
                    if (c < 10 && ch < 950)
                    {
                        tmpA += $"{item.amount}\n";
                        tmpN += $"{DiscordEmoji.FromName(ctx.Client, item.emoji.Trim())}{item.name}\n";
                        tmpW += $"{item.worth}\n";
                        ch = amountList.Length + nameList.Length + worthList.Length;
                        c++;
                    }
                    else
                    {
                        text.Add(new Tuple<string, string, string>(tmpA, tmpN, tmpW));
                        tmpA = $"{item.amount}\n";
                        tmpN = $"{DiscordEmoji.FromName(ctx.Client, item.emoji.Trim())}{item.name}\n";
                        tmpW = $"{item.worth}\n";
                        c = 1;
                    }

                }

                text.Add(new Tuple<string, string, string>(tmpA, tmpN, tmpW));

                for (int i = 0; i <= text.Count; i++)
                {
                    var embeded = new DiscordEmbedBuilder()
                    {
                        Title = $"{(i > 0 ? string.Empty : "The contents of your inventory:")}  Page {i + 1}/{text.Count}"
                    };

                    embeded.AddField("Amount", text[i].Item1, true);
                    embeded.AddField("Itemname", text[i].Item2, true);
                    embeded.AddField("Worth", text[i].Item3, true);

                    await ctx.Channel.SendMessageAsync(embeded).ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Please register first.").ConfigureAwait(false);
            }
        }

        [Command("EquipBait")]
        [Hidden]
        public async Task EquipBait(CommandContext ctx)
        {
            int succes = 0;
            string name = string.Empty;

            Player player = checkPlayer((long)ctx.Member.Id);
            if (player.id == -1)
            {
                await ctx.Channel.SendMessageAsync("Please register first.").ConfigureAwait(false);
                return;
            }
            else if ((bool)player.locked)
            {
                await ctx.Channel.SendMessageAsync("You can't do that at this moment").ConfigureAwait(false);
                return;
            }

            lockPlayer(player.id);

            List<Item> availableBait = selectItemsByTypePlayer(player.id, 2);

            if(availableBait.Count > 0)
            {
                var dummy = string.Empty;
                List<string> emojiList = new List<string>();

                foreach (var bait in availableBait)
                {
                    dummy += $"{bait.amount}x{bait.name}\n";
                    emojiList.Add(bait.emoji);
                }
                                        
                var msg = await ctx.Channel.SendMessageAsync($"You have the following bait available:\n{dummy}\nPlease react with the emoji of the corresponding bait to equip it.").ConfigureAwait(false);

                foreach (var emoji in emojiList)
                {
                    await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emoji.Trim())).ConfigureAwait(false);
                    await Task.Delay(500);
                }

                var ia = ctx.Client.GetInteractivity();

                var result = await ia.WaitForReactionAsync(
                                    x => x.Message == msg &&
                                    x.User == ctx.User).ConfigureAwait(false);

                switch (result.Result.Emoji.Name)
                {
                    case "BaitEarthworms":
                        succes = updatePlayerBaitId(player.id, 22);
                        name = "Earthworms";
                        break;

                    case "BaitGrubs":
                        succes = updatePlayerBaitId(player.id, 23);
                        name = "Grubs";
                        break;

                    case "BaitLeeches":
                        succes = updatePlayerBaitId(player.id, 24);
                        name = "Leeches";
                        break;

                    default:
                        break;
                }

                if(succes > 0)
                {
                    await ctx.Channel.SendMessageAsync($"You now have equipped {name} as your bait").ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"Something went wrong, sorry...").ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"You have no bait available, please first get some bait").ConfigureAwait(false);
            }

            unlockPlayer(player.id);
        }

        [Command("ChangeIsland")]
        [Hidden]
        public async Task ChangeIsland(CommandContext ctx)
        {
            string name = string.Empty;
            string distance = string.Empty;
            string number = string.Empty;
            List<string> emojiList = new List<string>();
            int n = 1;

            Player player = checkPlayer((long)ctx.Member.Id);
            if (player.id == -1)
            {
                await ctx.Channel.SendMessageAsync("Please register first.").ConfigureAwait(false);
                return;
            }
            else if ((bool)player.locked)
            {
                await ctx.Channel.SendMessageAsync("You can't do that at this moment").ConfigureAwait(false);
                return;
            }

            lockPlayer(player.id);
            var availableIslands = getIslandPool((int)player.currentIslandId);

            foreach (var island in availableIslands)
            {
                name += $"{island.Item2}\n";
                distance += $"{island.Item3}\n";
                emojiList.Add($":{Functions.Functions.numberToWord(n)}:");
                number += $"{DiscordEmoji.FromName(ctx.Client, $":{Functions.Functions.numberToWord(n)}:")}\n";
                n++;
            }

            var embeded = new DiscordEmbedBuilder()
            {
                Title = "List of available islands:\n"
            };

            embeded.AddField("Emoji",  number, true);
            embeded.AddField("Islandname", name, true);
            embeded.AddField("Distance", distance, true);

            var msg = await ctx.Channel.SendMessageAsync(embeded).ConfigureAwait(false);

            foreach (var emoji in emojiList)
            {
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emoji)).ConfigureAwait(false);
                await Task.Delay(500);
            }

            await msg.CreateReactionAsync(DiscordEmoji.FromUnicode(ctx.Client, "❌")).ConfigureAwait(false);

            var ia = ctx.Client.GetInteractivity();

            var result = await ia.WaitForReactionAsync(
                                x => x.Message == msg &&
                                x.User == ctx.User).ConfigureAwait(false);

            var islandIndex = Functions.Functions.wordToNumber(result.Result.Emoji.Name);

            if (result.Result.Emoji.Name.Equals("❌"))
            {
                await ctx.Channel.SendMessageAsync($"You decide to cancel your plans on sailing the sea").ConfigureAwait(false);
                unlockPlayer(player.id);
                return;
            }

            await ctx.Channel.SendMessageAsync($"You set your sails to sail to {availableIslands[islandIndex].Item2}").ConfigureAwait(false);
            await Task.Delay(6000 * Int32.Parse(availableIslands[islandIndex].Item3));

            var islandId = updatePlayerIslandId(player.id, availableIslands[islandIndex].Item1);

            await ctx.Channel.SendMessageAsync($"You are now on {islandId.name}").ConfigureAwait(false);
            unlockPlayer(player.id);
        }
        


        [Command("Register")]
        [Hidden]
        public async Task Register(CommandContext ctx)
        {
            var result = registerPlayer((long)ctx.User.Id);

            //TODO Texte überarbeiten
            if(result > 0)
            {
                await ctx.Channel.SendMessageAsync("Du wurdest erfolgreich registriert.\nDu kannst nun fischen :)").ConfigureAwait(false);
            }
            else if(result == -1)
            {
                await ctx.Channel.SendMessageAsync("Du bist schon registriert").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg").ConfigureAwait(false);
            }
        }

        [Command("AddItem")]
        [Hidden]
        public async Task AddItem(CommandContext ctx, int itemId, int amount)
        {
            Player player = checkPlayer((long)ctx.Member.Id);
            int succes = -1;
            string msg = string.Empty;
            Item insertedItem = new Item();

            if (player.id > 0)
            {
                succes = insertPlayerInventory(player.id, itemId, 2, amount);
                insertedItem = selectItem(itemId);
                if (succes > 0)
                {
                    msg = $"{amount} {insertedItem.name} have been added to your inventory";
                }
                else
                {
                    msg = $"Couldn't add your item, sorry...";
                }
                await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Please register first.").ConfigureAwait(false);
            }
        }

        public static int insertPlayerInventory (int playerId, int itemId, int itemTypeId, int amount)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            int result = 0;
            int? tmpId = null;
            int? tmpItemTypeId = null;
            int? tmpItemId = null;

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();

                string checkIfExistsQuery = "SELECT id FROM Inventory WHERE playerId = @playerId AND itemTypeId = @itemTypeId AND itemId = @itemId";

                var checkIfExistsCmd = new SqlCommand(checkIfExistsQuery, conn);
                checkIfExistsCmd.CommandType = CommandType.Text;
                checkIfExistsCmd.Parameters.AddWithValue("@playerId", playerId);
                checkIfExistsCmd.Parameters.AddWithValue("@itemId", itemId);
                checkIfExistsCmd.Parameters.AddWithValue("@itemTypeId", itemTypeId);

                rdr = checkIfExistsCmd.ExecuteReader();
                while (rdr.Read())
                {
                    tmpId = (int)rdr["id"];
                    tmpItemId = (int)rdr["itemId"];
                    tmpItemTypeId = (int)rdr["itemTypeId"];
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();

                if (tmpId < 1 && tmpItemId < 1 && tmpItemTypeId < 1)
                {

                    string query = "INSERT INTO Inventory (id, playerId, itemId, itemTypeId, Amount) VALUES ((SELECT MAX(id) FROM Inventory + 1), @playerId, @itemId, @itemTypeId, @amount)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@playerId", playerId);
                    cmd.Parameters.AddWithValue("@itemId", itemId);
                    cmd.Parameters.AddWithValue("@itemTypeId", itemTypeId);
                    cmd.Parameters.AddWithValue("@amount", amount);

                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        result = (int)rdr[0];
                    }
                }
                else
                {
                    string query = "UPDATE Inventory SET amount = amount + @amount WHERE playerId = @playerId AND itemTypeId = @itemTypeId AND itemId = @itemId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@playerId", playerId);
                    cmd.Parameters.AddWithValue("@itemId", itemId);
                    cmd.Parameters.AddWithValue("@itemTypeId", itemTypeId);
                    cmd.Parameters.AddWithValue("@amount", amount);

                    rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        result = (int)rdr[0];
                    }
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return result;
        }

        public static Tuple<int, int> addPlayerXp(int playerId, int xp)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            Tuple<int, int> result = new Tuple<int, int>(0,0);

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("dbo.updatePlayerXp", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@playerId", playerId));
                cmd.Parameters.Add(new SqlParameter("@xp", xp));
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    result = new Tuple<int, int>((int)rdr["result"], (int)rdr["level"]);
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return result;
        }

        public static int registerPlayer(long userId)
        {


            SqlConnection conn = null;
            SqlDataReader rdr = null;
            int result = 0;

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();

                //Check if Player already exists
                var checkPlayerSQL = new SqlCommand("SELECT id FROM Player WHERE userId = @userId", conn);
                checkPlayerSQL.CommandType = CommandType.Text;
                checkPlayerSQL.Parameters.AddWithValue("@userId", userId);

                int playerId = 0;

                rdr = checkPlayerSQL.ExecuteReader();
                while (rdr.Read())
                {
                    playerId = (int)rdr["id"];
                }

                if (playerId  < 1)
                {

                    var Player = new Player()
                    {
                        level = 0,
                        userId = userId,
                        encumbrance = 0,
                        gold = 200,
                        doubloon = 10,
                        equippedBaitId = 0,
                        equippedRodId = 0,
                        currentIslandId = 0,
                        locked = false
                    };

                    SqlCommand cmd = new SqlCommand("INSERT INTO Player (Id, Encumbrance, Level, Gold, Doubloon, EquippedBaitId, EquippedRodId, CurrentIslandId, Locked, UserId) VALUES ((SELECT MAX(id) FROM Player + 1), @Encumbrance, @Level, @Gold, @Doubloon, @EquippedBaitId, @EquippedRodId, @CurrentIslandId, @Locked, @UserId)", conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@id", Player.id);
                    cmd.Parameters.AddWithValue("@Encumbrance", Player.encumbrance);
                    cmd.Parameters.AddWithValue("@Level", Player.level);
                    cmd.Parameters.AddWithValue("@Gold", Player.gold);
                    cmd.Parameters.AddWithValue("@Doubloon", Player.doubloon);
                    cmd.Parameters.AddWithValue("@EquippedBaitId", Player.equippedBaitId);
                    cmd.Parameters.AddWithValue("@EquippedRodId", Player.equippedRodId);
                    cmd.Parameters.AddWithValue("@CurrentIslandId", Player.currentIslandId);
                    cmd.Parameters.AddWithValue("@Locked", Player.locked);
                    cmd.Parameters.AddWithValue("@UserId", Player.userId);

                    result = cmd.ExecuteNonQuery();
                }
                else
                {
                    result = -1;
                }

            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return result;
        }

        public static Player checkPlayer(long userId)
        {

            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlDataReader rdr = null;
            Player player = new Player();

            try
            {
                string query = "SELECT Id, Encumbrance, Level, Gold, Doubloon, EquippedBaitId, CurrentIslandId, Locked FROM Player WHERE UserId = @userId";
                var cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.CommandTimeout = 1000;

                rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                {

                    while (rdr.Read())
                    {
                        player = new Player
                        {
                            id = (int)rdr["Id"],
                            encumbrance = rdr["Encumbrance"].Equals(DBNull.Value) ? 0 : (decimal)rdr["Encumbrance"],
                            level = rdr["Level"].Equals(DBNull.Value) ? 0 : (int)rdr["Level"],
                            gold = rdr["Gold"].Equals(DBNull.Value) ? 0 : (int)rdr["Gold"],
                            doubloon = rdr["Doubloon"].Equals(DBNull.Value) ? 0 : (int)rdr["Doubloon"],
                            equippedBaitId = rdr["EquippedBaitId"].Equals(DBNull.Value) ? 0 : (int)rdr["EquippedBaitId"],
                            // equippedRodId = rdr["EquippedRodId"].Equals(DBNull.Value) ? 0 : (int)rdr["EquippedRodId"],
                            currentIslandId = rdr["CurrentIslandId"].Equals(DBNull.Value) ? 0 : (int)rdr["CurrentIslandId"],
                            locked = rdr["Locked"].Equals(DBNull.Value) ? false : (bool)rdr["Locked"],
                            userId = userId

                        };
                    }
                }
            }
            catch (Exception e)
            {
                var dummy = e.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return player;
        }

        public static int updatePlayerBaitId(int playerId, int baitId)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            int result = -1;

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("dbo.updatePlayerBaitId", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@playerId", playerId));
                if(baitId == -1)
                {
                    cmd.Parameters.Add(new SqlParameter("@itemId", DBNull.Value)); 
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@itemId", baitId));
                }
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    result = (int)rdr[0];
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return result;
        }

        public static Island updatePlayerIslandId(int playerId, int islandId)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            Island result = new Island();

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("dbo.updatePlayerIslandId", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@playerId", playerId));
                cmd.Parameters.Add(new SqlParameter("@islandId", islandId));
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Island tmpIsland = new Island
                    {
                        id = (int)rdr["id"],
                        name = rdr["name"].Equals(DBNull.Value) ? string.Empty : rdr["name"].ToString(),
                        regionId = rdr["regionId"].Equals(DBNull.Value) ? 0 : (int)rdr["regionId"],
                        x = rdr["x"].Equals(DBNull.Value) ? 0 : (int)rdr["x"],
                        y = rdr["y"].Equals(DBNull.Value) ? 0 : (int)rdr["y"],
                        earth = rdr["earth"].Equals(DBNull.Value) ? 0 : (int)rdr["earth"],
                        sand = rdr["sand"].Equals(DBNull.Value) ? 0 : (int)rdr["sand"],
                        shore = rdr["shore"].Equals(DBNull.Value) ? 0 : (int)rdr["shore"]
                    };
                    result = tmpIsland;
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return result;
        }

        public static List<Item> selectItemsByTypePlayer(int playerId, int itemTypeId)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            List<Item> returnItems = new List<Item>();

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("dbo.selectItemsByTypePlayer", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@playerId", playerId));
                cmd.Parameters.Add(new SqlParameter("@itemTypeId", itemTypeId));
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Item tmpItem = new Item
                    {
                        id = (int)rdr["id"],
                        amount = rdr["amount"].Equals(DBNull.Value) ? 0 : (int)rdr["amount"],
                        name = rdr["name"].ToString(),
                        description = rdr["description"].ToString(),
                        itemTypeId = rdr["itemTypeId"].Equals(DBNull.Value) ? 0 : (int)rdr["itemTypeId"],
                        weight = rdr["weight"].Equals(DBNull.Value) ? 0 : (int)rdr["weight"],
                        worth = rdr["worth"].Equals(DBNull.Value) ? 0 : (int)rdr["worth"],
                        currencyId = rdr["currencyId"].Equals(DBNull.Value) ? 0 : (int)rdr["currencyId"],
                        baitId = rdr["baitId"].Equals(DBNull.Value) ? 0 : (int)rdr["baitId"],
                        regionId = rdr["regionId"].Equals(DBNull.Value) ? 0 : (int)rdr["regionId"],
                        imageURL = rdr["imageURL"].Equals(DBNull.Value) ? string.Empty : rdr["imageURL"].ToString(),
                        percentage = rdr["percentage"].Equals(DBNull.Value) ? 0 : (decimal)rdr["percentage"],
                        emoji = rdr["emoji"].Equals(DBNull.Value) ? string.Empty : rdr["emoji"].ToString(),
                        xp = rdr["xp"].Equals(DBNull.Value) ? 0 : (int)rdr["xp"]
                    };
                    returnItems.Add(tmpItem);
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return returnItems;
        }

        public static List<Item> selectItemsByPlayer(int playerId)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            List<Item> returnItems = new List<Item>();

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("dbo.selectItemsByPlayer", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@playerId", playerId));
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Item tmpItem = new Item
                    {
                        id = (int)rdr["id"],
                        amount = rdr["amount"].Equals(DBNull.Value) ? 0 : (int)rdr["amount"],
                        name = rdr["name"].ToString(),
                        description = rdr["description"].ToString(),
                        itemTypeId = rdr["itemTypeId"].Equals(DBNull.Value) ? 0 : (int)rdr["itemTypeId"],
                        weight = rdr["weight"].Equals(DBNull.Value) ? 0 : (int)rdr["weight"],
                        worth = rdr["worth"].Equals(DBNull.Value) ? 0 : (int)rdr["worth"],
                        currencyId = rdr["currencyId"].Equals(DBNull.Value) ? 0 : (int)rdr["currencyId"],
                        baitId = rdr["baitId"].Equals(DBNull.Value) ? 0 : (int)rdr["baitId"],
                        regionId = rdr["regionId"].Equals(DBNull.Value) ? 0 : (int)rdr["regionId"],
                        imageURL = rdr["imageURL"].Equals(DBNull.Value) ? string.Empty : rdr["imageURL"].ToString(),
                        percentage = rdr["percentage"].Equals(DBNull.Value) ? 0 : (decimal)rdr["percentage"],
                        emoji = rdr["emoji"].Equals(DBNull.Value) ? string.Empty : rdr["emoji"].ToString(),
                        xp = rdr["xp"].Equals(DBNull.Value) ? 0 : (int)rdr["xp"]
                    };
                    returnItems.Add(tmpItem);
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return returnItems;
        }

        public static Item selectItem(int itemId)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            Item returnItem = null;

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT id, name, description, itemTypeId, weight, worth, currencyId, baitId, regionId, imageURL, percentage, emoji, xp FROM Item WHERE id = @itemId", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@itemId", itemId);
                
                rdr = cmd.ExecuteReader();
                
                while (rdr.Read())
                {
                    Item tmpItem = new Item
                    {
                        id = (int)rdr["id"],
                        name = rdr["name"].ToString(),
                        description = rdr["description"].ToString(),
                        itemTypeId =  rdr["itemTypeId"].Equals(DBNull.Value) ? 0 : (int)rdr["itemTypeId"],
                        weight = rdr["weight"].Equals(DBNull.Value) ? 0 : (int)rdr["weight"],
                        worth = rdr["worth"].Equals(DBNull.Value) ? 0 : (int)rdr["worth"],
                        currencyId = rdr["currencyId"].Equals(DBNull.Value) ? 0 : (int)rdr["currencyId"],
                        baitId = rdr["baitId"].Equals(DBNull.Value) ? 0 : (int)rdr["baitId"],
                        regionId = rdr["regionId"].Equals(DBNull.Value) ? 0 : (int)rdr["regionId"],
                        imageURL = rdr["imageURL"].Equals(DBNull.Value) ? string.Empty : rdr["imageURL"].ToString(),
                        percentage = rdr["percentage"].Equals(DBNull.Value) ? 0 : (decimal)rdr["percentage"],
                        emoji = rdr["emoji"].Equals(DBNull.Value) ? string.Empty : rdr["emoji"].ToString(),
                        xp = rdr["xp"].Equals(DBNull.Value) ? 0 : (int)rdr["xp"]
                    };
                    returnItem = tmpItem;
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return returnItem;
        }

        public static List<Tuple<int, string, string>> getIslandPool(int currentIsland, int distance = 0)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            var islandPool = new List<Tuple<int, string, string>>();

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = new SqlCommand("dbo.getIslandPool", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@islandId", currentIsland));
                cmd.Parameters.Add(new SqlParameter("@distance", distance));
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var tmpIsland = new Tuple<int, string, string>((int)rdr["id"], rdr["name"].ToString(), rdr["distance"].ToString());
                    islandPool.Add(tmpIsland);
                }
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return islandPool;
        }

        public static List<Fish> getFishPool(int island, int? bait)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
            var fishPool = new List<Fish>();
            var tmpFishFishList = new List<Fish>();

            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();

                //string query = "SELECT id, name, percentage, imageURL FROM Fish WHERE islandId = @islandId AND baitId = @baitId";

                //SqlCommand cmd = new SqlCommand(query, conn);
                //cmd.CommandType = CommandType.Text;
                //cmd.Parameters.AddWithValue("@islandId", island);
                //cmd.Parameters.AddWithValue("@baitId", bait);

                //rdr = cmd.ExecuteReader();

                //while (rdr.Read())
                //{
                //    Fish tmpFisch = new Fish
                //    {
                //        id = (int)rdr["id"],
                //        name = rdr["name"].ToString(),
                //        percentage = (decimal)rdr["percentage"],
                //        imageURL = rdr["imageURL"].ToString()
                //    };
                //    fishPool.Add(tmpFisch);
                //}

                //var tmpItemFishList = new List<Fish>();

                //string itemQuery = "SELECT id, name, percentage, imageURL FROM Item WHERE itemTypeId = 1";

                //var itemCmd = new SqlCommand(itemQuery, conn);
                //itemCmd.CommandType= CommandType.Text;

                //rdr = itemCmd.ExecuteReader();

                //while (rdr.Read())
                //{
                //    Fish tmpItemFish = new Fish
                //    {
                //        id = 0,
                //        name = null,
                //        percentage = 0,
                //        imageURL = null
                //    };
                //    tmpItemFishList.Add(tmpItemFish);
                //}

                string fishQuery = "SELECT id, name FROM Fish WHERE islandId = @islandId AND baitId = @baitId";

                var fishCmd = new SqlCommand(fishQuery, conn);
                fishCmd.CommandType = CommandType.Text;
                fishCmd.Parameters.AddWithValue("@islandId", island);
                fishCmd.Parameters.AddWithValue("@baitId", bait);

                rdr = fishCmd.ExecuteReader();

                while (rdr.Read())
                {
                    Fish tmpFishFish = new Fish
                    {
                        id = (int)rdr["id"],
                        name = rdr["name"].ToString()
                    };
                    tmpFishFishList.Add(tmpFishFish);
                }

            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }

            try 
            {

                conn = new SqlConnection(connStr);
                conn.Open();

                string nameStr = "";

                foreach (var fish in tmpFishFishList)
                {
                    nameStr += $"'{fish.name}', ";
                }

                nameStr = nameStr.Substring(0, nameStr.Length - 2);

                string itemQuery = $"SELECT id, name, percentage, imageURL FROM Item WHERE name IN ({nameStr})";

                var itemCmd = new SqlCommand(itemQuery, conn);
                itemCmd.CommandType = CommandType.Text;

                rdr = itemCmd.ExecuteReader();

                while(rdr.Read())
                {
                    Fish tmpItemFish = new Fish
                    {
                        //id = 0,
                        //name = null,
                        //percentage = 0,
                        //imageURL = null

                        id = (int)rdr["id"],
                        name = rdr["name"].ToString(),
                        percentage = (decimal)rdr["percentage"],
                        imageURL = rdr["imageURL"].ToString()
                    };
                    fishPool.Add(tmpItemFish);
                }

                //for (int i = 0;  i < tmpItemFishList.Count; i++)
                //{
                //    if (tmpItemFishList[i].name == tmpFishFishList[i].name) 
                //    {
                //        fishPool.Add(tmpItemFishList[i]);
                //    }
                //}

            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return fishPool;
        }

        public static void lockPlayer(int playerId)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;
           // var fishPool = new List<Fish>();

            try
            {

                //Update Player
                //  SET loked = 1
                //WHERE id = @playerId

                conn = new SqlConnection(connStr);
                conn.Open();

                string query = "UPDATE Player SET locked = 1 WHERE id = @playerId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@playerId", playerId);

                rdr = cmd.ExecuteReader();
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return;
        }

        public static void unlockPlayer(int playerId)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;

            //Update Player
            //  SET loked = 0
            //WHERE id = @playerId


            try
            {
                conn = new SqlConnection(connStr);
                conn.Open();

                string query = "UPDATE Player SET locked = 0 WHERE id = @playerId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@playerId", playerId);

                rdr = cmd.ExecuteReader();
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return;
        }



    }


}