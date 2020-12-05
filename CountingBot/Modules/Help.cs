using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace CountingBot.Modules
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
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
                    "setchannel [channel mention/channel ID]\n" +
                    "  - Sets the counting channel\n\n" +
                    "getcount [user mention/user ID (optional)]\n" +
                    "  - Gets the number of counting messages sent by the user and their rank on the leaderboard\n\n" +
                    "counter\n" +
                    "  - Gets a leaderboard of the top 5 users\n\n" +
                    "currentcount\n" +
                    "  - Gets the current counter value"
                );
            embed.AddField(field);

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}