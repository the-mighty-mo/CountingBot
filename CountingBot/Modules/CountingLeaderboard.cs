using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CountingBot.DatabaseManager;

namespace CountingBot.Modules
{
    public class CountingLeaderboard : ModuleBase<SocketCommandContext>
    {
        [Command("counter")]
        public async Task CountingLeaderboardAsync()
        {
            List<(SocketGuildUser user, int count)> userCounts = await countingDatabase.UserCounts.GetAllUserCountsAsync(Context.Guild);
            List<(SocketGuildUser user, int count)> topFive = userCounts.Take(5).ToList();

            string leaderboard = "";
            int rank = 1;
            foreach ((SocketGuildUser user, int count) user in topFive)
            {
                leaderboard += $"{rank} - {user.user.Mention}: {user.count}\n";
                rank++;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithCurrentTimestamp();

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Counting Leaderboard")
                .WithValue(leaderboard);
            embed.AddField(field);

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
