using Discord;
using Discord.Interactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CountingBot.Modules
{
    public class Help : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("help", "List of commands")]
        public Task HelpAsync()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithTitle(SecurityInfo.botName);

            List<EmbedFieldBuilder> fields = new();

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
            fields.Add(field);
            embed.WithFields(fields);

            return Context.Interaction.RespondAsync(embed: embed.Build(), ephemeral: true);
        }
    }
}