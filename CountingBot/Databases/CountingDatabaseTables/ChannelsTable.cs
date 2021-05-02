using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace CountingBot.Databases.CountingDatabaseTables
{
    public class ChannelsTable : ITable
    {
        private readonly SqliteConnection connection;

        public ChannelsTable(SqliteConnection connection) => this.connection = connection;

        public Task InitAsync()
        {
            using SqliteCommand cmd = new("CREATE TABLE IF NOT EXISTS Channels (guild_id TEXT PRIMARY KEY, channel_id TEXT NOT NULL, count INTEGER NOT NULL);", connection);
            return cmd.ExecuteNonQueryAsync();
        }

        public async Task<SocketTextChannel> GetCountingChannelAsync(SocketGuild g)
        {
            SocketTextChannel channel = null;

            string getChannel = "SELECT channel_id FROM Channels WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getChannel, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _ = ulong.TryParse(reader["channel_id"].ToString(), out ulong channelID);
                channel = g.GetTextChannel(channelID);
            }
            reader.Close();

            return channel;
        }

        public async Task SetCountingChannelAsync(SocketTextChannel channel)
        {
            string update = "UPDATE Channels SET channel_id = @channel_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Channels (guild_id, channel_id, count) SELECT @guild_id, @channel_id, 0 WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", channel.Guild.Id.ToString());
            cmd.Parameters.AddWithValue("@channel_id", channel.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveCountingChannelAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Channels WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(delete, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetCountAsync(SocketGuild g)
        {
            int count = 0;

            string getCount = "SELECT count FROM Channels WHERE guild_id = @guild_id;";

            using SqliteCommand cmd = new(getCount, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            SqliteDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                _ = int.TryParse(reader["count"].ToString(), out count);
            }
            reader.Close();

            return count;
        }

        public async Task IncrementCountAsync(SocketGuild g)
        {
            string update = "UPDATE Channels SET count = count + 1 WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Channels (guild_id, channel_id, count) SELECT @guild_id, @channel_id, 1 WHERE (SELECT Changes() = 0);";

            using SqliteCommand cmd = new(update + insert, connection);
            cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
