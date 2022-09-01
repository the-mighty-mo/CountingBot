using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CountingBot.DatabaseManager;

namespace CountingBot.Modules
{
    public class GetUserCount : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("get-count", "Gets the number of counting messages sent by the user and their rank on the leaderboard")]
        [RequireContext(ContextType.Guild)]
        public async Task GetCountAsync(SocketGuildUser user = null)
        {
            if (user is null)
            {
                if (Context.User is SocketGuildUser u)
                {
                    user = u;
                }
                else
                {
                    return;
                }
            }

            Task<int> count = countingDatabase.UserCounts.GetUserCountAsync(user);

            List<(SocketGuildUser user, int count)> userCounts = await countingDatabase.UserCounts.GetAllUserCountsAsync(Context.Guild);
            int rank = 1 + userCounts.IndexOf((user, await count));
            string rankString = rank switch
            {
                1 => ":first_place:",
                2 => ":second_place:",
                3 => ":third_place:",
                _ => rank.ToString()
            };

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"{user.Mention} has sent {await count} messages in the counting channel.\n" +
                    $"Rank: {rankString}");

            if (rank > 1)
            {
                int countAbove = userCounts[rank - 2].count;
                embed.Description += $"\nUntil next: {countAbove - await count}";
            }

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}