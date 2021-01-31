using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using DiscordBotTest.Commands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

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
            Commands.RegisterCommands<SqlCommand>();
            Commands.RegisterCommands<DatabaseCommands>();

            //Connect Client
            await Client.ConnectAsync();
            
            //Endless Task Delay
            await Task.Delay(-1);
        }
    }
}
