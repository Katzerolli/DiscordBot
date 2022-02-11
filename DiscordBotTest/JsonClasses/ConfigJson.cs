using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;

namespace DiscordBot.JsonClasses
{
    public class ConfigJson
    {
        public BotConfig BotConfig { get; set; }
        public CommandConfig CommandConfig { get; set; }
        public StoredValues StoredValues { get; set; }
        public TwitterValues TwitterValues { get; set; }
    }

    public  class BotConfig
    {
        public bool AutoReconnect { get; set; }
        public string Prefix { get; set; }
        public string Token { get; set; }
    }

    public class CommandConfig
    {
        public bool CaseSensitive { get; set; }
        public bool DmHelp { get; set; }
        public bool EnableDefaultHelp { get; set; }
        public bool EnableDms { get; set; }
        public bool EnableMentionPrefix { get; set; }
        public bool IgnoreExtraArguments { get; set; }
        public bool UseDefaultCommandHandler { get; set; }
    }

    public class StoredValues
    {
        public string DefaultColor { get; set; }
        public ulong EmojiPlusOne { get; set; }
        public ulong EmojiYes { get; set; }
        public ulong EmojiNo { get; set; }
        public ulong EmojiLoading { get; set; }
    }

    public class TwitterValues
    {
        public string BearerToken { get; set; }
        public string APIKey { get; set; }
        public string APISecret { get; set; }
        public ulong GiveawayChannelId { get; set; }
    }
}
