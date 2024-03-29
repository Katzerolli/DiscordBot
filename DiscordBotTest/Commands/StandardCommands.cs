﻿ using System;
using System.IO;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using System.Linq;
using Microsoft.Data.Sqlite;
using RestSharp;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Drawing;
using DiscordBot.JsonClasses;
using System.Security.Policy;
using System.Threading.Channels;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices.Marshalling;
using static System.Net.WebRequestMethods;

namespace DiscordBot.Commands
{

    public class StandardCommands : BaseCommandModule
    {
        //Read config file
        private readonly ConfigJson config = Functions.Functions.ReadConfig();

        private static readonly string dblocation = $@"{Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName}\Milfbase.db";
        
        private readonly RestClientOptions rOptions = new RestClientOptions() { MaxTimeout = -1 };

        #region TestCommands

        [Command("SetStatus")]
        [Hidden]
        public async Task SetStatus(CommandContext ctx, string type = "Watching", [RemainingText] string status = "over the seven Seas")
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
        }
        #endregion

        #region Role Commands

        [Command("CreateRole")]
        [Hidden]
        [Description("Erstellt Rolle und weist diese direkt dem Ausführenden zu")]
        public async Task CreateRole(CommandContext ctx,
                                    [Description("Name der Rolle")] string name,
                                    [Description("Farbe als Hex oder Name")] string color = "#9c59b6")
        {

            DiscordColor roleColor;
            if (color.Substring(0, 1).Equals("#"))
            {
                roleColor = new DiscordColor(color);
            }
            else
            {
                roleColor = new DiscordColor(Functions.Functions.GetHexFromColor(color));
            }
            var role = await ctx.Guild.CreateRoleAsync(name, color: roleColor);

            await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);

        }

        [Command("CreateRoleAndChannel")]
        [Hidden]
        public async Task CreateRoleAndChannel(CommandContext ctx,
                                              [Description("Name der Rolle")] string name,
                                              [Description("Farbe als Hex oder Name")] string color,
                                              [Description("[Optional] ChannelTyp => Text, Voice (Standard), Private")] string type = "Voice")
        {
            DiscordColor roleColor;
            if (color.Substring(0, 1).Equals("#"))
            {
                roleColor = new DiscordColor(color);
            }
            else
            {
                roleColor = new DiscordColor(Functions.Functions.GetHexFromColor(color));
            }

            ChannelType channelType = ChannelType.Unknown;
            switch (type)
            {
                case "Text":
                    channelType = ChannelType.Text;
                    break;
                case "Voice":
                    channelType = ChannelType.Voice;
                    break;
                case "Private":
                    channelType = ChannelType.Private;
                    break;
            }

            var role = await ctx.Guild.CreateRoleAsync(name, color: roleColor).ConfigureAwait(false);
            var channel = await ctx.Guild.CreateChannelAsync(name, channelType).ConfigureAwait(false);

            await channel.AddOverwriteAsync(role, Permissions.All).ConfigureAwait(false);
            await channel.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.All).ConfigureAwait(false);
            await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
        }

        [Command("AddUserToRole")]
        [Hidden]
        [Description("Erstellt Rolle und weist diese direkt dem Ausführenden zu")]
        public async Task AddUserToRole(CommandContext ctx,
                                        [Description("Rollen ID")] ulong roleId,
                                        [Description("Userid(s) der User")][RemainingText] string userIds)
        {
            DiscordRole role = ctx.Guild.GetRole(roleId);

            List<string> userlist;
            DiscordMember member;

            if (userIds.Contains(","))
            {
                userlist = userIds.Split(',').ToList();
                foreach (var user in userlist)
                {
                    member = await ctx.Guild.GetMemberAsync(Convert.ToUInt64(user)).ConfigureAwait(false);
                    await member.GrantRoleAsync(role).ConfigureAwait(false);
                }
            }
            else
            {
                member = await ctx.Guild.GetMemberAsync(Convert.ToUInt64(userIds)).ConfigureAwait(false);
                await member.GrantRoleAsync(role).ConfigureAwait(false);
            }

        }

        [Command("AddChannelToRole")]
        [Hidden]
        [Description("Erstellt Rolle und weist diese direkt dem Ausführenden zu")]
        public async Task AddChannelToRole(CommandContext ctx,
                                [Description("Rollen ID")] ulong roleId,
                                [Description("[Optional] ChannelTyp => Text, Voice (Standard), Private")] string type = "Voice")
        {
            DiscordRole role = ctx.Guild.GetRole(roleId);

            ChannelType channelType = ChannelType.Unknown;
            switch (type)
            {
                case "Text":
                    channelType = ChannelType.Text;
                    break;
                case "Voice":
                    channelType = ChannelType.Voice;
                    break;
                case "Private":
                    channelType = ChannelType.Private;
                    break;
            }

            var channel = await ctx.Guild.CreateChannelAsync(role.Name, channelType).ConfigureAwait(false);

            await channel.AddOverwriteAsync(role, Permissions.All).ConfigureAwait(false);
            await channel.AddOverwriteAsync(ctx.Guild.EveryoneRole, deny: Permissions.All).ConfigureAwait(false);

        }

        [Command("DeleteRole")]
        [Description("Löscht die angegebene Rolle, benötigt min. Moderatorrechte")]
        [Hidden]
        public async Task DeleteRole(CommandContext ctx,
                                    [Description("Die zu löschende RollenId")] ulong roleId)
        {
            DiscordRole role = ctx.Guild.GetRole(roleId);
            await role.DeleteAsync().ConfigureAwait(false);
        }


        [Command("RemoveUserFromRole")]
        [Description("Entfernt einem User die angegebene Rolle, benötigt min. Moderatorrechte")]
        [Hidden]
        public async Task RemoveRole(CommandContext ctx, ulong userId, ulong roleId)
        {
            DiscordRole role = ctx.Guild.GetRole(roleId);
            DiscordMember member = await ctx.Guild.GetMemberAsync(userId).ConfigureAwait(false);
            await member.GrantRoleAsync(role).ConfigureAwait(false);
        }

        #endregion

        #region Text Commands

        [Command("PostText")]
        [Hidden]
        public async Task PostText(CommandContext ctx, string category)
        {
            var temp = Functions.Functions.GetCategoryText(ctx, category);
            var text = Functions.Functions.DiscordFormat(ctx, temp);
            await ctx.Channel.SendMessageAsync(text).ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("EditText")]
        [Hidden]
        public async Task EditText(CommandContext ctx, string category, [RemainingText] string text)
        {
            Functions.Functions.EditCategoryText(ctx, text, category);
            await ctx.Channel.SendMessageAsync($"Die Kategorie {category} wurde in Datenbank aktualisiert").ConfigureAwait(false);
        }

        [Command("AddText")]
        [Hidden]
        public async Task AddText(CommandContext ctx, string category, [RemainingText] string text)
        {
            Functions.Functions.AddCategoryText(ctx, text, category);
            await ctx.Channel.SendMessageAsync($"Neue Kategorie {category} in Datenbank angelegt").ConfigureAwait(false);
        }

        #endregion

        #region MILF Commands
        [Command("PostAnwärter")]
        [Hidden]
        [Description("Erstellt und postet den Anwärtertext")]
        public async Task PostAnwärter(CommandContext ctx,
                                        [Description("Die UserId des Anwärters")] ulong userId,
                                        [Description("Anmerkung über den Anwärter")][RemainingText] string anmerkung)
        {
            if (ctx.Guild.Id.Equals(848588654020132874)) //Nur für Milf Discord
            {
                DateTime inaktivität = DateTime.Now;
                DiscordMessage msg;
                var text = $"<@&848640163797794828>\nAbstimmung für den Anwärter: <@{userId}>\nAnmerkung: {anmerkung}\nEntfernung bei Inaktivität: {inaktivität.AddDays(14):dd.MM.yyyy}";
                msg = await ctx.Channel.SendMessageAsync(text).ConfigureAwait(false);

                DiscordEmoji emojiPlusOne = await ctx.Guild.GetEmojiAsync(config.StoredValues.EmojiPlusOne).ConfigureAwait(false);
                DiscordEmoji emojiYes = await ctx.Guild.GetEmojiAsync(config.StoredValues.EmojiYes).ConfigureAwait(false);
                DiscordEmoji emojiNo = await ctx.Guild.GetEmojiAsync(config.StoredValues.EmojiNo).ConfigureAwait(false);
                DiscordEmoji emojiLoading = await ctx.Guild.GetEmojiAsync(config.StoredValues.EmojiLoading).ConfigureAwait(false);
                DiscordEmoji emojiTop = DiscordEmoji.FromUnicode("💯");

                await msg.CreateReactionAsync(emojiPlusOne).ConfigureAwait(false);
                await msg.CreateReactionAsync(emojiYes).ConfigureAwait(false);
                await msg.CreateReactionAsync(emojiNo).ConfigureAwait(false);
                await msg.CreateReactionAsync(emojiLoading).ConfigureAwait(false);
                await msg.CreateReactionAsync(emojiTop).ConfigureAwait(false);

                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }

        }

        [Command("PostAchievement")]
        [Description("Hiermit können bei freien Plätzen \"Events\" wie Holztruhen, FotD, FoF etc gepingt & zeitlich begrenzt werden.")]
        public async Task PostAchievement(CommandContext ctx,
                                            [Description("Typ des Achievements (holz, chantal, grog, fof, fotd)")] string typ,
                                            [Description("Die Zeit bis die Nachricht \"gelöscht\" wird")] int zeit,
                                            [Description("Sonstige Anmerkung")][RemainingText] string anmerkung = "")
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                string text;
                string eventName;
                DiscordMessage msg;

                switch (typ)
                {
                    case "holz":
                        eventName = "Holzauftrag";
                        break;

                    case "chantal":
                        eventName = "Chest of Sorrow";
                        break;

                    case "grog":
                        eventName = "Chest of Thousand Grogs";
                        break;

                    case "fof":
                        eventName = "Fort of Fortune";
                        break;

                    case "fotd":
                        eventName = "Fort of the Damned";
                        break;

                    default:
                        text = $"Ich konnte leider nicht finden was du meinst, bitte versuche es mit einem anderen Typ erneut";
                        msg = await ctx.Channel.SendMessageAsync(text).ConfigureAwait(false);
                        await Task.Delay(zeit * 10000).ConfigureAwait(true);
                        await msg.DeleteAsync().ConfigureAwait(false);
                        return;
                }

                text = $"<@&890311556204748810>\n1x {eventName} ist für die nächste(n) {zeit} Minute(n) verfügaber.\n" +
                        $"Bei Interesse bei {ctx.Member.Mention} melden. :slight_smile:{(anmerkung.Equals(string.Empty) ? "" : "\n" + anmerkung)}";
                msg = await ctx.Channel.SendMessageAsync(text).ConfigureAwait(false);

                await ctx.Message.DeleteAsync().ConfigureAwait(false);
                await Task.Delay(zeit * 60000).ConfigureAwait(true);
                await msg.ModifyAsync($"~~{msg.Content}~~").ConfigureAwait(false);
            }
        }
        #endregion

        [Command("xkcd")]
        [Hidden]
        [Description("Postet einen xkcd Comic")]
        public async Task PostComic(CommandContext ctx,
                                [Description("Die Comicnummer")] int number = -1)
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                string uri = string.Empty;
                string messageText;

                if (number == -1)
                {
                    uri = "https://xkcd.com/info.0.json";
                }
                else if (number == 0)
                { 
                    var rClient = new RestClient(rOptions);
                    var rRequest = new RestRequest("https://xkcd.com/info.0.json", Method.Get);
                    RestResponse rResponse = rClient.Execute(rRequest);

                    if (rResponse.IsSuccessful)
                    {
                        var rSerilizedXkcd = JsonConvert.DeserializeObject<xkcd>(rResponse.Content);
                        Random rnd = new Random();
                        int cNr = rnd.Next(1, rSerilizedXkcd.num);
                        uri = $"https://xkcd.com/{cNr}/info.0.json";
                    }
                    else
                    {
                        messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                    }
                }
                else
                {
                    uri = $"https://xkcd.com/{number}/info.0.json";
                }

                var client = new RestClient(rOptions);
                var request = new RestRequest(uri, Method.Get);
                RestResponse response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    var serilizedXkcd = JsonConvert.DeserializeObject<xkcd>(response.Content);
                    messageText = serilizedXkcd.img;
                }
                else
                {
                    messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                }

                await ctx.Channel.SendMessageAsync(messageText).ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }

        [Command("Hund")]
        [Description("What the dog doin? Guess you have to find out...")]
        public async Task Hund(CommandContext ctx)
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                string messageText;
                string randomDog;

                var client = new RestClient(rOptions);
                var request = new RestRequest("https://random.dog/woof", Method.Get);
                RestResponse randomDogRequest = client.Execute(request);

                if (randomDogRequest.IsSuccessful)
                {
                    randomDog = randomDogRequest.Content;
                    messageText = $"https://random.dog/{randomDog}";
                }
                else
                {
                    messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                }

                await ctx.Channel.SendMessageAsync(messageText).ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }

        [Command("Katze")]
        [Description(@"*Miau* /ᐠ｡▿｡ᐟ\*ᵖᵘʳʳ*")]
        public async Task Katze(CommandContext ctx)
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                string messageText;

                var client = new RestClient(rOptions);
                var request = new RestRequest("https://api.thecatapi.com/v1/images/search", Method.Get);
                RestResponse randomCatRequest = client.Execute(request);

                if (randomCatRequest.IsSuccessful)
                {
                    var strippedResponse = randomCatRequest.Content.Substring(1, randomCatRequest.Content.Length - 2);
                    var serilizedCat = JsonConvert.DeserializeObject<cat>(strippedResponse);
                    messageText = serilizedCat.url;
                }
                else
                {
                    messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                }

                var msg = await ctx.Channel.SendMessageAsync(messageText).ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }
        
        [Command("Echse")]
        [Description("random.exe")]
        public async Task Echse(CommandContext ctx)
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {

                string messageText;

                var client = new RestClient(rOptions);
                var request = new RestRequest("https://nekos.life/api/v2/img/lizard", Method.Get);
                RestResponse randomLizardRequest = client.Execute(request);

                if (randomLizardRequest.IsSuccessful)
                {
                    var serilizedLizard = JsonConvert.DeserializeObject<echse>(randomLizardRequest.Content);
                    messageText = serilizedLizard.url;
                }
                else
                {
                    messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                }

                var msg = await ctx.Channel.SendMessageAsync(messageText).ConfigureAwait(false);
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }

        [Command("DuKek")]
        [Hidden]
        public async Task Hurensohn(CommandContext ctx)
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {

                //var token = "a26f2eb96ff8a00e0e2bdbfb6239dd236dbf3292013a4412723a553b61f3a4a3eabde36c20eee96bb87a29889f44b8478b55d7048547bc4146712fe006304730";

                var client = new RestClient("https://v1.api.amethyste.moe/generate/batslap");
                //var request = new RestRequest(Method.POST);
                //request.AddHeader("Content-Type", "application/json");
                //request.AddHeader("Authorization", $"Bearer {token}");

                JObject jObjectbody = new JObject();
                jObjectbody.Add("avatar", "https://content1.promiflash.de/article-images/video_480/carsten-stahl-laechelt.jpg");
                jObjectbody.Add("url", ctx.User.AvatarUrl);

                //request.AddParameter("application/json", jObjectbody, ParameterType.RequestBody);


                //var clientValue = client.Execute<HttpWebResponse>(request);

                //var dummy = clientValue.RawBytes;

                //using (Image image = Image.FromStream(new MemoryStream(dummy)))
                //{
                //    image.Save("C:\\Users\\daboni\\Desktop\\Neuer Ordner\\output2.png", System.Drawing.Imaging.ImageFormat.Png);
                //}

                //TODO Image per Api hochladen und dann im embedded einbinden
                var builder = new DiscordEmbedBuilder()
                {
                    Description = $"<@{ctx.User.Id}> Selber Hurensohn",
                    ImageUrl = "attachment:C:\\Users\\daboni\\Desktop\\Neuer Ordner\\output.png"
                    //"https://randomwordgenerator.com/img/picture-generator/54e5d2464f55a914f1dc8460962e33791c3ad6e04e50744077297bd5954ec6_640.jpg"
                };

                builder.Title = "Bastard";

                await ctx.Channel.SendMessageAsync(builder).ConfigureAwait(false);
                await Task.Delay(10000);
                await ctx.Member.SetMuteAsync(true).ConfigureAwait(false); //.RemoveAsync().ConfigureAwait(false);
            }
        }

        #region Hentai
        [Command("HentaiV1")]
        [Hidden]
        [Description("Api wurde vorerst deaktiviert, funktioniert also momentan nicht")]
        public async Task HentaiV1(CommandContext ctx,
                                [Description("Unterstützte Kategorien: ass, bdsm, cum, creampie, manga, femdom, hentai, incest, masturbation, public, ero, orgy, elves, yuri, pantsu, glasses, cuckold, blowjob, " +
                                            "boobjob, foot, thighs, vagina, ahegao, uniform, gangbang, tentacles, gif, neko, nsfwMobileWallpaper, zettaiRyouiki")]string category = "")
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                if (ctx.Channel.IsNSFW || ctx.Member.Id == 326776845171294209)
                {
                    if (category.Equals(string.Empty))
                    {
                        var rnd = new Random();
                        string[] categoryArray = { "ass", "bdsm", "cum", "creampie", "manga", "femdom", "hentai", "incest", "masturbation", "public", "ero", "orgy", "elves", "yuri", "pantsu", "glasses", "cuckold",
                                                "blowjob", "boobjob", "foot", "thighs", "vagina", "ahegao", "uniform", "gangbang", "tentacles", "gif", "neko", "nsfwMobileWallpaper", "zettaiRyouiki" };
                        category = categoryArray[rnd.Next(0, 29)];
                    }

                    var messageText = string.Empty;
                    hentaiV1 hentai = new hentaiV1();
                    var url = $"https://hmtai.herokuapp.com/nsfw/{category}";

                    var client = new RestClient(rOptions);
                    var request = new RestRequest(url, Method.Get);
                    RestResponse randomHentaiRequest = client.Execute(request);

                    if (randomHentaiRequest.IsSuccessful)
                    {
                        hentai = JsonConvert.DeserializeObject<hentaiV1>(randomHentaiRequest.Content);
                        messageText = hentai.url;
                    }
                    else
                    {
                        messageText = "https://i.kym-cdn.com/entries/icons/original/000/033/758/Screen_Shot_2020-04-28_at_12.21.48_PM.png";
                    }

                    var msg = await ctx.Channel.SendMessageAsync(messageText).ConfigureAwait(false);
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Befehl kann nur in NSFW Channel ausgeführt werden").ConfigureAwait(false);
                }
            }
        }

        [Command("HentaiV2")]
        [Hidden]
        [Description("You naughty naughty you teasing me you naughty naughty")]
        public async Task HentaiV2(CommandContext ctx, 
                                [Description("femdom, classic, feet, feetg, lewd, nsfw_neko_gif, kuni, tits, boobs, pussy_jpg, pussy, cum_jpg, cum, spank, hentai, nsfw_avatar, solo, solog, blowjob, bj, yuri, les, " +
                                "trap, anal, gasm, futanari, pwankg, ero, eroyuri, eron, erofeet, hololewd, lewdk")]string category = "")
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {

                if (ctx.Channel.IsNSFW || ctx.Member.Id == 326776845171294209)
                {
                    if (category.Equals(string.Empty))
                    {
                        category = "Random_hentai_gif";
                    }

                    var messageText = string.Empty;
                    hentaiV2 hentai = new hentaiV2();
                    var url = $"https://nekos.life/api/v2/img/{category}";

                    var client = new RestClient(rOptions);
                    var request = new RestRequest(url, Method.Get);
                    RestResponse randomHentaiRequest = client.Execute(request);

                    if (randomHentaiRequest.IsSuccessful)
                    {
                        hentai = JsonConvert.DeserializeObject<hentaiV2>(randomHentaiRequest.Content);
                        messageText = hentai.url;
                    }
                    else
                    {
                        messageText = "https://i.kym-cdn.com/entries/icons/original/000/033/758/Screen_Shot_2020-04-28_at_12.21.48_PM.png";
                    }

                    var msg = await ctx.Channel.SendMessageAsync(messageText).ConfigureAwait(false);
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Befehl kann nur in NSFW Channel ausgeführt werden").ConfigureAwait(false);
                }
            }
        }

        [Command("Hentai")]
        [Hidden]
        [Description("You naughty naughty you teasing me you naughty naughty")]
        public async Task HentaiV3(CommandContext ctx,
                        [Description("ahegao_avatar, femdom, cosplay, classic_lewd, classic, feet_lewd, feet, neko_lewd, neko_ero, neko, kuni, tits_lewd, tits, pussy_lewd, pussy, cum_lewd, cum, spank, ero, lewd, solo, solo_girl, bj_lewd, bj, yuri_lewd, " +
                                        "yuri, trap, anal_lewd, wallpaper_ero, wallpaper_lewd, anus, anal, futanari, pussy_wank, bdsm, yuri_ero, feet_ero, holo_lewd, holo_avatar, holo_ero, kitsune_lewd, kitsune_ero, kemonomimi_lewd, kemonomimi_ero, pantyhose_lewd, " +
                                        "pantyhose_ero, piersing_lewd, piersing_ero, peeing, keta, smalboobs, keta_avatar, yiff_lewd, yiff")] string category = "")
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                if (ctx.Channel.IsNSFW || ctx.Member.Id == 326776845171294209)
                {
                    if (category.Equals(string.Empty))
                    {
                        category = "random";
                    }

                    var endpoint = Functions.Functions.categoryToEndpoint(category);

                    var messageText = string.Empty;
                    hentaiV3 hentai = new hentaiV3();
                    var url = $"https://api.nekos.dev/api/v3/images/{endpoint}";

                    var client = new RestClient(rOptions);
                    var request = new RestRequest(url, Method.Get);
                    RestResponse randomHentaiRequest = client.Execute(request);

                    if (randomHentaiRequest.IsSuccessful)
                    {
                        hentai = JsonConvert.DeserializeObject<hentaiV3>(randomHentaiRequest.Content);
                        messageText = hentai.data.response.url;
                    }
                    else
                    {
                        messageText = "https://i.kym-cdn.com/entries/icons/original/000/033/758/Screen_Shot_2020-04-28_at_12.21.48_PM.png";
                    }

                    var msg = await ctx.Channel.SendMessageAsync(messageText).ConfigureAwait(false);
                    await ctx.Message.DeleteAsync().ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Befehl kann nur in NSFW Channel ausgeführt werden").ConfigureAwait(false);
                }
            }
        }
        #endregion

        [Command("CSGO")]
        [Hidden]
        [Description("Gibt CSGO Inventory Inhalt aus")]
        public async Task CSGO(CommandContext ctx, long steamId = 76561198122925075)
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                string messageText = string.Empty;
                List<string> text = new List<string>();

                var client = new RestClient(rOptions);
                var request = new RestRequest("https://steamcommunity.com/inventory/{steamId}/730/2?l=english", Method.Get);
                RestResponse randomLizardRequest = client.Execute(request);

                if (randomLizardRequest.IsSuccessful)
                {
                    var inv = JsonConvert.DeserializeObject<CSGOInv>(randomLizardRequest.Content);
                    List<Description> descriptions = inv.descriptions;
                    var ch = 0;
                    foreach (var item in descriptions)
                    {
                        if (ch < 950)
                        {
                            messageText += $"{item.name}\n";
                            ch = messageText.Length;
                        }
                        else
                        {
                            text.Add(messageText);
                            messageText = $"{item.name}\n";
                            ch = messageText.Length;

                        }

                    }
                    foreach (var t in text)
                    {
                        await ctx.Channel.SendMessageAsync(t).ConfigureAwait(false);
                    }


                }
                else
                {
                    messageText = "https://img.pr0gramm.com/2021/08/25/40bb76ab3c2c6bee.jpg";
                }


            }
        }

        [Command("Buttons")]
        [Hidden]
        [Description("Buttons")]
        public async Task Buttons(CommandContext ctx)
        {

            var myButton = new DiscordButtonComponent(ButtonStyle.Primary, "emoji_button", null, false, new DiscordComponentEmoji(944192261510561832));

            var builder = new DiscordMessageBuilder()
                .WithContent("This message has buttons! Pretty neat innit?")
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Primary, "1_top", "Blurple!"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "2_top", "Grey!"),
                    new DiscordButtonComponent(ButtonStyle.Danger, "4_top", "Red!"),
                    new DiscordLinkButtonComponent("https://some-super-cool.site", "Link!")
                });

            await ctx.Channel.SendMessageAsync(builder);


        }

        [Command("Dropdown")]
        [Hidden]
        [Description("Dropdown")]
        public async Task Dropdown(CommandContext ctx)
        {
            try
            {
                var roles = ctx.Guild.Roles.Where(x => x.Value.Color.Value == 12745742);

                // Create the options for the user to pick
                var options = new List<DiscordSelectComponentOption>();

                foreach (var role in roles)
                {
                    options.Add(
                        new DiscordSelectComponentOption(
                        role.Value.Name,
                        role.Value.Tags.ToString(),
                        role.Value.Tags.ToString()));

                }

                //,

                //    new DiscordSelectComponentOption(
                //        "LOL",
                //        "lol",
                //        "Liga der Legenden",
                //        false,
                //        new DiscordComponentEmoji("🔮")),

                //    new DiscordSelectComponentOption(
                //        "COD",
                //        "cod",
                //        "sweaty tryhard",
                //        false,
                //        new DiscordComponentEmoji("🔫"))
                //};

                // Make the dropdown
                var dropdown = new DiscordSelectComponent("dropdown", null, options, false, 1, 2);
                var builder = new DiscordMessageBuilder()
                    .WithContent("Bitte Rolle auswählen!")
                    .AddComponents(dropdown);

                await builder.SendAsync(ctx.Channel);

            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message).ConfigureAwait(true);
            
            }

        }

        [Command("Quote")]
        [Description("gets a random anime quote")]
        public async Task Dummy(CommandContext ctx, string anime = "", string character = "")
        {
            if (Functions.Functions.OnlyPlebhunter((long)ctx.Guild.Id))
            {
                bool err = false;
                string messageText;
                var builder = new DiscordEmbedBuilder();
                string url;
                if (anime == "" && character == "")
                {
                    url = "https://animechan.vercel.app/api/random";
                }
                else if (anime != "" && character == "")
                {
                    url = $"https://animechan.vercel.app/api/random/anime?title={anime}";
                }
                else if (character != "" && anime == "")
                {
                    url = $"https://animechan.vercel.app/api/random/character?name={character}";
                }
                else
                {
                    url = "https://animechan.vercel.app/api/random";
                    err = true;
                }

                var client = new RestClient(rOptions);
                var request = new RestRequest(url, Method.Get);
                RestResponse randomAnimeRequest = client.Execute(request);

                if (randomAnimeRequest.IsSuccessful)
                {
                    var randomQuote = JsonConvert.DeserializeObject<RandomQuote>(randomAnimeRequest.Content);
                    messageText = randomQuote.quote.ToString();
                    messageText += $"\n - {randomQuote.character}";
                    messageText += $"\n Anime: {randomQuote.anime}";
                    if (err)
                    {
                        messageText = "You cannot specify an anime and character";
                    }

                    builder = new DiscordEmbedBuilder()
                    {
                        //Optional color
                        Color = DiscordColor.Aquamarine,
                        Description = "Random Anime Quote"
                    };

                    builder.AddField("Quote", messageText);
                }
                else
                {
                    messageText = "failed";
                }

                //Sended eine Nachricht
                await ctx.Channel.SendMessageAsync(builder).ConfigureAwait(false);
                
                //Löscht die Auslösungsnachricht
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
        }





    }
}
