using Discord;
using Discord.Interactions;
using System.Threading.Tasks;
using static CountingBot.DatabaseManager;

namespace CountingBot.Modules
{
    public class CurrentCount : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("current-count", "Gets the current counter value")]
        [RequireContext(ContextType.Guild)]
        public async Task CurrentCountAsync()
        {
            int count = await countingDatabase.Channels.GetCountAsync(Context.Guild).ConfigureAwait(false);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithCurrentTimestamp()
                .WithDescription($"The counter is at {count}.\n" +
                    $"Looking for: {count + 1}");

            await Context.Interaction.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}