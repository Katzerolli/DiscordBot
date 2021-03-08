using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;

namespace DiscordBotTest
{
    public class ConfigJson
    {
        public BotConfig BotConfig { get; set; }
        public CommandConfig CommandConfig { get; set; }
        public StoredValues StoredValues { get; set; }
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
        public ulong ClanSortRoleId { get; set; }
        public ulong ClanRoleId { get; set; }
        public ulong ClanLeaderRoleId { get; set; }
        public ulong ModRoleId { get; set; }
        public string DefaultColor { get; set; }
    }
}
