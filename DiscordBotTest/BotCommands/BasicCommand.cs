using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotTest.Commands
{
    public class BasicCommand : BaseCommandModule
    {
        [Command("IsOwner")]
        public async Task IsOwner(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(ctx.Member.IsOwner.ToString()).ConfigureAwait(false);
        }

        [Command("GetRoles")]
        public async Task GetRoles(CommandContext ctx)
        {
            var roles = string.Empty;

            foreach(var r in ctx.Member.Roles)
            {
                roles += "Rolename: " + r.Name + "\tRoleId: " + r.Id + "\n";
            }

            await ctx.Channel.SendMessageAsync(roles).ConfigureAwait(false);
        }




    }
}
