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
        public Roles Roles { get; set; }
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

    public class Roles
    {
        public ulong ClanLeader { get; set; }
        public ulong ClanMember { get; set; }
    }
}
