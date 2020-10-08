using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

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
            if (await GetCountingChannelAsync(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("You already do not have a channel set.");
                return;
            }

            await Task.WhenAll
            (
                RemoveCountingChannelAsync(Context.Guild),
                Context.Channel.SendMessageAsync("Counting messages will no longer be managed.")
            );
        }

        [Command("setchannel")]
        [Alias("set-channel")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(SocketTextChannel channel)
        {
            if (await GetCountingChannelAsync(Context.Guild) == channel)
            {
                await Context.Channel.SendMessageAsync($"{channel.Mention} is already configured for counting messages.");
                return;
            }

            await Task.WhenAll
            (
                SetCountingChannelAsync(channel),
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

        public static async Task<SocketTextChannel> GetCountingChannelAsync(SocketGuild g)
        {
            SocketTextChannel channel = null;

            string getChannel = "SELECT channel_id FROM Channels WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getChannel, Program.cnCounting))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                    channel = g.GetTextChannel(channelID);
                }
                reader.Close();
            }

            return channel;
        }

        public static async Task SetCountingChannelAsync(SocketTextChannel channel)
        {
            string update = "UPDATE Channels SET channel_id = @channel_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Channels (guild_id, channel_id, count) SELECT @guild_id, @channel_id, 0 WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnCounting))
            {
                cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
                cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveCountingChannelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Channels WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnCounting))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<int> GetCountAsync(SocketGuild g)
        {
            int count = 0;

            string getCount = "SELECT count FROM Channels WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getCount, Program.cnCounting))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int.TryParse(reader["count"].ToString(), out count);
                }
                reader.Close();
            }

            return count;
        }

        public static async Task IncrementCountAsync(SocketGuild g)
        {
            string update = "UPDATE Channels SET count = count + 1 WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Channels (guild_id, channel_id, count) SELECT @guild_id, @channel_id, 1 WHERE (SELECT Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnCounting))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
