﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;
using DiscordBot.Commands;
using DiscordBot.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DiscordBot.JsonClasses;
using DSharpPlus.CommandsNext.Attributes;

namespace DiscordBot
{

    class Bot
    {
        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension SlashCommands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public async Task RunAsync(ConfigJson config)
        {
            //Discord Client config
            var discordConfig = new DiscordConfiguration
            {
                Token = config.BotConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = config.BotConfig.AutoReconnect,
                Intents = DiscordIntents.All | DiscordIntents.MessageContents
            };

            //Initialize Discord Client
            Client = new DiscordClient(discordConfig);

            #region Events
            //Get Client Interactivity
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            //On new reaction
            Client.MessageReactionAdded += async (s, e) =>
            {
                //Grant Roles Anni Discord
                //Functions.Functions.GrantRolesByReaction(s, e);
                DiscordBot.SlashCommands.StandardCommands.QuoteReactionAdded(s, e);
            };

            //On removed reaction
            Client.MessageReactionRemoved += async (s, e) =>
            {
                //Revoke Roles Anni Discord
                Functions.Functions.RemoveRolesByReaction(s, e);
                DiscordBot.SlashCommands.StandardCommands.QuoteReactionRemoved(s, e);
            };

            //On message created
            Client.MessageCreated += async (s, e) =>
            {
                //if (e.Message.Content.ToLower().Contains("datboi") && e.Author.Id != 853976207493038090)
                //{
                //    await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/916043520610017301/943957313314754580/shitsfire.jpg");
                //}
            };

            ////On button press
            //Client.ComponentInteractionCreated += async (s, e) =>
            //{
            //    //Functions.Functions.ButtonPressed(s, e); 
            //};

            //On dropdown selection
            Client.ComponentInteractionCreated += async (s, e) =>
            {
                //Grant roles
                Functions.Functions.GrantRolesByDropDown(s, e);
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            };

            #endregion

            //Discord Command config
            var cmdConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { config.BotConfig.Prefix },
                EnableMentionPrefix = config.CommandConfig.EnableMentionPrefix,
                EnableDms = config.CommandConfig.EnableDms,
                IgnoreExtraArguments = config.CommandConfig.IgnoreExtraArguments
            };

            //Initialize Commands in Client
            Commands = Client.UseCommandsNext(cmdConfig);

            //Register Command Modules
            Commands.RegisterCommands<Commands.StandardCommands>();
            Commands.RegisterCommands<FishCommands>();
            Commands.RegisterCommands<TwitterCommands>();
            Commands.RegisterCommands<AnniCommands>();

            //Discord Command config
            var slscmdConfig = new SlashCommandsConfiguration
            {

            };

            //Initialize Slashcommands in Client
            SlashCommands = Client.UseSlashCommands(slscmdConfig);

            //Register Command Modules
            SlashCommands.RegisterCommands<SlashCommands.StandardCommands>();

            //Connect Client
            await Client.ConnectAsync();

            //Endless Task Delay
            await Task.Delay(-1);
        }
    }
}