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
            IEnumerable<(SocketGuildUser user, int count)> topFive = userCounts.Take(5);

            string leaderboard = "";
            int rank = 1;
            foreach ((SocketGuildUser user, int count) user in topFive)
            {
                string rankString = rank switch
                {
                    1 => ":first_place:",
                    2 => ":second_place:",
                    3 => ":third_place:",
                    _ => $"\u200b {rank} \u200b \u200b"
                };
                leaderboard += $"{rankString} - {user.user.Mention}: {user.count}\n";
                rank++;
            }

            if (leaderboard == "")
            {
                leaderboard = "no users have sent a message";
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