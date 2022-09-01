using Discord;
using Discord.Interactions;
using System.Threading.Tasks;

namespace CountingBot.Modules
{
    public class Help : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("help", "List of commands")]
        public async Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName);

            EmbedFieldBuilder prefix = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Prefix")
                .WithValue("\\" +
                    "\n**or**\n" +
                    Context.Client.CurrentUser.Mention + "\n\u200b");
            embed.AddField(prefix);

            EmbedFieldBuilder field = new EmbedFieldBuilder()
                .WithIsInline(false)
                .WithName("Commands")
                .WithValue(
                    "ping\n" +
                    "  - Returns the bot's Server and API latencies\n\n" +
                    "set-channel [channel]\n" +
                    "  - Sets the counting channel\n\n" +
                    "get-count [user (optional)]\n" +
                    "  - Gets the number of counting messages sent by the user and their rank on the leaderboard\n\n" +
                    "counter\n" +
                    "  - Gets a leaderboard of the top 5 users\n\n" +
                    "current-count\n" +
                    "  - Gets the current counter value"
                );
            embed.AddField(field);

            await Context.Interaction.RespondAsync(embed: embed.Build());
        }
    }
}