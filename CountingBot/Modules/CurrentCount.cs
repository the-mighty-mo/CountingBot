using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace CountingBot.Modules
{
    public class CurrentCount : ModuleBase<SocketCommandContext>
    {
        [Command("currentcount")]
        [Alias("current-count")]
        public async Task CurrentCountAsync()
        {
            int count = await SetChannel.GetCountAsync(Context.Guild);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithCurrentTimestamp()
                .WithDescription($"The counter is at {count}.\n" +
                    $"Looking for: {count + 1}");

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}
