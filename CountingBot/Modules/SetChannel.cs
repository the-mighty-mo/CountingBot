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
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("You already do not have a channel set.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("Counting messages will no longer be managed.");

            await Task.WhenAll
            (
                countingDatabase.Channels.RemoveCountingChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync(embed: embed.Build())
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
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{channel.Mention} is already configured for counting messages.");

                await Context.Channel.SendMessageAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"Counting messages will now be managed in {channel.Mention}.");

            await Task.WhenAll
            (
                countingDatabase.Channels.SetCountingChannelAsync(channel),
                Context.Channel.SendMessageAsync(embed: embed.Build())
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