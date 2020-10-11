using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static CountingBot.DatabaseManager;

namespace CountingBot.Modules
{
    public class SetChannel : ModuleBase<SocketCommandContext>
    {
        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync()
        {
            if (await countingDatabase.Channels.GetCountingChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("You already do not have a channel set.");
                return;
            }

            await Task.WhenAll
            (
                countingDatabase.Channels.RemoveCountingChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync("Counting messages will no longer be managed.")
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(SocketTextChannel channel)
        {
            if (await countingDatabase.Channels.GetCountingChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"{channel.Mention} is already configured for counting messages.");
                return;
            }

            await Task.WhenAll
            (
                countingDatabase.Channels.SetCountingChannelAsync(channel),
                Context.Channel.SendMessageAsync($"Counting messages will now be managed in {channel.Mention}.")
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(string channel)
        {
            SocketTextChannel c;
            if (ulong.TryParse(channel, out ulong channelID) && (c = Context.Guild.GetTextChannel(channelID)) != null)
            {
                await SetChannelAsync(c);
                return;
            }
            await Context.Channel.SendMessageAsync("Error: the given text channel does not exist.");
        }
    }
}
