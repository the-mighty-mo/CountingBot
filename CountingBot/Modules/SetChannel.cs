using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Tasks;
using static CountingBot.DatabaseManager;

namespace CountingBot.Modules
{
    public class SetChannel : InteractionModuleBase<SocketInteractionContext>
    {

        [SlashCommand("set-channel", "Sets the counting channel")]
        [RequireContext(ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(SocketTextChannel channel = null)
        {
            if (channel is null)
            {
                await SetChannelPrivAsync();
            }
            else
            {
                await SetChannelPrivAsync(channel);
            }
        }

        public async Task SetChannelPrivAsync()
        {
            if (await countingDatabase.Channels.GetCountingChannelAsync(Context.Guild) == null)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription("You already do not have a channel set.");

                await Context.Interaction.RespondAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription("Counting messages will no longer be managed.");

            await Task.WhenAll
            (
                countingDatabase.Channels.RemoveCountingChannelAsync(Context.Guild),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }

        public async Task SetChannelPrivAsync(SocketTextChannel channel)
        {
            if (await countingDatabase.Channels.GetCountingChannelAsync(Context.Guild) == channel)
            {
                EmbedBuilder emb = new EmbedBuilder()
                    .WithColor(SecurityInfo.botColor)
                    .WithDescription($"{channel.Mention} is already configured for counting messages.");

                await Context.Interaction.RespondAsync(embed: emb.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(SecurityInfo.botColor)
                .WithDescription($"Counting messages will now be managed in {channel.Mention}.");

            await Task.WhenAll
            (
                countingDatabase.Channels.SetCountingChannelAsync(channel),
                Context.Interaction.RespondAsync(embed: embed.Build())
            );
        }
    }
}