using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using System;
using System.Threading.Tasks;
using DiscordBotTest.Commands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using DiscordBotTest.JasonClasses;

namespace DiscordBotTest
{

    class Bot
    {
        

        public DiscordClient Client{ get; set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public async Task RunAsync(ConfigJson config)
        {
            //Discord Client config
            var discordConfig = new DiscordConfiguration
            {
                Token = config.BotConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = config.BotConfig.AutoReconnect,
                Intents = DiscordIntents.All
            };
            //Initialize Discord Client
            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            //Discord Command config
            var cmdConfig = new CommandsNextConfiguration {
                StringPrefixes = new string[] { config.BotConfig.Prefix },
                EnableMentionPrefix = config.CommandConfig.EnableMentionPrefix,
                EnableDms = config.CommandConfig.EnableDms,
                IgnoreExtraArguments = config.CommandConfig.IgnoreExtraArguments
            };
            //Initialize Commands in Client
            Commands = Client.UseCommandsNext(cmdConfig);

            //Register Command Modules
            Commands.RegisterCommands<StandardCommands>();
            Commands.RegisterCommands<FishCommands>();

            //Eventhandling
            //Client.MessageReactionAdded += OnReactionAdded;
            //Client.DmChannelCreated += NewDM;

            //Connect Client
            await Client.ConnectAsync();

            //Endless Task Delay
            await Task.Delay(-1);
        }


        //private async Task OnReactionAdded(DiscordClient sender, MessageReactionRemoveEventArgs e)
        //{
        //    var messageId = e.Message.Id;
        //    var guild = e.Message.Channel.Guild;
        //    var reactionName = e.Emoji.GetDiscordName();

        //    var reactionDetail = ReactionDetails.FirstOrDefault(x =>
        //        x.MessageId == messageId
        //        && x.GuildId == guild.Id
        //        && x.ReactionName == reactionName);

        //    if (reactionDetail != null)
        //    {
        //        var member = e.User as DiscordMember;
        //        if (member != null)
        //        {
        //            var role = guild.Roles.FirstOrDefault(x => x.Value.Id == reactionDetail.RoleId).Value;
        //            await member.GrantRoleAsync(role).ConfigureAwait(false);
        //        }
        //    }
        //}


    }
}
