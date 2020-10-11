using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CountingBot.DatabaseManager;

namespace CountingBot.Modules
{
    public class GetUserCount : ModuleBase<SocketCommandContext>
    {
        [Command("getcount")]
        [Alias("get-count")]
        public async Task GetCountAsync()
        {
            if (Context.User is SocketGuildUser user)
            {
                await GetCountAsync(user);
            }
        }

        [Command("getcount")]
        [Alias("get-count")]
        public async Task GetCountAsync(SocketGuildUser user)
        {
            Task<int> count = countingDatabase.UserCounts.GetUserCountAsync(user);

            List<(SocketGuildUser user, int count)> userCounts = await countingDatabase.UserCounts.GetAllUserCountsAsync(Context.Guild);
            int userRank = 1 + userCounts.IndexOf((user, await count));

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"{user.Mention} has sent {await count} messages in the counting channel.\n" +
                    $"Rank: {userRank}");

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
